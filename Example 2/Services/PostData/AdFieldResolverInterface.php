<?php

namespace App\Services\PostData;

use App\DTO\AdDataPayload;

interface AdFieldResolverInterface
{
    function getFields(AdDataPayload $adPayload): array;
    function getDataKey(AdDataPayload $adPayload): string;
}
