const express = require('express');
const router = express.Router();
const axios = require('axios');

// Backend API Adresi (HTTPS - Swagger portuyla aynı olmalı)
const API_URL = process.env.API_URL || 'https://localhost:44391/api';

// Localhost SSL sertifika hatalarını yoksay (Geliştirme ortamı için kritik)
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

// ==========================================
// MIDDLEWARE & HELPERS
// ==========================================

// JWT Token Çözümleyici (Helper)
function parseJwt(token) {
    try {
        return JSON.parse(Buffer.from(token.split('.')[1], 'base64').toString());
    } catch (e) {
        return null;
    }
}

// Middleware: Sadece Admin Girebilir (DEBUG MODU AÇIK)
function requireAdmin(req, res, next) {
    // 1. Oturum veya Token yoksa Login'e at
    if (!req.session.user || !req.session.token) {
        console.log('[Admin Middleware] Oturum yok, Login\'e yönlendiriliyor.');
        return res.redirect('/auth/login?redirect=' + encodeURIComponent(req.originalUrl));
    }

    // 2. Token'ı Decode Et
    const payload = parseJwt(req.session.token);

    if (payload) {
        // Konsola Token İçeriğini Bas (Hata Ayıklama İçin)
        console.log('--- ADMIN YETKİ KONTROLÜ ---');
        console.log('Token Payload:', JSON.stringify(payload, null, 2));

        // .NET Identity Claim İsimleri (Uzun ve Kısa halleri)
        const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || payload['Role'];
        const nameClaim = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload['unique_name'] || payload['name'];

        // Rol Kontrolü (Array veya String olabilir)
        let hasAdminRole = false;
        if (Array.isArray(roleClaim)) {
            hasAdminRole = roleClaim.some(r => r.toLowerCase() === 'admin');
        } else if (typeof roleClaim === 'string') {
            hasAdminRole = roleClaim.toLowerCase() === 'admin';
        }

        // İsim Kontrolü (Yedek olarak isme de bakıyoruz)
        const isUserAdmin = nameClaim && nameClaim.toLowerCase() === 'admin';

        // Session'daki username kontrolü (Eğer backend body'de dönmüyorsa bu undefined olabilir, sorun değil)
        const isSessionAdmin = req.session.user.username && req.session.user.username.toLowerCase() === 'admin';

        console.log(`Kontrol Sonucu -> Role: ${hasAdminRole}, Name: ${isUserAdmin}, Session: ${isSessionAdmin}`);

        if (hasAdminRole || isUserAdmin || isSessionAdmin) {
            console.log('[Admin Middleware] Yetki Onaylandı. Giriş yapılıyor...');
            return next();
        }
    }

    // Yetkisiz ise Ana Sayfaya gönder
    console.warn(`[Yetkisiz Erişim] Kullanıcı: ${req.session.user.email} - Token veya Rol yetersiz.`);
    return res.redirect('/');
}

