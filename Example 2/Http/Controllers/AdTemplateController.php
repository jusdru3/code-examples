<?php

namespace App\Http\Controllers;

use App\Http\Controllers\Controller;
use App\Models\AdTemplate;
use App\Repositories\AdTemplateRepository;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

class AdTemplateController extends Controller
{
    public function __construct(
        private AdTemplateRepository $adTemplateRepository,
    ) {
    }

    public function index(): View
    {
        $adTemplates = $this->adTemplateRepository->getAllAdTemplates();

        return view('facebook.ad-templates.index')
            ->with('adTemplates', $adTemplates)
        ;
    }

    public function create(): View
    {
        return view('facebook.ad-templates.create');
    }

    public function show(int $id): View
    {
        return view('facebook.ad-templates.update')
            ->with('id', $id)
        ;
    }

    public function destroy(AdTemplate $adTemplate): RedirectResponse
    {
        $adTemplate->delete();

        return redirect('ad-templates.index')
            ->with('status', 'success')
            ->with('message', 'Ad template deleted successfully')
        ;
    }
}
