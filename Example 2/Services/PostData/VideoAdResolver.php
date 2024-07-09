<?php

namespace App\Services\PostData;

use App\DTO\AdDataPayload;
use App\DTO\AdMediaDTO;
use FacebookAds\Object\AdVideo;
use FacebookAds\Object\Fields\AdCreativeLinkDataCallToActionFields;
use FacebookAds\Object\Fields\AdCreativeLinkDataCallToActionValueFields;
use FacebookAds\Object\Fields\AdCreativeObjectStorySpecFields;
use FacebookAds\Object\Fields\AdCreativeVideoDataFields;
use JetBrains\PhpStorm\Pure;

class VideoAdResolver implements AdFieldResolverInterface
{
    function getFields(AdDataPayload $adPayload): array
    {
        $adVideo = $this->getAdVideo($adPayload);
        $response = $adVideo->getThumbnails(
            [
                'uri',
            ]
        );

        $thumbnails = $response->getLastResponse()->getContent();

        return [
            AdCreativeVideoDataFields::IMAGE_URL => $thumbnails['data'][0]['uri'],
            AdCreativeVideoDataFields::VIDEO_ID => $adVideo->id,
            AdCreativeVideoDataFields::TITLE => $adPayload->getAdTemplate()->linkData->headline,
            AdCreativeVideoDataFields::LINK_DESCRIPTION => $adPayload->getAdTemplate()->linkData->description,
            AdCreativeVideoDataFields::MESSAGE => $adPayload->getAdTemplate()->linkData->primary_text,
            AdCreativeVideoDataFields::CALL_TO_ACTION => [
                AdCreativeLinkDataCallToActionFields::TYPE => $adPayload->getAdTemplate()->callToAction->type,
                AdCreativeLinkDataCallToActionFields::VALUE => [
                    AdCreativeLinkDataCallToActionValueFields::LINK => $adPayload->getAdTemplate()->callToAction->link,
                ],
            ],
        ];
    }

    #[Pure]
    function getDataKey(AdDataPayload $adPayload): string
    {
        return $adPayload->getAdMedia()->count() === 1 ? AdCreativeObjectStorySpecFields::VIDEO_DATA : AdCreativeObjectStorySpecFields::TEMPLATE_DATA;
    }

    function getAdVideo(AdDataPayload $adPayload): AdVideo
    {
        if ($adPayload->getAdMedia()->count() === 1) {
            return $adPayload->getAdMedia()->first()->getVideo();
        }
        return $adPayload->getAdMedia()->firstWhere(function (AdMediaDTO $element) {
            return $element->isDefault();
        })->getVideo();
    }
}
