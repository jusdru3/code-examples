<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasOne;

class AdTemplate extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'ad_type',
        'link',
        'url_tags',
    ];

    public function linkData(): HasOne
    {
        return $this->hasOne(LinkData::class, 'ad_template_id', 'id');
    }

    public function callToAction(): HasOne
    {
        return $this->hasOne(CallToAction::class, 'ad_template_id', 'id');
    }
}
