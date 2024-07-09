<?php

namespace App\Repositories;

use App\DTO\PagePayload;
use App\Models\Page;
use Illuminate\Database\Eloquent\Collection;

class PageRepository extends AbstractRepository
{

    public function getModelClass(): string
    {
        return Page::class;
    }

    public function deleteAll(): void
    {
        Page::truncate();
    }

    public function getPageByPageId(string $pageId): Page
    {
        return Page::where('page_id', $pageId)
            ->first()
        ;
    }

    public function createPage(PagePayload $pagePayload): Page
    {
        return Page::create([
            'name' => $pagePayload->getName(),
            'page_id' => $pagePayload->getPageId(),
            'type' => $pagePayload->getType(),
            'access_token' => $pagePayload->getAccessToken(),
        ]);
    }

    public function getAllPages(): Collection
    {
        return Page::query()
            ->get()
        ;
    }
}
