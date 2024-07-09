<?php

declare(strict_types=1);

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class AdCampaign extends Model
{
    protected $table = 'ad_campaigns';

    protected $fillable = [
        'campaign_id',
        'name',
        'ad_account_id',
    ];

    public function account(): BelongsTo
    {
        return $this->belongsTo(AdAccount::class, 'ad_account_id', 'ad_account_id');
    }
}
