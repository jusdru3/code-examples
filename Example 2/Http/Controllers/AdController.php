<?php

namespace App\Http\Controllers;

use App\Services\AdAccountCacheService;
use Illuminate\Http\RedirectResponse;
use Illuminate\View\View;

class AdController extends Controller
{
    public function __construct(
        private AdAccountCacheService $adAccountCacheService
    ) {
    }

    public function create(): View
    {
        return view('facebook.ads.create');
    }

    public function cacheAdsetsAndCampaigns(): RedirectResponse
    {
        $this->adAccountCacheService->cache();

        return redirect('ads/create');
    }
}
