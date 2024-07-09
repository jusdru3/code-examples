<?php

namespace App\Services\Placement;

use App\DTO\AdDataPayload;
use App\DTO\AdMediaDTO;
use App\Models\Enum\AdTypeEnum;

class AssetFeedSpecContentService
{
    public function getAssetFeedMedia(AdDataPayload $adDataPayload): array
    {
        if ($adDataPayload->getAdMedia()->first()->getType() === AdTypeEnum::VIDEO->value) {
            return $this->getAssetFeedVideos($adDataPayload);
        }

        return $this->getAssetFeedImages($adDataPayload);
    }

    public function getAssetFeedVideos(AdDataPayload $adDataPayload): array
    {
        $videos = [];

        $adMediaVideos = $adDataPayload->getAdMedia()->filter(function (AdMediaDTO $adMediaDTO) {
            return $adMediaDTO->getType() === AdTypeEnum::VIDEO->value;
        });

        foreach ($adMediaVideos as $adMedia) {
            if (!$adMedia->getPlacement()) {
                continue;
            }

            $newAdLabel =  ['name' => $adMedia->getName() . '_' . $adMedia->getPlacement()];

            $videos[$adMedia->getPlacement()] = [
                'adlabels' => [
                    $newAdLabel,
                ],
                'video_id' => $adMedia->getVideo()->id,
            ];
        }

        return $videos;
    }

    public function getAssetFeedImages(AdDataPayload $adDataPayload): array
    {
        $images = [];

        $adMediaImages = $adDataPayload->getAdMedia()->filter(function (AdMediaDTO $adMediaDTO) {
            return $adMediaDTO->getType() === AdTypeEnum::LINK->value;
        });

        foreach ($adMediaImages as $adMedia) {
            if (!$adMedia->getPlacement()) {
                continue;
            }

            $newAdLabel =  ['name' => $adMedia->getName() . '_' . $adMedia->getPlacement()];

            $images[$adMedia->getPlacement()] = [
                'adlabels' => [
                    $newAdLabel,
                ],
                'hash' => $adMedia->getImageHash(),
            ];
        }

        return $images;
    }
}
