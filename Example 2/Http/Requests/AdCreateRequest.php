<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

class AdCreateRequest extends FormRequest
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
            'ad_set_template_id' => 'required|integer',
            'ad_name' => 'required|string',
            'ad_creative_id' => 'sometimes|integer|nullable',
            'ad_creative_title' => 'required_without:ad_creative_id|string',
            'ad_creative_body' => 'required_without:ad_creative_id|string',
            'ad_creative_link' => 'required_without:ad_creative_id|string',
            'ad_creative_images' => 'required_without:ad_creative_id|array',
        ];
    }
}
