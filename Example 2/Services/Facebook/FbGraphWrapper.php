<?php

namespace App\Services\Facebook;

use App\DTO\AdDataPayload;
use App\Exceptions\FacebookApiException;
use App\Helpers\GeneralHelper;
use App\Http\Resources\PageAccessTokenResource;
use App\Http\Resources\PageBackedInstagramAccountResource;
use App\Models\Page;
use Exception;
use Facebook\Exception\ResponseException;
use JoelButcher\Facebook\Facebook;

class FbGraphWrapper
{
    private Facebook $facebook;
    public const LIMIT = 100;

    public function __construct()
    {
        $this->facebook = new Facebook([
            'app_id' => config('fb.credentials.app_id'),
            'app_secret' => config('fb.credentials.app_secret'),
            'default_graph_version' => config('fb.default_graph_version'),
            // 'default_access_token' => config('fb.credentials.access_token'),
        ]);
    }

    public function createAdsetCopy(string $adsetName, AdDataPayload $data): array
    {
        try {
            $originalAdsetId = $data->getAdSetId();
            $params = [
                'campaign_id' => $data->getAdCampaignId(),
                'rename_options' => [
                    'rename_prefix' => $adsetName,
                    'rename_suffix' => " ",
                ],
                'start_time' => $data->getStartTime(),
                'status_option' => $data->getAdSetStatus(),
                'access_token' => config('fb.credentials.access_token'),
            ];

            GeneralHelper::log('params ' . json_encode($params), 'createAdsetCopy');


            $response = $this->facebook->post(
                "/{$originalAdsetId}/copies",
                $params
            );

            GeneralHelper::log('request ' . "/{$originalAdsetId}/copies" .' -params' . json_encode($params), 'createAdsetCopy');
            GeneralHelper::log('response ' . "/{$originalAdsetId}/copies" .'  ' . json_encode($response), 'createAdsetCopy');

        } catch (ResponseException $facebookException) {
            GeneralHelper::log('exception ' . "/{$originalAdsetId}/copies" .'  ' . json_encode($facebookException->getMessage()), 'createAdsetCopy');

            throw new FacebookApiException($facebookException->getMessage(), $facebookException->getHttpStatusCode());
        }


        return $response->getDecodedBody();
    }

    public function getBusinessPages(?string $after = null): array
    {
        $businessId = config('fb.business_id');
        try {
            $response = $this->facebook->get(
                "{$businessId}/owned_pages",
                [
                    'summary' => 'total_count',
                    'after' => $after,
                    'limit' => self::LIMIT,
                    'access_token' => config('fb.credentials.access_token'),
                ]
            );
        } catch (Exception $e) {
            throw $e;
        }


        return $response->getDecodedBody();
    }

    public function getPageAccessTokens(?string $after = null): PageAccessTokenResource
    {
        $scopedUserId = config('fb.scope_user_id');
        $response = $this->facebook->get(
            "{$scopedUserId}/accounts",
            [
                'fields' => 'access_token,id,name',
                'limit' => self::LIMIT,
                'after' => $after,
                'access_token' => config('fb.credentials.access_token'),
            ]
        );

        return new PageAccessTokenResource($response->getDecodedBody());
    }

    public function getAdAccounts(?string $after = null): array
    {
        $businessId = config('fb.business_id');
        $endpoint = config('fb.ad_account_endpoints.' . config('fb.env'));
        try {
            $params = [
                'fields' => 'id,name',
                'after' => $after,
                'limit' => self::LIMIT,
                'access_token' => config('fb.credentials.access_token'),
            ];
            $response = $this->facebook->get(
                "{$businessId}/{$endpoint}",
                $params
            );
        } catch (Exception $e) {
            report($e);
            GeneralHelper::log('request '. $endpoint . ' failed' . $e->getMessage(), 'request-exception');
            throw $e;
        }

        return $response->getDecodedBody();
    }

    public function createBatchRequest(array $batch, string $accountId)
    {
        $objects = [];
        $version = config('fb.default_graph_version');
        foreach ($batch as $key=>$ad) {
            $object = [];
            $object['method'] = "POST";
            $object['relative_url'] = "{$version}/{$accountId}/ads";
            $object['name'] = 'test' . $key;
            $object['body'] = $ad;
            $objects[] = $object;
        }
        $jsonString = json_encode($objects);
        try {
            $params = [
                'name' => 'batch_' . strtotime(date('Y-m-d')),
                'batch' => $jsonString,
                'access_token' => config('fb.credentials.access_token'),
            ];
            $response = $this->facebook->post(
                "/",
                $params
            );
            GeneralHelper::log('request / -params' . json_encode($params), 'createBatchRequest');
            GeneralHelper::log('response / ' . json_encode($response), 'createBatchRequest');

        } catch (Exception $e) {
            report($e);
            GeneralHelper::log('request / -params' . json_encode($params), 'createBatchRequest');
            throw $e;
        }

        return $response;
    }

    public function getPageBackedInstagramAccounts(Page $page): PageBackedInstagramAccountResource | null
    {
        if(!$page->access_token) return null;

        $pageId = $page->page_id;
        $response = $this->facebook->get(
            "/{$pageId}/page_backed_instagram_accounts",
            [
                'access_token' => $page->access_token,
            ]
        );

        return new PageBackedInstagramAccountResource($response->getDecodedBody());
    }
}
