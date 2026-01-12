import api from './api';
import { Product, ProductListDto } from '../types/Product';

export const productService = {
    getAll: async () => {
        const response = await api.get<ProductListDto[]>('/products/getall');
        return response.data;
    },

    getById: async (id: number) => {
        const response = await api.get<Product>(`/products/${id}`);
        return response.data;
    },

    search: async (keyword: string) => {
        const response = await api.get<ProductListDto[]>('/products/search', {
            params: { keyword }
        });
        return response.data;
    },

    getLastAdded: async (count: number = 10) => {
        const response = await api.get<ProductListDto[]>(`/products/last-added/${count}`);
        return response.data;
    },

    // Showcase endpoints
    getShowcase: async (displayType: number) => {
        const response = await api.get<ProductListDto[]>(`/products/showcase/${displayType}`);
        return response.data;
    }
};
