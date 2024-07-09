<?php

namespace App\Services;

use App\DTO\AdMediaDTO;
use App\Models\Enum\AdPlacementEnum;
use App\Models\Enum\PlacementEnum;
use App\Services\Files\CanDeleteFiles;
use App\Services\PostData\FieldDataResolverManager;
use FacebookAds\Object\AdImage;
use FacebookAds\Object\Fields\AdImageFields;
use Illuminate\Support\Collection;
use Illuminate\Support\Facades\File;
use SplFileInfo;

class ImageService
{
    use CanDeleteFiles;

    /** @return SplFileInfo[] */
    public function getFiles(string $path): array
    {
        if (File::isDirectory($path)) {
            return File::allFiles($path);
        }
        return File::files($path);
    }

    public function createAdImage(SplFileInfo $uploadedImage)
    {
        $image = new AdImage(null, config('fb.credentials.ad_account_id'));
        $image->{AdImageFields::FILENAME} = $uploadedImage->getPathname();

        $image->create();

        return $image->{AdImageFields::HASH};
    }

    public function parseFileName(SplFileInfo $uploadedFile): string
    {
        return $uploadedFile->getBasename('.' . $uploadedFile->getExtension());
    }

    public function createAdMediaDTO(SplFileInfo $file): AdMediaDTO
    {
        $type = FieldDataResolverManager::getMediaTypeByExtension($file->getExtension());
        $parts = explode('$', $file->getBasename('.' . $file->getExtension()));

        return (new AdMediaDTO())
            ->setType($type)
            ->setName($parts[0])
            ->setPlacement($parts[1] ?? null)
            ->setIsDefault(
                (isset($parts[1]) && $parts[1] === PlacementEnum::FEED->value)
                || !isset($parts[1])
            )
        ;
    }

    public function parseImagePlacement(SplFileInfo $uploadedFile): string
    {
        $imageName = $uploadedFile->getFilename();
        $parts = explode('_', pathinfo($imageName)['filename']);

        return $parts[1] ?? AdPlacementEnum::ALL->value;
    }

    /**
     * @param string $path
     * @return Collection<SplFileInfo>
     */
    public function getFilesFromFolder(string $path): Collection
    {
        return collect(File::files($path));
    }
}
