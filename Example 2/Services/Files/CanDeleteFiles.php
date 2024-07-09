<?php

namespace App\Services\Files;

use Illuminate\Support\Facades\File;

trait CanDeleteFiles
{
    public function delete(string $path): void
    {
        if (File::isDirectory($path)) {
            File::deleteDirectory($path);

            return;
        }

        File::delete($path);
    }
}
