<?php

namespace App\Http\Controllers\Api;

use App\DTO\AdTemplatePayload;
use App\Http\Requests\AdTemplateRequest;
use App\Http\Requests\UpdateAdTemplateRequest;
use App\Models\AdTemplate;
use App\Repositories\AdTemplateRepository;
use App\Services\AdTemplateService;
use Illuminate\Http\JsonResponse;

class AdTemplateController
{
    private AdTemplateRepository $adTemplateRepository;

    public function __construct(
        AdTemplateRepository $adTemplateRepository,
    ) {
        $this->adTemplateRepository = $adTemplateRepository;
    }

    public function create(AdTemplateRequest $request): JsonResponse
    {
        $adTemplatePayload = AdTemplatePayload::fromRequest($request);

        try {
            $adTemplate = $this->adTemplateRepository->createAdTemplate($adTemplatePayload);
        } catch (\Exception $e) {
            return response()->json(['message' => $e->getMessage()], 500);
        }

        return response()->json(['status' => true, 'data' => $adTemplate]);
    }

    public function put(AdTemplateRequest $request, int $id): JsonResponse
    {
        $adTemplatePayload = AdTemplatePayload::fromRequest($request);

        try {
            $this->adTemplateRepository->updateAdTemplate($id, $adTemplatePayload);
        } catch (\Exception $e) {
            return response()->json(['message' => $e->getMessage()], 500);
        }

        return response()->json([]);
    }

    public function index(): JsonResponse
    {
        return response()->json(['data' => $this->adTemplateRepository->getAllAdTemplates()]);
    }

    public function getOne(int $id): JsonResponse
    {
        return response()->json(['data' => $this->adTemplateRepository->getAdTemplate($id)]);
    }

    public function delete(AdTemplate $adTemplate): JsonResponse
    {
        $adTemplate->delete();

        return response()->json([]);
    }
}
