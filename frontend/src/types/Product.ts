export interface Category {
    categoryId: number;
    name: string;
}

export interface Product {
    id: number;
    name: string;
    description?: string;
    price: number;
    imageUrl?: string;
    stockQuantity?: number; // Deduced from context, might be in Stock object
    category?: Category;
    productType?: number; // Enum: 0 = Fabric, 1 = ReadyToWear
}

export interface ProductListDto {
    id: number;
    name: string;
    price: number;
    imageUrl?: string;
    categoryName?: string;
}
