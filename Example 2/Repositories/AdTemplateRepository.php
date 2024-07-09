<?php

namespace App\Repositories;

use App\DTO\AdTemplatePayload;
use App\Models\AdTemplate;
use Illuminate\Database\Eloquent\Collection;

class AdTemplateRepository extends AbstractRepository
{

    public function getModelClass(): string
    {
        return AdTemplate::class;
    }

    public function getAllAdTemplates(): Collection
    {
        return AdTemplate::with(['linkData', 'callToAction'])->get();
    }

    public function getAdTemplate(int $id): ?AdTemplate
    {
        return AdTemplate::where('id', $id)
            ->with(['linkData', 'callToAction'])
            ->first()
        ;
    }

    public function createAdTemplate(AdTemplatePayload $adTemplatePayload): AdTemplate
    {
        $adTemplate = AdTemplate::create([
            'name' => $adTemplatePayload->getName(),
            'title' => $adTemplatePayload->getTitle(),
            'body' => $adTemplatePayload->getBody(),
            'link' => $adTemplatePayload->getLink(),
            'url_tags' => $adTemplatePayload->getUrlTags(),
        ]);

        $adTemplate->linkData()->create([
           'headline' => $adTemplatePayload->getHeadline(),
           'link' => $adTemplatePayload->getLink(),
           'description' => $adTemplatePayload->getBody(),
           'primary_text' => $adTemplatePayload->getTitle()
        ]);

        $adTemplate->callToAction()->create([
            'type' => $adTemplatePayload->getCallToActionType(),
            'link' => $adTemplatePayload->getLink(),
        ]);

        return $adTemplate;
    }

    public function updateAdTemplate(int $id, AdTemplatePayload $adTemplatePayload): void
    {
        $adTemplate = $this->getAdTemplate($id);

        $adTemplate->update([
            'name' => $adTemplatePayload->getName(),
            'title' => $adTemplatePayload->getTitle(),
            'body' => $adTemplatePayload->getBody(),
            'link' => $adTemplatePayload->getLink(),
            'url_tags' => $adTemplatePayload->getUrlTags(),
        ]);

        $adTemplate->linkData()->update([
            'headline' => $adTemplatePayload->getHeadline(),
            'link' => $adTemplatePayload->getLink(),
            'description' => $adTemplatePayload->getBody(),
            'primary_text' => $adTemplatePayload->getTitle()
        ]);

        $adTemplate->callToAction()->update([
            'type' => $adTemplatePayload->getCallToActionType(),
            'link' => $adTemplatePayload->getLink(),
        ]);
    }

    public function deleteAdTemplate(int $id): void
    {
        AdTemplate::where('id', $id)
            ->delete()
        ;
    }
}