// Helper: Token içeren Header hazırlar
function getHeaders(req) {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${req.session.token}`
    };
}

// ==========================================
// 1. DASHBOARD (Ana Sayfa)
// ==========================================
router.get('/', requireAdmin, async (req, res) => {
    try {
        const [productsRes, categoriesRes, usersRes] = await Promise.all([
            axios.get(`${API_URL}/products/getall`).catch(() => ({ data: [] })),
            axios.get(`${API_URL}/categories`).catch(() => ({ data: [] })),
            axios.get(`${API_URL}/auth/user-list`, { headers: getHeaders(req) }).catch(() => ({ data: [] }))
        ]);

        res.render('pages/admin/dashboard', {
            title: 'Admin Paneli',
            user: req.session.user,
            stats: {
                products: productsRes.data.length,
                categories: categoriesRes.data.length,
                users: usersRes.data.length,
                orders: 0
            }
        });
    } catch (error) {
        console.error('Dashboard error:', error.message);
        res.render('pages/admin/dashboard', {
            title: 'Admin Paneli',
            user: req.session.user,
            stats: { products: 0, categories: 0, users: 0, orders: 0 }
        });
    }
});

// ==========================================
// 2. ÜRÜN YÖNETİMİ
// ==========================================

router.get('/products', requireAdmin, async (req, res) => {
    try {
        const response = await axios.get(`${API_URL}/products/getall`);
        res.render('pages/admin/products/index', {
            title: 'Ürün Yönetimi',
            products: response.data || [],
            user: req.session.user
        });
    } catch (error) {
        res.render('pages/admin/products/index', {
            title: 'Ürün Yönetimi',
            products: [],
            user: req.session.user
        });
    }
});

router.get('/products/new', requireAdmin, async (req, res) => {
    try {
        const categoriesRes = await axios.get(`${API_URL}/categories`);
        res.render('pages/admin/products/form', {
            title: 'Yeni Ürün',
            product: null,
            categories: categoriesRes.data || [],
            user: req.session.user
        });
    } catch (error) {
        res.redirect('/admin/products');
    }
});

router.get('/products/:id/edit', requireAdmin, async (req, res) => {
    try {
        const { id } = req.params;
        const [productRes, categoriesRes] = await Promise.all([
            axios.get(`${API_URL}/products/${id}`),
            axios.get(`${API_URL}/categories`)
        ]);

        res.render('pages/admin/products/form', {
            title: 'Ürün Düzenle',
            product: productRes.data,
            categories: categoriesRes.data || [],
            user: req.session.user
        });
    } catch (error) {
        res.redirect('/admin/products');
    }
});

router.post('/products', requireAdmin, async (req, res) => {
    try {
        const { id, name, description, price, categoryId, imageUrl, productType, stockQuantity } = req.body;

        const productData = {
            name,
            description,
            price: parseFloat(price),
            categoryId: parseInt(categoryId),
            imageUrl,
            productType: parseInt(productType),
            stockQuantity: stockQuantity ? parseFloat(stockQuantity) : null
        };

        if (id) {
            productData.id = parseInt(id);
            productData.productId = parseInt(id);
            await axios.put(`${API_URL}/products`, productData, { headers: getHeaders(req) });
        } else {
            await axios.post(`${API_URL}/products`, productData, { headers: getHeaders(req) });
        }

        res.redirect('/admin/products');
    } catch (error) {
        console.error('Save product error:', error.message);
        res.redirect('/admin/products?error=save_failed');
    }
});

router.post('/products/:id/delete', requireAdmin, async (req, res) => {
    try {
        const { id } = req.params;
        await axios.delete(`${API_URL}/products/${id}`, { headers: getHeaders(req) });
        res.redirect('/admin/products');
    } catch (error) {
        res.redirect('/admin/products?error=delete_failed');
    }
});

router.post('/products/:id/toggle-status', requireAdmin, async (req, res) => {
    try {
        const { id } = req.params;
        await axios.get(`${API_URL}/products/toggle-status/${id}`, { headers: getHeaders(req) });
        res.redirect('/admin/products');
    } catch (error) {
        res.redirect('/admin/products');
    }
});

router.post('/products/:id/showcase', requireAdmin, async (req, res) => {
    try {
        const { id } = req.params;
        const { displayType } = req.body;

        await axios.post(`${API_URL}/products/add-to-showcase`, {
            productId: parseInt(id),
            displayType: parseInt(displayType) || 1,
            displayOrder: 1,
            imageUrl: null
        }, { headers: getHeaders(req) });

        res.redirect('/admin/products?success=showcase');
    } catch (error) {
        res.redirect('/admin/products?error=showcase_failed');
    }
});

// ==========================================
// 3. KATEGORİ YÖNETİMİ
// ==========================================

router.get('/categories', requireAdmin, async (req, res) => {
    try {
        const response = await axios.get(`${API_URL}/categories`);
        res.render('pages/admin/categories/index', {
            title: 'Kategori Yönetimi',
            categories: response.data || [],
            user: req.session.user
        });
    } catch (error) {
        res.render('pages/admin/categories/index', {
            title: 'Kategori Yönetimi',
            categories: [],
            user: req.session.user
        });
    }
});

router.get('/categories/new', requireAdmin, (req, res) => {
    res.render('pages/admin/categories/form', {
        title: 'Yeni Kategori',
        category: null,
        user: req.session.user
    });
});

router.get('/categories/:id/edit', requireAdmin, async (req, res) => {
    try {
        const { id } = req.params;
        const response = await axios.get(`${API_URL}/categories/${id}`);
        res.render('pages/admin/categories/form', {
            title: 'Kategori Düzenle',
            category: response.data,
            user: req.session.user
        });
    } catch (error) {
        res.redirect('/admin/categories');
    }
});

router.post('/categories', requireAdmin, async (req, res) => {
    try {
        const { id, name, imageUrl, description, parentCategoryId } = req.body;

        const categoryData = {
            name,
            imageUrl: imageUrl || 'https://via.placeholder.com/300x200?text=Kategori',
            description,
            parentCategoryId: parentCategoryId ? parseInt(parentCategoryId) : null
        };

        if (id) {
            categoryData.id = parseInt(id);
            categoryData.categoryId = parseInt(id);
            await axios.put(`${API_URL}/categories`, categoryData, { headers: getHeaders(req) });
        } else {
            await axios.post(`${API_URL}/categories`, categoryData, { headers: getHeaders(req) });
        }

        res.redirect('/admin/categories');
    } catch (error) {
        console.error('Category save error:', error.message);
        res.redirect('/admin/categories');
    }
});

router.post('/categories/:id/delete', requireAdmin, async (req, res) => {
    try {
        const { id } = req.params;
        await axios.delete(`${API_URL}/categories/${id}`, { headers: getHeaders(req) });
        res.redirect('/admin/categories');
    } catch (error) {
        res.redirect('/admin/categories');
    }
});

// ==========================================
// 4. STOK YÖNETİMİ
// ==========================================

router.get('/stock', requireAdmin, async (req, res) => {
    try {
        const response = await axios.get(`${API_URL}/Stock/list`, {
            headers: getHeaders(req)
        });

        // DÜZELTME: Dosya yolu 'pages/admin/stock/index' olmalı
        res.render('pages/admin/stock/index', {
            title: 'Stok Yönetimi',
            stocks: response.data || [],
            user: req.session.user
        });
    } catch (error) {
        console.error('Stok sayfası hatası:', error.message);
        // DÜZELTME: Dosya yolu 'pages/admin/stock/index' olmalı
        res.render('pages/admin/stock/index', {
            title: 'Stok Yönetimi',
            stocks: [],
            user: req.session.user,
            error: 'Stok verileri alınamadı.'
        });
    }
});

router.post('/stock/add', requireAdmin, async (req, res) => {
    try {
        const { productId, quantity, variantId, location } = req.body;

        await axios.post(`${API_URL}/Stock/add-stock`, {
            productId: parseInt(productId),
            quantity: parseFloat(quantity),
            variantId: variantId ? parseInt(variantId) : null,
            location: location || 'Ana Depo'
        }, { headers: getHeaders(req) });

        res.redirect('/admin/stock?success=added');
    } catch (error) {
        res.redirect('/admin/stock?error=add_failed');
    }
});

router.post('/stock/update', requireAdmin, async (req, res) => {
    try {
        const { productId, newQuantity } = req.body;

        await axios.put(`${API_URL}/Products/update-stock`, null, {
            headers: getHeaders(req),
            params: {
                productId: parseInt(productId),
                newQuantity: parseFloat(newQuantity)
            }
        });

        res.redirect('/admin/stock?success=updated');
    } catch (error) {
        res.redirect('/admin/stock?error=update_failed');
    }
});

router.get('/stock/history/:productId', requireAdmin, async (req, res) => {
    try {
        const { productId } = req.params;
        const response = await axios.get(`${API_URL}/Products/stock-history/${productId}`, {
            headers: getHeaders(req)
        });

        // DÜZELTME: Dosya yolu 'pages/admin/stock/history' olmalı
        res.render('pages/admin/stock/history', {
            title: `Stok Geçmişi #${productId}`,
            history: response.data || [],
            productId,
            user: req.session.user
        });
    } catch (error) {
        res.redirect('/admin/stock');
    }
});

