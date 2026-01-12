const express = require('express');
const router = express.Router();
const axios = require('axios');

const API_URL = process.env.API_URL || 'http://localhost:5277/api';

// Ana Sayfa
router.get('/', async (req, res) => {
    try {
        // Vitrin ürünleri ve son eklenenler
        const [showcaseRes, lastAddedRes, categoriesRes] = await Promise.all([
            axios.get(`${API_URL}/products/showcase/1`).catch(() => ({ data: [] })),
            axios.get(`${API_URL}/products/last-added/8`).catch(() => ({ data: [] })),
            axios.get(`${API_URL}/categories`).catch(() => ({ data: [] }))
        ]);

        res.render('pages/home', {
            title: 'Ana Sayfa',
            showcaseProducts: showcaseRes.data || [],
            lastAddedProducts: lastAddedRes.data || [],
            categories: categoriesRes.data || []
        });
    } catch (error) {
        console.error('Home page error:', error.message);
        res.render('pages/home', {
            title: 'Ana Sayfa',
            showcaseProducts: [],
            lastAddedProducts: [],
            categories: []
        });
    }
});

module.exports = router;
