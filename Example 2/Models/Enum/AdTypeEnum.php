<?php

namespace App\Models\Enum;

enum AdTypeEnum: string
{
    case LINK = 'link';
    case POST = 'post';
    case IMAGE = 'image';
    case VIDEO = 'video';
}
