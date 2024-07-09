<?php

namespace App\Repositories;

use App\DTO\AdsetPayload;
use App\Models\AdSetTemplate;
use Illuminate\Database\Eloquent\Collection;

class AdSetTemplateRepository extends AbstractRepository
{

    public function getModelClass(): string
    {
        return AdSetTemplate::class;
    }

    public function deleteAll(): void
    {
        AdSetTemplate::truncate();
    }

    public function createAdSetTemplate(AdsetPayload $adsetPayload): AdSetTemplate
    {
        return AdSetTemplate::create([
            'adset_id' => $adsetPayload->getAdsetId(),
            'campaign_id' => $adsetPayload->getCampaignId(),
            'name' => $adsetPayload->getName(),
        ]);
    }

    public function getAdSetTemplates(): Collection
    {
        return AdSetTemplate::query()
            ->with(['campaign', 'campaign.account'])
            ->get()
        ;
    }
}
