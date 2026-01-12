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

// Siparişlerim
router.get('/', requireAuth, async (req, res) => {
    try {
        const response = await axios.get(`${API_URL}/order/my-orders`, {
            headers: getHeaders(req)
        });

        res.render('pages/orders/index', {
            title: 'Siparişlerim',
            orders: response.data || []
        });
    } catch (error) {
        console.error('Orders error:', error.message);
        res.render('pages/orders/index', {
            title: 'Siparişlerim',
            orders: []
        });
    }
});

// Sipariş Detay
router.get('/:id', requireAuth, async (req, res) => {
    try {
        const { id } = req.params;
        const { success } = req.query;

        const [orderRes, trackRes] = await Promise.all([
            axios.get(`${API_URL}/order/detail/${id}`, { headers: getHeaders(req) }),
            axios.get(`${API_URL}/logistics/track/${id}`, { headers: getHeaders(req) }).catch(() => ({ data: null }))
        ]);

        res.render('pages/orders/detail', {
            title: `Sipariş #${id}`,
            order: orderRes.data,
            tracking: trackRes.data,
            showSuccess: success === '1'
        });
    } catch (error) {
        console.error('Order detail error:', error.message);
        res.redirect('/orders');
    }
});

module.exports = router;
