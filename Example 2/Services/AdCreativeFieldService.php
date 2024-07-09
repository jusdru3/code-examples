<?php

namespace App\Services;

use App\DTO\AdDataPayload;
use App\Services\Placement\PlacementFieldResolver;
use App\Services\PostData\AdFieldResolverInterface;
use FacebookAds\Object\Fields\AdCreativeFields;
use FacebookAds\Object\Fields\AdCreativeObjectStorySpecFields;
use JetBrains\PhpStorm\ArrayShape;

class AdCreativeFieldService
{
    private AdFieldResolverInterface $fieldDataResolver;

    public function __construct(
        private PlacementFieldResolver $placementFieldResolver,
    ) {}

    public function setFieldDataResolver(AdFieldResolverInterface $fieldDataResolver): self
    {
        $this->fieldDataResolver = $fieldDataResolver;

        return $this;
    }

    #[ArrayShape([
        AdCreativeFields::NAME => "string",
        AdCreativeFields::URL_TAGS => "mixed",
        AdCreativeFields::OBJECT_STORY_SPEC => "array",
        AdCreativeFields::ASSET_FEED_SPEC => "array",
        AdCreativeFields::CALL_TO_ACTION_TYPE => "mixed"])]
    public function getExtraFields(AdDataPayload $adPayload): array
    {
        $adDataFields = $this->fieldDataResolver->getFields($adPayload);

        $placementFields = [];
        if ($adPayload->getAdMedia()->count() > 1) {
            $placementFields = $this->placementFieldResolver->getPlacementFields($adPayload);
        }

        $objectStorySpec = [
            AdCreativeObjectStorySpecFields::PAGE_ID => $adPayload->getPageId(),
            AdCreativeObjectStorySpecFields::INSTAGRAM_ACTOR_ID => $adPayload->getActorId(),
            $this->fieldDataResolver->getDataKey($adPayload) => $adDataFields,
        ];

        return [
            AdCreativeFields::NAME => $adPayload->getName(),
            AdCreativeFields::URL_TAGS => $adPayload->getAdTemplate()->url_tags,
            AdCreativeFields::OBJECT_STORY_SPEC => $objectStorySpec,
            AdCreativeFields::ASSET_FEED_SPEC => $placementFields,
        ];
    }
}
