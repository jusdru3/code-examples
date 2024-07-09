<?php

namespace App\Services;

use App\DTO\AdCampaignPayload;
use App\DTO\AdMediaDTO;
use App\DTO\AdsetPayload;
use App\Events\MediaPrepared;
use App\Models\Enum\AdTypeEnum;
use App\Models\Enum\PlacementEnum;
use App\Repositories\AdCampaignRepository;
use App\Repositories\AdSetTemplateRepository;
use Exception;
use FacebookAds\Api;
use FacebookAds\Object\AdAccount;
use FacebookAds\Object\AdImage;
use FacebookAds\Object\AdVideo;
use FacebookAds\Object\Campaign;
use FacebookAds\Object\Fields\AdAccountFields;
use FacebookAds\Object\Fields\AdImageFields;
use FacebookAds\Object\Fields\AdSetFields;
use FacebookAds\Object\Fields\CampaignFields;
use Illuminate\Support\Collection;
use Illuminate\Support\Facades\Session;
use SplFileInfo;

class FbAdManager
{
    private Api $instance;
    private AdAccount $adAccount;
    private AdSetTemplateRepository $adSetTemplateRepository;
    private AdCampaignRepository $adCampaignRepository;
    private ImageService $imageService;
    private array $adAccountConfig;

    public function __construct(
        AdSetTemplateRepository $adSetTemplateRepository,
        AdCampaignRepository $adCampaignRepository,
        ImageService $imageService,
    ) {
        $this->adSetTemplateRepository = $adSetTemplateRepository;
        $this->adCampaignRepository = $adCampaignRepository;
        $this->imageService = $imageService;

        $env = config('fb.env');
        $this->adAccountConfig = config("fb.{$env}.ad_accounts");

        $this->init();
    }

    private function init(): void
    {
        Api::init(
            config('fb.credentials.app_id'),
            config('fb.credentials.app_secret'),
            config('fb.credentials.access_token')
        );

        $this->instance = Api::instance();
        if (request()->has('ad_account_id') || Session::has('ad_account_id')) {
            $this->adAccount = new AdAccount(
                request()->get('ad_account_id')
                ?? Session::get('ad_account_id')
            );
        }
    }

    public function setAdAccountId(string $adAccountId): self
    {
        $this->adAccount = new AdAccount($adAccountId);

        return $this;
    }

    public function getCampaigns(): array
    {
        $cursor = $this->adAccount->getCampaigns([
            CampaignFields::ID,
            CampaignFields::NAME,
        ],
        [
            'limit' => 100,
        ]);

        $cursor->setUseImplicitFetch(true);

        $campaignsData = [];

        while($cursor->current() !== false) {
            $campaign = $cursor->current();
            $campaignsData[] = [
                'id' => $campaign->{CampaignFields::ID},
                'name' => $campaign->{CampaignFields::NAME},
            ];
            $cursor->next();
        }

        return $campaignsData;
    }

    public function getAdSetsByCampaignId($campaignId): array
    {
        $campaign = new Campaign($campaignId);

        $cursor = $campaign->getAdSets(
            [
                AdSetFields::ID,
                AdSetFields::NAME,
                AdSetFields::CAMPAIGN_ID,
            ],
            [
                'limit' => 100,
            ]
        );

        $cursor->setUseImplicitFetch(true);

        $adSetData = [];

        while($cursor->current() !== false) {
            $adset = $cursor->current();
            $adSetData[] = [
                'id' => $adset->{AdSetFields::ID},
                'name' => $adset->{AdSetFields::NAME},
            ];

            $cursor->next();
        }

        return $adSetData;
    }

    public function uploadAdVideo(SplFileInfo $file): AdVideo
    {
        try {
            $adVideo = $this->adAccount->createAdVideo(
                [],
                [
                    'video_file_chunk' => $file,
                ]
            );
        } catch(Exception $exception) {
           throw $exception;
        }

        return $adVideo;
    }

    public function createAdImage(SplFileInfo $uploadedImage)
    {
        $image = new AdImage(null, $this->adAccount->id);
        $image->{AdImageFields::FILENAME} = $uploadedImage->getPathname();

        $image->create();

        return $image->{AdImageFields::HASH};
    }

    /**
     *  @param array<SplFileInfo> $files
     * @return Collection<AdMediaDTO>
     */
    public function prepareAdMedia(array $files): Collection
    {
        $mediaArr = [];

        foreach ($files as $key => $file) {
            $adMedia = $this->imageService->createAdMediaDTO($file);
            $adMedia->setId($key);

            if ($adMedia->getType() === AdTypeEnum::LINK->value) {
                $imageHash = $this->createAdImage($file);
                $adMedia->setImageHash($imageHash);
            } else if ($adMedia->getType() === AdTypeEnum::VIDEO->value) {
                $video = $this->uploadAdVideo($file);
                $adMedia->setVideo($video);
            }

            $mediaArr[$adMedia->getName()][] = $adMedia;
        }

        $mediaArr = collect(array_map(function (iterable $media) {
            return collect($media);
        }, $mediaArr));

        MediaPrepared::dispatch($mediaArr);

        return $mediaArr;
    }

    public function cacheCampaignsAndAdsets(): void
    {
        $accountConfig = $this->adAccountConfig[$this->adAccount->{AdAccountFields::ID}] ?? null;
        $templateCampaignIds = [];
        if (isset($accountConfig)) {
            $templateCampaignIds = $accountConfig['template_campaigns'];
        }
        $campaigns = $this->getCampaigns();

        foreach ($campaigns as $campaign) {
            $this->adCampaignRepository->createAdCampaign(
                (new AdCampaignPayload)
                ->setCampaignId($campaign['id'])
                ->setName($campaign['name'])
                ->setAccountId($this->adAccount->{AdAccountFields::ID})
            );
        }

        foreach ($templateCampaignIds as $campaignId) {
            $adsets = $this->getAdSetsByCampaignId($campaignId);

            foreach ($adsets as $adset) {
                $this->adSetTemplateRepository->createAdSetTemplate(
                    (new AdsetPayload())
                    ->setName($adset['name'])
                    ->setCampaignId($campaignId)
                    ->setAdsetId($adset['id'])
                );
            }
        }
    }
}
