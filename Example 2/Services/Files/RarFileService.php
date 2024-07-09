<?php

namespace App\Services\Files;

use Illuminate\Http\UploadedFile;
use RarArchive;

class RarFileService implements FileExtractInterface
{
    use CanDeleteFiles;

    public function extract(UploadedFile $uploadedFile): string
    {
        $file = $uploadedFile->store('public');
        $extractFolder = explode('.', $uploadedFile->getClientOriginalName())[0];
        $archive = RarArchive::open(storage_path('app/' . $file));
        $entries = $archive->getEntries();
        foreach ($entries as $entry) {
            $entry->extract(public_path('storage/app/' . $extractFolder));
        }

        $archive->close();
        $this->delete(storage_path('app/' . $file));

        return storage_path('app/public/' . $extractFolder);
    }
}
