<?php

namespace App\Services\Facebook;

use App\DTO\AdAccountPayload;
use App\DTO\PagePayload;
use App\Models\AdAccount;
use App\Models\Page;
use App\Repositories\AdAccountRepository;
use App\Repositories\PageRepository;
use Exception;
use Illuminate\Support\Collection;

class FbGraphManager
{

    public function __construct(
        private FbGraphWrapper $fbGraphWrapper,
        private PageRepository $pageRepository,
        private AdAccountRepository $adAccountRepository,
    ) {
    }

    public function cacheBusinessPages(): void
    {
        $this->pageRepository->deleteAll();
        $accessTokens = $this->fbGraphWrapper->getPageAccessTokens()->resolve();
        do {
            $cursor = $this->fbGraphWrapper->getBusinessPages($cursor['paging']['cursors']['after'] ?? null);

            foreach ($cursor['data'] as $page) {
                $this->pageRepository->createPage(
                    (new PagePayload())
                        ->setType(Page::PAGE_TYPE_FACEBOOK)
                        ->setName($page['name'])
                        ->setPageId($page['id'])
                        ->setAccessToken($accessTokens[$page['id']] ?? null)
                );
            }
        } while (isset($cursor['paging']['next']) && count($cursor['data']) >= FbGraphWrapper::LIMIT);
    }

    public function cacheInstagramPagesLocal(): void
    {
        $instagramPages = config('fb.instagram_accounts');
        if (!$instagramPages) {
            return;
        }

        foreach ($instagramPages as $page) {
            $this->pageRepository->createPage(
                (new PagePayload())
                    ->setType(Page::PAGE_TYPE_INSTAGRAM)
                    ->setName($page['name'])
                    ->setPageId($page['id'])
            );
        }
    }

    public function cacheInstagramPages(): void
    {
        $this->pageRepository->deleteAll();
        do {
            $cursor = $this->fbGraphWrapper->getInstagramAccounts($cursor['paging']['cursors']['after'] ?? null);

            foreach ($cursor['data'] as $page) {
                $this->pageRepository->createPage(
                    (new PagePayload())
                        ->setType(Page::PAGE_TYPE_INSTAGRAM)
                        ->setName($page['name'])
                        ->setPageId($page['id'])
                );
            }
        } while (isset($cursor['paging']['next']) && count($cursor['data']) >= FbGraphWrapper::LIMIT);
    }

    /**
     * @return Collection<AdAccount>
     * @throws Exception
     */
    public function cacheAdAccounts(): Collection
    {
        $this->adAccountRepository->deleteAll();
        $adAccounts = collect([]);

        do {
            $cursor = $this->fbGraphWrapper->getAdAccounts($cursor['paging']['cursors']['after'] ?? null);

            foreach ($cursor['data'] as $adAccount) {
                $newAccount = $this->adAccountRepository->createAdAccount(
                    (new AdAccountPayload())
                        ->setAdAccountId($adAccount['id'])
                        ->setName($adAccount['name'])
                );

                $adAccounts->push($newAccount);
            }
        } while (isset($cursor['paging']['next']) && count($cursor['data']) >= FbGraphWrapper::LIMIT);

        return $adAccounts;
    }
}
