export interface CartItem {
    shoppingCartItemId: number;
    productId: number;
    productName: string;
    productImage: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
    variantId?: number;
}

export interface ShoppingCart {
    shoppingCartId: number;
    userId: number;
    totalPrice: number;
    items: CartItem[];
}