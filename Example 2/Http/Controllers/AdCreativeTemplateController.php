<?php

namespace App\Http\Controllers;

use App\Http\Controllers\Controller;
use App\Models\AdCreativeTemplate;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

class AdCreativeTemplateController extends Controller
{
    public function index(): View
    {
        return view('facebook.ad-creatives.index', [
            'adCreatives' => AdCreativeTemplate::orderBy('created_at', 'desc')->get(),
        ]);
    }

    public function create(): View
    {
        return view('facebook.ad-creatives.create');
    }

    public function store(Request $request): RedirectResponse
    {
        $request->validate([
            'name' => 'required',
            'title' => 'required',
            'body' => 'required',
        ]);

        AdCreativeTemplate::create($request->toArray());

        return redirect()->back();
    }

    public function edit(AdCreativeTemplate $adCreativeTemplate)
    {
        dd($adCreativeTemplate);
    }

    public function update(AdCreativeTemplate $adCreativeTemplate, Request $request): RedirectResponse
    {
        $request->validate([
            'name' => 'required',
            'title' => 'required',
            'body' => 'required',
        ]);

        $adCreativeTemplate->update($request->toArray());

        return redirect()->back();
    }
}
