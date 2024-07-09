<?php

declare(strict_types=1);

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasOne;

class AdSetTemplate extends Model
{
    use HasFactory;

    protected $fillable = [
        'name',
        'adset_id',
        'campaign_id',
    ];

    public function campaign(): HasOne
    {
        return $this->hasOne(AdCampaign::class, 'campaign_id', 'campaign_id');
    }
}
