<?php

namespace App\Console\Commands;

use App\Services\AdAccountCacheService;
use Illuminate\Console\Command;

class CacheAdManager extends Command
{
    /**
     * The name and signature of the console command.
     *
     * @var string
     */
    protected $signature = 'admanager:cache';

    /**
     * The console command description.
     *
     * @var string
     */
    protected $description = 'Command description';

    /**
     * Execute the console command.
     *
     * @return int
     */

    public function handle()
    {
        /** @var AdAccountCacheService $adAccountCacheService */
        $adAccountCacheService = app(AdAccountCacheService::class);

        $adAccountCacheService->cache();
    }
}
