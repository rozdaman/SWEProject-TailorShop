import React from 'react';
import { useCart } from '../context/CartContext';

const CartPage = () => {
    const { cart } = useCart();

    return (
        <div className="bg-white p-8 rounded-lg shadow-sm">
            <h1 className="text-2xl font-bold mb-4">Shopping Cart</h1>
            {cart?.items.length === 0 ? (
                <p>Your cart is empty.</p>
            ) : (
                <div className="space-y-4">
                    {cart?.items.map(item => (
                        <div key={item.shoppingCartItemId} className="flex justify-between border-b pb-2">
                            <span>{item.productName} (x{item.quantity})</span>
                            <span className="font-bold">${item.unitPrice * item.quantity}</span>
                        </div>
                    ))}
                    <div className="mt-4 pt-4 text-right text-xl font-bold">
                        Total: ${cart?.totalPrice}
                    </div>
                </div>
            )}
            <div className="mt-8 p-4 bg-yellow-50 border border-yellow-200 text-yellow-800 rounded">
                🚧 Detailed Cart View & Checkout Flow - Work in progress
            </div>
        </div>
    );
};

export default CartPage;
