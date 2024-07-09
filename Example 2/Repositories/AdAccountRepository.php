<?php

namespace App\Repositories;

use App\DTO\AdAccountPayload;
use App\Models\AdAccount;
use Illuminate\Database\Eloquent\Collection;

class AdAccountRepository extends AbstractRepository
{
    public function getModelClass(): string
    {
        return AdAccount::class;
    }

    public function deleteAll(): void
    {
        AdAccount::truncate();
    }

    public function getAllAccounts(): Collection
    {
        return AdAccount::query()
            ->get()
        ;
    }

    public function createAdAccount(AdAccountPayload $adAccountPayload): AdAccount
    {
        return AdAccount::create([
            'ad_account_id' => $adAccountPayload->getAdAccountId(),
            'name' => $adAccountPayload->getName(),
        ]);
    }
}
