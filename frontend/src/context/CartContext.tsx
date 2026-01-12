import React, { createContext, useState, useEffect, useContext } from 'react';
import { ShoppingCart, AddCartItemDto, UpdateCartItemDto } from '../types/Cart';
import { cartService } from '../services/cartService';

interface CartContextType {
    cart: ShoppingCart | null;
    loading: boolean;
    addToCart: (productId: number, quantity: number, variantId?: number) => Promise<void>;
    removeFromCart: (cartItemId: number) => Promise<void>;
    updateQuantity: (cartItemId: number, newQuantity: number) => Promise<void>;
    refreshCart: () => Promise<void>;
    clearCart: () => Promise<void>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [cart, setCart] = useState<ShoppingCart | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    const refreshCart = async () => {
        try {
            setLoading(true);
            const data = await cartService.getMyCart();
            setCart(data);
        } catch (error) {
            console.error("Failed to fetch cart:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        refreshCart();
    }, []);

    const addToCart = async (productId: number, quantity: number, variantId?: number) => {
        try {
            const dto: AddCartItemDto = { productId, quantity, variantId };
            await cartService.addToCart(dto);
            await refreshCart(); // Reload cart to get updated totals/items
        } catch (error) {
            console.error("Failed to add to cart:", error);
            throw error;
        }
    };

    const removeFromCart = async (cartItemId: number) => {
        try {
            await cartService.removeFromCart(cartItemId);
            await refreshCart();
        } catch (error) {
            console.error("Failed to remove from cart:", error);
            throw error;
        }
    };

    const updateQuantity = async (cartItemId: number, newQuantity: number) => {
        try {
            const dto: UpdateCartItemDto = { cartItemId, newQuantity };
            await cartService.updateQuantity(dto);
            await refreshCart();
        } catch (error) {
            console.error("Failed to update quantity:", error);
            throw error;
        }
    };

    const clearCart = async () => {
        try {
            await cartService.clearCart();
            setCart(null);
        } catch (error) {
            console.error("Failed to clear cart:", error);
            throw error;
        }
    };

    return (
        <CartContext.Provider value={{ cart, loading, addToCart, removeFromCart, updateQuantity, refreshCart, clearCart }}>
            {children}
        </CartContext.Provider>
    );
};

export const useCart = () => {
    const context = useContext(CartContext);
    if (!context) {
        throw new Error('useCart must be used within a CartProvider');
    }
    return context;
};
