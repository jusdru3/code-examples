<?php

namespace App\Http\Controllers;

use App\Http\Controllers\Controller;
use FacebookAds\Object\Fields\AdSetFields;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

class AdSetTemplateController extends Controller
{
    public function index(): View
    {
        return view('facebook.ad-sets.index');
    }

    public function create(): View
    {
        return view('facebook.ad-sets.create');
    }

    public function store(): View
    {
        return view('facebook.ad-sets.index');
    }
}
