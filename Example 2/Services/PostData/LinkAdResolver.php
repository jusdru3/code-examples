<?php

namespace App\Services\PostData;

use App\DTO\AdDataPayload;
use App\DTO\AdMediaDTO;
use FacebookAds\Object\Fields\AdCreativeLinkDataCallToActionFields;
use FacebookAds\Object\Fields\AdCreativeLinkDataCallToActionValueFields;
use FacebookAds\Object\Fields\AdCreativeLinkDataFields;
use FacebookAds\Object\Fields\AdCreativeObjectStorySpecFields;
use JetBrains\PhpStorm\Pure;

class LinkAdResolver implements AdFieldResolverInterface
{
    public function getFields(AdDataPayload $adPayload): array
    {
        return [
            AdCreativeLinkDataFields::IMAGE_HASH => $this->getImageHash($adPayload),
            AdCreativeLinkDataFields::NAME => $adPayload->getAdTemplate()->linkData->headline,
            AdCreativeLinkDataFields::LINK => $adPayload->getAdTemplate()->linkData->link,
            AdCreativeLinkDataFields::DESCRIPTION => $adPayload->getAdTemplate()->linkData->description,
            AdCreativeLinkDataFields::MESSAGE => $adPayload->getAdTemplate()->linkData->primary_text,
            AdCreativeLinkDataFields::CALL_TO_ACTION => [
                AdCreativeLinkDataCallToActionFields::TYPE => $adPayload->getAdTemplate()->callToAction->type,
                AdCreativeLinkDataCallToActionFields::VALUE => [
                    AdCreativeLinkDataCallToActionValueFields::LINK => $adPayload->getAdTemplate()->callToAction->link,
                ]
            ],
        ];
    }

    #[Pure]
    function getDataKey(AdDataPayload $adPayload): string
    {
        return $adPayload->getAdMedia()->count() === 1 ? AdCreativeObjectStorySpecFields::LINK_DATA : AdCreativeObjectStorySpecFields::TEMPLATE_DATA;
    }

    function getImageHash(AdDataPayload $adPayload): string
    {
        if ($adPayload->getAdMedia()->count() === 1) {
            return $adPayload->getAdMedia()->first()->getImageHash();
        }
        return $adPayload->getAdMedia()->firstWhere(function (AdMediaDTO $element) {
            return $element->isDefault();
        })->getImageHash();
    }
}
