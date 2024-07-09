<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\HasMany;

class AdAccount extends Model
{
    protected $fillable = [
        'name',
        'ad_account_id',
    ];

    public function campaigns(): HasMany
    {
        return $this->hasMany(AdCampaign::class, 'ad_account_id', 'ad_account_id');
    }
}
