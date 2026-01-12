const express = require('express');
const router = express.Router();
const axios = require('axios');

// C# API URL'i (HTTPS Portu - Swagger adresinle aynı olmalı)
const API_URL = process.env.API_URL || 'https://localhost:44391/api';

// Localhost SSL sertifika hatalarını yoksay (Geliştirme ortamı için kritik)
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

// ============================================================
// YARDIMCI FONKSİYON: API İSTEKLERİNİ YÖNETME (PROXY)
// ============================================================
const forwardToApi = async (req, res, method, endpoint, data = null) => {
    try {
        // 1. Token: Kullanıcı giriş yapmışsa session'dan al
        const token = req.session.token;

        // 2. Session ID: Frontend header'dan gönderir, yoksa session'dan, yoksa varsayılan
        // Frontend 'X-Session-Id' header'ı gönderiyor (main.js'den)
        let sessionId = req.headers['x-session-id'] || req.session.sessionId;

        // Eğer header'dan yeni bir ID geldiyse ve session'da yoksa kaydet
        if (req.headers['x-session-id']) {
            req.session.sessionId = req.headers['x-session-id'];
            sessionId = req.headers['x-session-id'];
        }

        if (!sessionId) sessionId = 'guest-temp';

        const config = {
            method: method,
            url: `${API_URL}${endpoint}`,
            headers: {
                'Content-Type': 'application/json',
                'X-Session-Id': sessionId
            },
            data: data
        };

        // Eğer token varsa header'a ekle (Üye sepeti işlemleri için)
        if (token) {
            config.headers['Authorization'] = `Bearer ${token}`;
        }

        // C# API'ye isteği gönder
        const response = await axios(config);

        // Başarılı cevabı frontend'e olduğu gibi ilet
        res.status(response.status).json(response.data);

    } catch (error) {
        // Hata Detaylarını Yakala ve Düzenle
        const status = error.response?.status || 500;
        const apiErrorData = error.response?.data;

        console.error(`API Error [${method} ${endpoint}]:`, apiErrorData || error.message);

        // Frontend'in (main.js) anlayacağı formatta hata mesajı döndür
        let errorMessage = 'İşlem başarısız oldu.';

        if (apiErrorData) {
            if (typeof apiErrorData === 'string') errorMessage = apiErrorData;
            else if (apiErrorData.error) errorMessage = apiErrorData.error;
            else if (apiErrorData.message) errorMessage = apiErrorData.message;
            else if (apiErrorData.title) errorMessage = apiErrorData.title; // .NET default hatası
            else if (apiErrorData.errors) {
                // Validation hatalarını yakala
                const firstKey = Object.keys(apiErrorData.errors)[0];
                if (firstKey) errorMessage = apiErrorData.errors[firstKey][0];
            }
        }

        res.status(status).json({ error: errorMessage });
    }
};

// ============================================================
// ROTALAR
// ============================================================

// 1. SEPET SAYFASI (HTML RENDER)
// Tarayıcıda /cart adresine gidince çalışır
router.get('/', async (req, res) => {
    try {
        const token = req.session.token;
        // Sayfa ilk yüklenirken header'dan session gelmez, login/ekleme anında kaydedilen session'dan bakarız
        const sessionId = req.session.sessionId || 'guest-temp';

        const config = {
            headers: { 'X-Session-Id': sessionId }
        };
        if (token) config.headers['Authorization'] = `Bearer ${token}`;

        // Sepet verisini C# API'den çek
        const response = await axios.get(`${API_URL}/Cart`, config);

        // EJS sayfasını render et
        res.render('pages/cart/index', {
            title: 'Sepetim',
            cart: response.data || { items: [], totalPrice: 0 },
            user: req.session.user // Navbar için kullanıcı bilgisi
        });

    } catch (error) {
        console.error('Sepet sayfası yüklenirken hata:', error.message);
        // Hata olsa bile sayfayı aç, boş sepet göster (Sayfa patlamasın)
        res.render('pages/cart/index', {
            title: 'Sepetim',
            cart: { items: [], totalPrice: 0 },
            user: req.session.user
        });
    }
});

// 2. SEPET VERİSİ (JSON)
// Rozet (Badge) güncellemek için main.js tarafından çağrılır
router.get('/data', async (req, res) => {
    await forwardToApi(req, res, 'GET', '/Cart');
});

// 3. SEPETE EKLE (POST) -> /cart/add
router.post('/add', async (req, res) => {
    // req.body içinde { productId, quantity, variantId } gelir
    await forwardToApi(req, res, 'POST', '/Cart/add', req.body);
});

// 4. MİKTAR GÜNCELLE (PUT) -> /cart/update-quantity
router.put('/update-quantity', async (req, res) => {
    await forwardToApi(req, res, 'PUT', '/Cart/update-quantity', req.body);
});

// 5. ÜRÜN SİL (DELETE) -> /cart/remove/:id
router.delete('/remove/:id', async (req, res) => {
    await forwardToApi(req, res, 'DELETE', `/Cart/remove/${req.params.id}`);
});

// 6. SEPETİ TEMİZLE (DELETE) -> /cart/clear
router.delete('/clear', async (req, res) => {
    await forwardToApi(req, res, 'DELETE', '/Cart/clear');
});

// 7. ÖDEME SAYFASI (CHECKOUT)
router.get('/checkout', async (req, res) => {
    // Kullanıcı giriş yapmamışsa login'e yönlendir
    if (!req.session.user) {
        return res.redirect('/auth/login?returnUrl=/cart/checkout');
    }

    try {
        // Sepet ve Adres bilgilerini çekip sayfaya gönder
        const token = req.session.token;
        const config = { headers: { Authorization: `Bearer ${token}` } };

        const [cartRes, addressRes] = await Promise.all([
            axios.get(`${API_URL}/Cart`, config),
            axios.get(`${API_URL}/Address/my-addresses`, config)
        ]);

        res.render('pages/cart/checkout', {
            title: 'Ödeme',
            cart: cartRes.data,
            addresses: addressRes.data,
            user: req.session.user
        });
    } catch (error) {
        console.error('Checkout sayfası hatası:', error.message);
        res.redirect('/cart');
    }
});

module.exports = router;