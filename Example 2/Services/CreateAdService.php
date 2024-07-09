<?php

namespace App\Services;

use App\DTO\AdDataPayload;
use App\Repositories\AdTemplateRepository;
use App\Repositories\PageRepository;
use App\Exceptions\FileException;
use App\Services\Facebook\FbGraphWrapper;
use FacebookAds\Object\Fields\AdFields;
use App\Services\Files\FileExtractInterface;
use App\Services\Files\FileServiceFactory;

class CreateAdService
{
    public function __construct(
        private FbAdManager $fbAdManager,
        private FbGraphWrapper $fbGraphWrapper,
        private AdFieldService $adFieldService,
        private AdTemplateRepository $adTemplateRepository,
        private PageRepository $pageRepository,
        private ImageService $imageService,
    ) {
    }

    /**
     * @throws FileException
     */
    public function createAdGroupBatched(AdDataPayload $adDataPayload): void
    {
        /** @var FileExtractInterface $fileExtract */
        $fileExtract = FileServiceFactory::createFromFileExtension($adDataPayload->getImages()[0]->extension());

        $zipFile = $adDataPayload->getImages()[0];
        $imagePath = $fileExtract->extract($zipFile);
        $adsetName = explode('.', $zipFile->getClientOriginalName())[0];

        $adset = $this->fbGraphWrapper->createAdsetCopy(
            $adsetName,
            $adDataPayload
        );
        $page = $this->pageRepository->getPageByPageId($adDataPayload->getPageId());
        $instagramAccounts = null;
        if(config('fb.instagram_accounts')) {
            $pageBackedAccounts = $this->fbGraphWrapper->getPageBackedInstagramAccounts($page);
            if($pageBackedAccounts) {
                $instagramAccounts = $pageBackedAccounts->resolve();
            }
        }

        if (isset($instagramAccounts[0]) && isset($instagramAccounts[0]['id'])) {
            $adDataPayload->setActorId(null);
        }

        $adDataPayload->setAdSetId($adset['copied_adset_id']);

        $files = $this->imageService->getFiles($imagePath);
        $adTemplate = $this->adTemplateRepository
            ->getAdTemplate($adDataPayload->getAdTemplateId());

        $adDataPayload->setAdTemplate($adTemplate);
        $batch = [];
        $adMedia = $this->fbAdManager->prepareAdMedia($files);

        foreach ($adMedia as $media) {
            $adDataPayload
                ->setAdMedia($media)
                ->setName($media[0]->getName());

            $ad = $this->adFieldService->getAdFields($adDataPayload);

            $ad[AdFields::CREATIVE] = json_encode($ad[AdFields::CREATIVE]);

            $batch[] = http_build_query($ad);
        }

        $this->fbGraphWrapper->createBatchRequest($batch, $adDataPayload->getAdAccountId());

        $this->imageService->delete($imagePath);
    }
}
