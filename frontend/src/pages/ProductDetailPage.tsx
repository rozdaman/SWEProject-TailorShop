import React from 'react';
import { useParams } from 'react-router-dom';

const ProductDetailPage = () => {
    const { id } = useParams();

    return (
        <div className="bg-white p-8 rounded-lg shadow-sm">
            <h1 className="text-2xl font-bold mb-4">Product Detail</h1>
            <p className="text-gray-600">This is the detail page for product ID: <span className="font-mono font-bold">{id}</span></p>
            <div className="mt-8 p-4 bg-yellow-50 border border-yellow-200 text-yellow-800 rounded">
                🚧 Work in progress
            </div>
        </div>
    );
};

export default ProductDetailPage;
