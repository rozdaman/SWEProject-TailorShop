import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { productService } from '../services/productService';
import { ProductListDto } from '../types/Product';

const HomePage = () => {
    const [products, setProducts] = useState<ProductListDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [searchParams] = useSearchParams();

    useEffect(() => {
        const fetchProducts = async () => {
            setLoading(true);
            try {
                const keyword = searchParams.get('search');
                let data;

                if (keyword) {
                    data = await productService.search(keyword);
                } else {
                    data = await productService.getAll();
                }

                setProducts(data);
            } catch (err: any) {
                setError(err.message || 'Failed to fetch products');
            } finally {
                setLoading(false);
            }
        };

        fetchProducts();
    }, [searchParams]);

    if (loading) return <div className="text-center py-20 text-gray-500">Loading products...</div>;
    if (error) return <div className="text-center py-20 text-red-500">Error: {error}</div>;

    return (
        <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-8">
                {searchParams.get('search') ? `Results for "${searchParams.get('search')}"` : 'All Products'}
            </h1>

            {products.length === 0 ? (
                <p className="text-gray-500 text-center">No products found.</p>
            ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    {products.map((product) => (
                        <div key={product.id} className="bg-white rounded-xl shadow-sm hover:shadow-md transition-shadow duration-300 overflow-hidden border border-gray-100">
                            <div className="aspect-w-1 aspect-h-1 w-full overflow-hidden bg-gray-200 xl:aspect-w-7 xl:aspect-h-8">
                                {product.imageUrl ? (
                                    <img
                                        src={product.imageUrl}
                                        alt={product.name}
                                        className="h-64 w-full object-cover object-center group-hover:opacity-75"
                                    />
                                ) : (
                                    <div className="h-64 w-full flex items-center justify-center bg-gray-200 text-gray-400">
                                        No Image
                                    </div>
                                )}
                            </div>
                            <div className="p-4">
                                <h3 className="text-lg font-medium text-gray-900">{product.name}</h3>
                                <p className="mt-1 text-sm text-gray-500">{product.categoryName}</p>
                                <div className="mt-3 flex items-center justify-between">
                                    <span className="text-xl font-bold text-gray-900">${product.price}</span>
                                    <button className="text-blue-600 font-medium text-sm hover:text-blue-800">
                                        View Details
                                    </button>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default HomePage;
