<?php

namespace App\Services;

use App\DTO\AdDataPayload;
use App\Services\PostData\FieldDataResolverManager;
use FacebookAds\Object\Fields\AdFields;

class AdFieldService
{
    public function getAdFields(AdDataPayload $payload): array
    {
        $adMedia = $payload->getAdMedia();
        $fieldDataResolver = FieldDataResolverManager::getResolverByAdMedia($payload);

        /** @var AdCreativeFieldService $adCreativeFieldService */
        $adCreativeFieldService = app(AdCreativeFieldService::class)
            ->setFieldDataResolver($fieldDataResolver);

        $creativeFields = $adCreativeFieldService->getExtraFields($payload);

        return [
            AdFields::NAME => $adMedia[0]->getName() . ' - ' . $payload->getAdTemplate()->name,
            AdFields::ADSET_ID => $payload->getAdSetId(),
            AdFields::STATUS => $payload->getAdStatus(),
            AdFields::CREATIVE => $creativeFields,
            AdFields::CONVERSION_DOMAIN => $payload->getConversionDomain(),
        ];
    }
}
