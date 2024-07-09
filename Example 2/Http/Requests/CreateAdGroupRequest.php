<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

class CreateAdGroupRequest extends FormRequest
{
    /**
     * Determine if the user is authorized to make this request.
     *
     * @return bool
     */
    public function authorize()
    {
        return true;
    }

    /**
     * Get the validation rules that apply to the request.
     *
     * @return array
     */
    public function rules()
    {
        return [
            'ad_campaign_id' => 'required|integer',
            'ad_set_id' => 'required|integer',
            'ad_template_id' => 'required|integer',
            'ad_set_data' => 'required|array',
            'ad_set_data.start_time' => 'required|date',
            'ad_set_data.status' => 'string',
            'ad_status' => 'string',
            'page_id' => 'required|string',
            'actor_id' => 'sometimes|string',
            'ad_account_id' => 'required|string|exists:ad_accounts,ad_account_id',
            'images' => 'required',
            'conversion_domain' => 'sometimes|nullable|string',
        ];
    }
}
