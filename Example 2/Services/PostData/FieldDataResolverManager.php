<?php

namespace App\Services\PostData;

use App\DTO\AdDataPayload;
use App\DTO\AdMediaDTO;
use App\Models\Enum\AdTypeEnum;

class FieldDataResolverManager
{
    public static function getResolverByAdType(string $type): ?AdFieldResolverInterface
    {
        switch ($type) {
            case AdTypeEnum::LINK->value:
                return app(LinkAdResolver::class);
            case AdTypeEnum::VIDEO->value:
                return app(VideoAdResolver::class);
        }

        return null;
    }

    public static function getResolverByAdMedia(AdDataPayload $adDataPayload): ?AdFieldResolverInterface
    {
        if ($adDataPayload->getAdMedia()->count() > 1) {
            return app(LinkAdResolver::class);
        }

        return self::getResolverByAdType($adDataPayload->getAdMedia()[0]->getType());
    }

    public static function getMediaTypeByExtension(string $extension = 'jpg'): string
    {
        switch ($extension) {
            case 'mp4':
                return AdTypeEnum::VIDEO->value;
            case 'png':
            case 'jpg':
                return AdTypeEnum::LINK->value;
        }

        return AdTypeEnum::LINK->value;
    }
}
