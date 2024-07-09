<?php

namespace App\Repositories;

use App\DTO\AdCampaignPayload;
use App\Models\AdCampaign;
use Illuminate\Database\Eloquent\Collection;

class AdCampaignRepository extends AbstractRepository
{

    public function getModelClass(): string
    {
        return AdCampaign::class;
    }

    public function deleteAll(): void
    {
        AdCampaign::truncate();
    }

    public function createAdCampaign(AdCampaignPayload $adCampaignPayload): AdCampaign
    {
        return AdCampaign::create([
            'campaign_id' => $adCampaignPayload->getCampaignId(),
            'name' => $adCampaignPayload->getName(),
            'ad_account_id' => $adCampaignPayload->getAccountId(),
        ]);
    }

    public function getAllCampaigns(): Collection
    {
        return AdCampaign::query()
            ->with('account')
            ->get()
        ;
    }
}
