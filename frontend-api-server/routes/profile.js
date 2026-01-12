const express = require('express');
const router = express.Router();
const axios = require('axios');

const API_URL = process.env.API_URL || 'http://localhost:5277/api';

// Middleware: Auth Check
function requireAuth(req, res, next) {
    if (!req.session.user) {
        return res.redirect('/auth/login?redirect=' + req.originalUrl);
    }
    next();
}

// Helper: Get headers
function getHeaders(req) {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${req.session.token}`
    };
}

// Profil Sayfası
router.get('/', requireAuth, (req, res) => {
    res.render('pages/profile/index', { title: 'Profilim' });
});

// Adreslerim Listesi
router.get('/addresses', requireAuth, async (req, res) => {
    try {
        const response = await axios.get(`${API_URL}/address/my-addresses`, {
            headers: getHeaders(req)
        });
        res.render('pages/profile/addresses', {
            title: 'Adreslerim',
            addresses: response.data || []
        });
    } catch (error) {
        console.error('Addresses error:', error.message);
        res.render('pages/profile/addresses', { title: 'Adreslerim', addresses: [] });
    }
});

// Yeni Adres Formu
router.get('/addresses/new', requireAuth, (req, res) => {
    res.render('pages/profile/address-form', { title: 'Yeni Adres', address: null });
});

// Adres Düzenle Formu
router.get('/addresses/:id/edit', requireAuth, async (req, res) => {
    try {
        const { id } = req.params;
        const response = await axios.get(`${API_URL}/address/${id}`, {
            headers: getHeaders(req)
        });
        res.render('pages/profile/address-form', { title: 'Adres Düzenle', address: response.data });
    } catch (error) {
        res.redirect('/profile/addresses');
    }
});

// --- ADRES KAYDET (Ekleme & Güncelleme) ---
router.post('/addresses', requireAuth, async (req, res) => {
    try {
        const { id, title, city, district, fullAddress, zipCode, isDefault } = req.body;

        const addressData = {
            Title: title,
            City: city,
            District: district,
            FullAddress: fullAddress,
            ZipCode: zipCode || "",
            IsDefault: isDefault === 'on' || isDefault === 'true' || isDefault === true
        };

        if (id && id !== "") {
            addressData.Id = parseInt(id); 
            await axios.put(`${API_URL}/address`, addressData, { headers: getHeaders(req) });
        } else {
            await axios.post(`${API_URL}/address`, addressData, { headers: getHeaders(req) });
        }
        res.redirect('/profile/addresses');
    } catch (error) {
        console.error('Save address error:', error.response?.data || error.message);
        res.redirect('/profile/addresses');
    }
});

// --- ADRES SİL ---
router.post('/addresses/:id/delete', requireAuth, async (req, res) => {
    try {
        const { id } = req.params;
        console.log(`Silme işlemi başlatıldı, ID: ${id}`);

        await axios.delete(`${API_URL}/address/${id}`, {
            headers: getHeaders(req)
        });

        res.redirect('/profile/addresses');
    } catch (error) {
        console.error('Delete address error:', error.response?.data || error.message);
        res.redirect('/profile/addresses');
    }
});

// Favorilerim
router.get('/favorites', requireAuth, async (req, res) => {
    try {
        const response = await axios.get(`${API_URL}/favorites/my-favorites`, {
            headers: getHeaders(req)
        });
        res.render('pages/profile/favorites', { title: 'Favorilerim', favorites: response.data || [] });
    } catch (error) {
        res.render('pages/profile/favorites', { title: 'Favorilerim', favorites: [] });
    }
});

// Şifre Değiştir
router.post('/change-password', requireAuth, async (req, res) => {
    try {
        const { currentPassword, newPassword, confirmNewPassword } = req.body;
        if (newPassword !== confirmNewPassword) return res.status(400).json({ error: 'Şifreler uyuşmuyor.' });

        await axios.post(`${API_URL}/Auth/change-password`, {
            currentPassword, newPassword, confirmNewPassword
        }, { headers: getHeaders(req) });

        res.json({ message: 'Şifreniz güncellendi.' });
    } catch (error) {
        res.status(error.response?.status || 500).json({ error: error.response?.data || 'Hata oluştu.' });
    }
});

module.exports = router;