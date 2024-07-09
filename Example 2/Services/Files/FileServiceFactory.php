<?php

namespace App\Services\Files;

use App\Exceptions\FileException;

class FileServiceFactory
{
    /**
     * @throws FileException
     */
    public static function createFromFileExtension(string $extension): ?FileExtractInterface
    {
        switch ($extension) {
            case 'zip':
                return new ZipFileService();
            case 'rar':
                return new RarFileService();
        }

        throw new FileException('Unsupported file format.', 400);
    }
}
