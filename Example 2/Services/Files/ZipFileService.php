<?php

namespace App\Services\Files;

use Illuminate\Http\UploadedFile;
use VIPSoft\Unzip\Unzip;

class ZipFileService implements FileExtractInterface
{
    use CanDeleteFiles;

    public function extract(UploadedFile $uploadedFile): string
    {
        $unzipper = new Unzip();
        $file = $uploadedFile->store('public'); //store file in storage/app/zip
        $extractFolder = explode('.', $uploadedFile->getClientOriginalName())[0];
        $unzipper->extract(storage_path('app/'.$file), storage_path('app/public/' . $extractFolder));
        $this->delete(storage_path('app/' . $file));

        return storage_path('app/public/' . $extractFolder);
    }
}
