export interface CartItem {
    shoppingCartItemId: number;
    productId: number;
    productName: string;
    productImage?: string;
    quantity: number;
    unitPrice: number;
    variantId?: number;
}

export interface ShoppingCart {
    shoppingCartId: number;
    userId?: number;
    items: CartItem[];
    totalPrice: number;
}

export interface AddCartItemDto {
    productId: number;
    quantity: number;
    variantId?: number;
}

export interface UpdateCartItemDto {
    cartItemId: number;
    newQuantity: number;
}
