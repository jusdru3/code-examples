<?php

namespace App\Http\Controllers\Api;

use App\DTO\AdDataPayload;
use App\Http\Controllers\Controller;
use App\Http\Requests\CreateAdGroupRequest;
use App\Models\AdCreativeTemplate;
use App\Repositories\AdAccountRepository;
use App\Repositories\AdCampaignRepository;
use App\Repositories\AdSetTemplateRepository;
use App\Repositories\PageRepository;
use App\Services\CreateAdService;
use Illuminate\Http\JsonResponse;

class FbController extends Controller
{
    public function __construct(
        private PageRepository $pageRepository,
        private AdSetTemplateRepository $adSetTemplateRepository,
        private AdCampaignRepository $adCampaignRepository,
        private AdAccountRepository $adAccountRepository,
        private CreateAdService $createAdService,
    ) {
    }

    public function createAdGroup(CreateAdGroupRequest $request): JsonResponse
    {
        $data = $request->validated();

        $adGroupPayload = (new AdDataPayload())
            ->setAdCampaignId($data['ad_campaign_id'])
            ->setAdSetId($data['ad_set_id'])
            ->setAdTemplateId($data['ad_template_id'])
            ->setImages($request->file('images'))
            ->setStartTime($data['ad_set_data']['start_time'])
            ->setAdStatus($data['ad_status'])
            ->setAdSetStatus($data['ad_set_data']['status'])
            ->setAdAccountId($data['ad_account_id'])
            ->setPageId($data['page_id'])
            ->setActorId($data['actor_id'])
            ->setConversionDomain($data['conversion_domain'] ?? null)
        ;

        //
        $this->createAdService->createAdGroupBatched($adGroupPayload);

        return response()->json([
            'data' => [],
            'success' => true,
        ]);
    }
    public function getCampaigns(): JsonResponse
    {
        $campaigns = $this->adCampaignRepository->getAllCampaigns();

        return response()->json([
            'data' => $campaigns
        ]);
    }

    public function getAdsetTemplates(): JsonResponse
    {
        $adSets = $this->adSetTemplateRepository->getAdSetTemplates();

        return response()->json([
            'data' => $adSets,
        ]);
    }

    public function getAdCreativeTemplates(): JsonResponse
    {
        return response()->json([
            'data' => AdCreativeTemplate::orderBy('created_at', 'desc')->get(),
        ]);
    }

    public function getOwnedPages(): JsonResponse
    {
        $pages = $this->pageRepository->getAllPages();

        return response()->json([
            'data' => $pages
        ]);
    }

    public function getAccounts(): JsonResponse
    {
        $accounts = $this->adAccountRepository->getAllAccounts();

        return response()->json([
            'data' => $accounts,
        ]);
    }
}