// ==========================================
// 5. KULLANICI YÖNETİMİ
// ==========================================

router.get('/users', requireAdmin, async (req, res) => {
    try {
        const response = await axios.get(`${API_URL}/auth/user-list`, { headers: getHeaders(req) });
        res.render('pages/admin/users/index', {
            title: 'Kullanıcı Yönetimi',
            users: response.data || [],
            user: req.session.user
        });
    } catch (error) {
        res.render('pages/admin/users/index', {
            title: 'Kullanıcı Yönetimi',
            users: [],
            user: req.session.user
        });
    }
});

router.post('/users/assign-role', requireAdmin, async (req, res) => {
    try {
        const { userId, roleId, roleExist } = req.body;
        await axios.post(`${API_URL}/auth/assign-role`, {
            userId: parseInt(userId),
            roleId: parseInt(roleId),
            roleExist: roleExist === 'true',
            roleName: 'Admin'
        }, { headers: getHeaders(req) });

        res.redirect('/admin/users');
    } catch (error) {
        res.redirect('/admin/users?error=role_failed');
    }
});

router.post('/roles/create', requireAdmin, async (req, res) => {
    try {
        const { roleName } = req.body;
        await axios.post(`${API_URL}/auth/create-role`, null, {
            headers: getHeaders(req),
            params: { roleName }
        });
        res.redirect('/admin/users');
    } catch (error) {
        res.redirect('/admin/users?error=role_create_failed');
    }
});

// ==========================================
// 6. LOJİSTİK YÖNETİMİ
// ==========================================

router.get('/logistics', requireAdmin, (req, res) => {
    res.render('pages/admin/logistics/index', {
        title: 'Lojistik Yönetimi',
        user: req.session.user
    });
});

router.post('/logistics/create-shipment', requireAdmin, async (req, res) => {
    try {
        const { orderId, carrier, trackingNumber } = req.body;
        await axios.post(`${API_URL}/logistics/create-shipment`, {
            orderId: parseInt(orderId),
            carrier,
            trackingNumber
        }, { headers: getHeaders(req) });

        res.redirect('/admin/logistics?success=1');
    } catch (error) {
        res.redirect('/admin/logistics?error=1');
    }
});

module.exports = router;