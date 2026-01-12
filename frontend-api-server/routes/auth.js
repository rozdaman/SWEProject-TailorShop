const express = require('express');
const router = express.Router();
const axios = require('axios');

const API_URL = process.env.API_URL || 'http://localhost:5277/api';

// Giriş Sayfası
router.get('/login', (req, res) => {
    if (req.session.user) {
        return res.redirect('/');
    }
    res.render('pages/auth/login', {
        title: 'Giriş Yap',
        error: null
    });
});

// Giriş İşlemi
router.post('/login', async (req, res) => {
    try {
        const { email, password } = req.body;

        const response = await axios.post(`${API_URL}/auth/login`, {
            email,
            password
        });

        // Backend yanıt yapısı: { token, expireDate, userId, username, role }
        const data = response.data;

        req.session.token = data.token;
        req.session.user = {
            id: data.userId,
            email: email,
            username: data.username,
            role: data.role
        };

        res.redirect('/');
    } catch (error) {
        const message = error.response?.data?.message || error.response?.data || 'Giriş yapılamadı';
        res.render('pages/auth/login', {
            title: 'Giriş Yap',
            error: typeof message === 'object' ? JSON.stringify(message) : message
        });
    }
});

// Kayıt Sayfası
router.get('/register', (req, res) => {
    if (req.session.user) {
        return res.redirect('/');
    }
    res.render('pages/auth/register', {
        title: 'Kayıt Ol',
        error: null
    });
});

// Kayıt İşlemi
router.post('/register', async (req, res) => {
    try {
        const { firstName, lastName, userName, email, password, confirmPassword } = req.body;

        await axios.post(`${API_URL}/auth/register`, {
            firstName,
            lastName,
            userName,
            email,
            password,
            confirmPassword
        });

        res.redirect('/auth/login?registered=1');
    } catch (error) {

        let message = 'Kayıt yapılamadı';

        if (error.response?.data) {
            const data = error.response.data;

            // 1. Array formatı: [{ code: '...', description: '...' }]
            if (Array.isArray(data)) {

                message = data.map(e => e.description || e.message || JSON.stringify(e)).join('<br>');
            }
            // 2. Mesaj içinde array varsa: { message: [...] }
            else if (Array.isArray(data.message)) {
                message = data.message.map(e => e.description || e).join('<br>');
            }
            // 3. Tekil mesaj string
            else if (typeof data === 'string') {
                message = data;
            }
            // 4. Object ama 'message' property var
            else if (data.message) {
                message = data.message;
            }
            // 5. 'errors' objesi varsa (ASP.NET validation default)
            else if (data.errors) {
                message = Object.values(data.errors).flat().join('<br>');
            }
        }

        res.render('pages/auth/register', {
            title: 'Kayıt Ol',
            error: message
        });
    }
});

// Çıkış
router.get('/logout', (req, res) => {
    req.session.destroy();
    res.redirect('/');
});

module.exports = router;
