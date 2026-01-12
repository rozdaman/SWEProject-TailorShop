const express = require('express');
const session = require('express-session');
const path = require('path');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;
const API_URL = process.env.API_URL || 'http://localhost:5277/api';




// Middleware
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(express.static(path.join(__dirname, 'public')));

// Session
app.use(session({
    secret: 'terzi-secret-key-2024',
    resave: false,
    saveUninitialized: true,
    cookie: { maxAge: 24 * 60 * 60 * 1000 } // 1 gün
}));

// View Engine
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

// Global variables
app.use(async (req, res, next) => {
    res.locals.user = req.session.user || null;
    res.locals.token = req.session.token || null;
    res.locals.API_URL = API_URL;
    res.locals.cartCount = req.session.cartCount || 0;

    // Fetch categories for Navbar
    try {
        const axios = require('axios');
        const categoriesRes = await axios.get(`${API_URL}/categories`);
        res.locals.categories = categoriesRes.data || [];
    } catch (error) {
        res.locals.categories = [];
    }

    next();
});

// Routes
const indexRoutes = require('./routes/index');
const authRoutes = require('./routes/auth');
const productRoutes = require('./routes/products');
const cartRoutes = require('./routes/cart');
const orderRoutes = require('./routes/orders');
const profileRoutes = require('./routes/profile');
const adminRoutes = require('./routes/admin');

app.use('/', indexRoutes);
app.use('/auth', authRoutes);
app.use('/products', productRoutes);
app.use('/cart', cartRoutes);
app.use('/orders', orderRoutes);
app.use('/profile', profileRoutes);
app.use('/admin', adminRoutes);

// 404 Handler
app.use((req, res) => {
    res.status(404).render('pages/404', { title: 'Sayfa Bulunamadı' });
});

// Error Handler
app.use((err, req, res, next) => {
    console.error(err.stack);
    res.status(500).render('pages/error', { title: 'Hata', error: err.message });
});

app.listen(PORT, () => {
    console.log(`Terzi E-Ticaret sunucusu ${PORT} portunda çalışıyor`);
    console.log(`http://localhost:${PORT}`);
});
