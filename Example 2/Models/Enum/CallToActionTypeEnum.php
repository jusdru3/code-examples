<?php

namespace App\Models\Enum;

enum CallToActionTypeEnum: string
{
    case OPEN_LINK = 'OPEN_LINK';
    case LIKE_PAGE = 'LIKE_PAGE';
    case SHOP_NOW = 'SHOP_NOW';
    case LEARN_MORE = 'LEARN_MORE';
    case SIGN_UP = 'SIGN_UP';
    case BUY_NOW = 'BUY_NOW';
    case BUY = 'BUY';
    case ORDER_NOW = 'ORDER_NOW';
    case ADD_TO_CART = 'ADD_TO_CART';
    case SWIPE_UP_PRODUCT = 'SWIPE_UP_PRODUCT';
    case SWIPU_UP_SHOP = 'SWIPE_UP_SHOP';
    case FOLLOW_PAGE = 'FOLLOW_PAGE';
}
