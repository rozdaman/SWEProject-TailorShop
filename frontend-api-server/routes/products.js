const express = require('express');
const router = express.Router();
const axios = require('axios');

const API_URL = process.env.API_URL || 'http://localhost:5277/api';

// Tüm Ürünler
router.get('/', async (req, res) => {
    try {
        const { page = 1, type, category, minPrice, maxPrice, keyword } = req.query;
        
        // 1. ADIM: Doğru API endpoint'ini seçiyoruz
        // Backend'deki özel filtreleri (type, search) öncelikli kullanıyoruz
        let apiUrl = `${API_URL}/products/getall`;

        if (type !== undefined && type !== '' && !isNaN(type)) {
            // Aksesuar (3), Kalıp (2) vb. için doğrudan tipi sorguluyoruz
            apiUrl = `${API_URL}/products/filter-type?type=${type}`;
        } else if (keyword) {
            // Arama yapılıyorsa arama endpoint'ine gidiyoruz
            apiUrl = `${API_URL}/products/search?keyword=${encodeURIComponent(keyword)}`;
        }

        // Seçilen endpoint'ten veriyi çekiyoruz
        const response = await axios.get(apiUrl);
        let products = response.data || [];

        // 2. ADIM: Node.js tarafında ek filtreleri (Kategori ve Fiyat) uyguluyoruz
        // Kategori filtresi (Eğer bir type seçilmemişse veya kategoriye özel bakılıyorsa)
        if (category && category !== 'undefined' && category !== '') {
            const catId = Number(category);
            products = products.filter(p => (p.categoryId || p.CategoryId) == catId);
        }

        // Fiyat filtreleri
        if (minPrice) {
            products = products.filter(p => (p.price || p.Price) >= parseFloat(minPrice));
        }
        if (maxPrice) {
            products = products.filter(p => (p.price || p.Price) <= parseFloat(maxPrice));
        }

        // Sidebar için kategorileri çekiyoruz
        const categoriesRes = await axios.get(`${API_URL}/categories`).catch(() => ({ data: [] }));

        res.render('pages/products/index', {
            title: 'Tüm Koleksiyon',
            products,
            categories: categoriesRes.data || [],
            currentPage: parseInt(page),
            totalPages: 1,
            filters: { type, category, minPrice, maxPrice, keyword }
        });
    } catch (error) {
        console.error('Products error:', error.message);
        res.render('pages/products/index', {
            title: 'Ürünler',
            products: [],
            categories: [],
            currentPage: 1,
            totalPages: 1,
            filters: {}
        });
    }
});

// [YENİ] KATEGORİYE GÖRE ÜRÜNLERİ GETİRME ROTASI (Artık dışarıda)
router.get('/category/:id', async (req, res) => {
    try {
        const categoryId = req.params.id;
        res.redirect(`/products?category=${categoryId}`);
    } catch (error) {
        res.redirect('/products');
    }
});
// Arama
router.get('/search', async (req, res) => {
    const { keyword } = req.query;
    res.redirect(`/products?keyword=${encodeURIComponent(keyword || '')}`);
});

// Kategoriler Sayfası
router.get('/categories', async (req, res) => {
    try {
        const response = await axios.get(`${API_URL}/categories`);
        res.render('pages/products/categories', {
            title: 'Kategoriler',
            categories: response.data || []
        });
    } catch (error) {
        res.render('pages/products/categories', {
            title: 'Kategoriler',
            categories: []
        });
    }
});



// Ürün Detay
router.get('/:id', async (req, res) => {
    try {
        const { id } = req.params;
        const response = await axios.get(`${API_URL}/products/${id}`);

        if (!response.data) {
            return res.redirect('/products');
        }

        console.log('DEBUG: Product Data for Detail Page:', JSON.stringify(response.data, null, 2));

        // İlgili ürünler
        const relatedRes = await axios.get(`${API_URL}/products/getall`).catch(() => ({ data: [] }));
        const relatedProducts = (relatedRes.data || [])
            .filter(p => p.categoryId === response.data.categoryId && p.id != id)
            .slice(0, 4);

        // Favori Durumu Kontrolü (Giriş Yapmışsa)
        let isFavorite = false;
        if (req.session.token) {
            try {
                const favCheck = await axios.get(`${API_URL}/favorites/check/${id}`, {
                    headers: { 'Authorization': `Bearer ${req.session.token}` }
                });
                isFavorite = favCheck.data.isFavorite;
            } catch (err) {
                console.warn('Favorite check error:', err.message);
            }
        }

        res.render('pages/products/detail', {
            title: response.data.name,
            product: response.data,
            relatedProducts,
            isFavorite
        });
    } catch (error) {
        console.error('Product detail error:', error.message);
        res.redirect('/products');
    }
});

module.exports = router;
