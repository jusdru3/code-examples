<?php

namespace App\Services\Files;

use Illuminate\Http\UploadedFile;

interface FileExtractInterface
{
    function extract(UploadedFile $uploadedFile): string;
}
