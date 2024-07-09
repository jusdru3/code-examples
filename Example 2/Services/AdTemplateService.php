<?php

namespace App\Services;

use App\DTO\AdCreativePayload;
use App\Repositories\AdTemplateRepository;

class AdTemplateService
{
    private AdTemplateRepository $adTemplateRepository;

    public function __construct(
        AdTemplateRepository $adTemplateRepository,
    ) {
        $this->adTemplateRepository = $adTemplateRepository;
    }

    public function getAdTemplateAsCreative(int $templateId): AdCreativePayload
    {
        $template = $this->adTemplateRepository->getOneById($templateId);

        return (new AdCreativePayload())
            ->setLink($template->link)
            ->setType($template->ad_type)
            ->setUrlTags($template->url_tags)
            ->setLinkData($template->linkData)
            ->setCallToAction($template->callToAction)
        ;
    }
}
