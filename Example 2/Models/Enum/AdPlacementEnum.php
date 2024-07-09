<?php

namespace App\Models\Enum;

enum AdPlacementEnum: string
{
    case ALL = 'all';
    case STORY = 'story';
    case FEED = 'feed';
}
