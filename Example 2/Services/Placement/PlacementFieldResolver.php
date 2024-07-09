<?php

namespace App\Services\Placement;

use App\DTO\AdDataPayload;
use App\DTO\AdMediaDTO;
use App\Models\Enum\AdTypeEnum;
use App\Models\Enum\PlacementEnum;
use FacebookAds\Object\Fields\AdAssetFeedSpecFields;
use FacebookAds\Object\Fields\AdCreativeLinkDataCallToActionFields;
use FacebookAds\Object\Fields\AdCreativeLinkDataCallToActionValueFields;
use FacebookAds\Object\Fields\AdCustomizationRuleSpecFields;
use JetBrains\PhpStorm\ArrayShape;
use JetBrains\PhpStorm\Pure;

class PlacementFieldResolver
{
    public const ACTIVE_PLATFORMS = ['facebook', 'instagram'];
    public const AD_FORMATS = [
        'link' => 'SINGLE_IMAGE',
        'video' => 'SINGLE_VIDEO',
    ];

    public const MEDIA_TYPE = [
        'video' => 'video',
        'link' => 'image',
    ];

    public function __construct(
        private AssetFeedSpecContentService $assetFeedSpecContentService
    ) {}

    #[Pure]
    #[ArrayShape([
        AdAssetFeedSpecFields::AD_FORMATS => "string[]",
        AdAssetFeedSpecFields::IMAGES => "array",
        AdAssetFeedSpecFields::OPTIMIZATION_TYPE => "string",
        AdAssetFeedSpecFields::TITLES => "array[]",
        AdAssetFeedSpecFields::BODIES => "array[]",
        AdAssetFeedSpecFields::DESCRIPTIONS => "array[]",
        AdAssetFeedSpecFields::CALL_TO_ACTIONS => "array[]",
        AdAssetFeedSpecFields::LINK_URLS => "array[]",
        AdAssetFeedSpecFields::ASSET_CUSTOMIZATION_RULES => "array"])]
    public function getPlacementFields(AdDataPayload $adPayload): array
    {
        $mediaType = self::MEDIA_TYPE[$adPayload->getAdMedia()->first()->getType()];

        $media = $this->assetFeedSpecContentService->getAssetFeedMedia($adPayload);

        $customizationRules = $this->createCustomizationRules($adPayload, $mediaType, $media);

        return [
            AdAssetFeedSpecFields::AD_FORMATS => [self::AD_FORMATS[$adPayload->getAdMedia()->first()->getType()]],
            $mediaType === 'image' ? AdAssetFeedSpecFields::IMAGES : AdAssetFeedSpecFields::VIDEOS => array_values($media),
            AdAssetFeedSpecFields::OPTIMIZATION_TYPE => 'PLACEMENT',
            AdAssetFeedSpecFields::TITLES => [[ 'text' => $adPayload->getAdTemplate()->linkData->headline ]],
            AdAssetFeedSpecFields::BODIES => [[ 'text' => $adPayload->getAdTemplate()->linkData->primary_text ]],
            AdAssetFeedSpecFields::DESCRIPTIONS => [[ 'text' => $adPayload->getAdTemplate()->linkData->description ]],
            AdAssetFeedSpecFields::CALL_TO_ACTIONS => [[
                AdCreativeLinkDataCallToActionFields::TYPE => $adPayload->getAdTemplate()->callToAction->type,
                AdCreativeLinkDataCallToActionFields::VALUE => [
                    AdCreativeLinkDataCallToActionValueFields::LINK => $adPayload->getAdTemplate()->callToAction->link,
                ],
            ]],
            AdAssetFeedSpecFields::LINK_URLS => [
                [
                    'website_url' => $adPayload->getAdTemplate()->linkData->link,
                    'display_url' => $adPayload->getAdTemplate()->linkData->link,
                ]
            ],
            AdAssetFeedSpecFields::ASSET_CUSTOMIZATION_RULES => $customizationRules,
        ];
    }

    private function getPlacements(string $placement, string $platform): array
    {
        if ($placement === PlacementEnum::FEED->value) {
            if ($platform === 'facebook') {
                return ['feed'];
            }

            return ['stream', 'explore'];
        }

        if ($placement === PlacementEnum::STORY->value) {
            return ['story'];
        }

        return ['feed', 'story'];
    }

    #[Pure]
    private function createCustomizationRules(AdDataPayload $adPayload, string $mediaLabel, array $media): array
    {
        $customizationRules = [];
        $usePlacementsMap = [];

        foreach ($media as $placementKey => $placement) {
            foreach (self::ACTIVE_PLATFORMS as $platform) {
                //resolve placements for ad media
                $platformPlacement = $this->getPlacements($placementKey, $platform);
                $customizationRules[] = [
                    AdCustomizationRuleSpecFields::CUSTOMIZATION_SPEC => [
                        'publisher_platforms' => [
                            $platform,
                        ],
                        "{$platform}_positions" => $platformPlacement,
                    ],
                    "{$mediaLabel}_label" => [
                        'name' => $placement['adlabels'][0]['name'],
                    ],
                ];
                $usePlacementsMap[$platform] = array_merge($usePlacementsMap[$platform] ?? [], $platformPlacement);
            }
        }

        $defaultPositions = $this->createDefaultPositions($adPayload, $mediaLabel, $usePlacementsMap);

        return array_merge($customizationRules, $defaultPositions);
    }

    /**
     * @usage get default positions from config and fill in the remaining positions with the default media of this ad
     * @param AdDataPayload $adPayload
     * @param string $mediaLabel
     * @param array $usedPlacementsMap
     * @return array
     */
    private function createDefaultPositions(AdDataPayload $adPayload, string $mediaLabel, array $usedPlacementsMap): array
    {
        $defaultPositions = array_map(function ($position) use ($adPayload, $mediaLabel) {
            $defaultAdMedia = $adPayload->getAdMedia()->firstWhere(function (AdMediaDTO $element) {
                return $element->isDefault();
            });
            $position["{$mediaLabel}_label"]['name'] = $defaultAdMedia->getName() . '_' . $defaultAdMedia->getPlacement();
            return $position;
        }, config('fb.default_customizations'));

        //remove default placements that are used here
        foreach ($usedPlacementsMap as $platform => $usedPlacements) {
            $specKeyToModify = null;
            foreach ($defaultPositions as $key => $defaultPosition) {
                if (array_search($platform, $defaultPosition['customization_spec']['publisher_platforms']) !== false) {
                    $specKeyToModify = $key;
                    break;
                }
            }

            if ($specKeyToModify === null) continue;

            foreach ($usedPlacements as $usedPlacement) {
                if (($key = array_search($usedPlacement, $defaultPositions[$specKeyToModify]['customization_spec']["{$platform}_positions"])) !== false) {
                    unset($defaultPositions[$specKeyToModify]['customization_spec']["{$platform}_positions"][$key]);
                }
            }
        }

        return $defaultPositions;
    }
}
