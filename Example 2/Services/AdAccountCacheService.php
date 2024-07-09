<?php

namespace App\Services;

use App\Repositories\AdCampaignRepository;
use App\Repositories\AdSetTemplateRepository;
use App\Services\Facebook\FbGraphManager;

class AdAccountCacheService
{
    public function __construct(
        private FbGraphManager $fbGraphManager,
        private FbAdManager $fbAdManager,
        private AdSetTemplateRepository $adSetTemplateRepository,
        private AdCampaignRepository $adCampaignRepository,
    ) {
    }

    public function cache(): void
    {
        $this->adCampaignRepository->deleteAll();
        $this->adSetTemplateRepository->deleteAll();
        $adAccounts = $this->fbGraphManager->cacheAdAccounts();

        foreach ($adAccounts as $adAccount) {
            $this->fbAdManager->setAdAccountId($adAccount->ad_account_id);
            $this->fbAdManager->cacheCampaignsAndAdsets();
        }

        $this->fbGraphManager->cacheBusinessPages();
        $this->fbGraphManager->cacheInstagramPagesLocal();
    }
}
