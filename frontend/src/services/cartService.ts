import api from './api';
import { ShoppingCart, AddCartItemDto, UpdateCartItemDto } from '../types/Cart';

export const cartService = {
    getMyCart: async () => {
        const response = await api.get<ShoppingCart>('/cart');
        return response.data;
    },

    addToCart: async (data: AddCartItemDto) => {
        const response = await api.post('/cart/add', data);
        return response.data;
    },

    updateQuantity: async (data: UpdateCartItemDto) => {
        const response = await api.put('/cart/update-quantity', data);
        return response.data;
    },

    removeFromCart: async (cartItemId: number) => {
        const response = await api.delete(`/cart/remove/${cartItemId}`);
        return response.data;
    },

    clearCart: async () => {
        const response = await api.delete('/cart/clear');
        return response.data;
    }
};
