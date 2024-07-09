<?php

namespace App\Http\Controllers;

use Illuminate\Http\RedirectResponse;

class PagesController extends Controller
{
    public function create(): RedirectResponse
    {
        return redirect('ads/create');
    }
}
