<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class Page extends Model
{
    use HasFactory;

    public const PAGE_TYPE_INSTAGRAM = 'instagram';
    public const PAGE_TYPE_FACEBOOK = 'facebook';

    protected $table = 'owned_pages';

    protected $fillable = [
        'page_id',
        'name',
        'type',
        'access_token',
    ];
}
