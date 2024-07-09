<?php

namespace App\Http\Requests;

use App\Models\Enum\CallToActionTypeEnum;
use Illuminate\Validation\Rules\Enum;

class UpdateAdTemplateRequest extends AdTemplateRequest
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
            'ad_name' => 'required|string',
            'ad_primary_text' => 'sometimes|nullable|string',
            'ad_body' => 'sometimes|nullable|string',
            'ad_link' => 'required|string',
            'ad_call_to_action_type' => ['nullable', new Enum(CallToActionTypeEnum::class)],
            'ad_headline' => 'sometimes|nullable|string',
            'ad_url_tags' => 'nullable|string',
        ];
    }
}
