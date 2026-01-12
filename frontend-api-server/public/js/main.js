// ========================================
// Terzi E-Ticaret - Main JavaScript
// ========================================

const API_URL = '';

// Toast Notification (SweetAlert2)
function showToast(message, type = 'success') {
    Swal.fire({
        text: message,
        icon: type,
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });
}

// Get Session ID for guest cart
function getSessionId() {
    let sessionId = localStorage.getItem('sessionId');
    if (!sessionId) {
        sessionId = 'guest-' + Math.random().toString(36).substr(2, 9);
        localStorage.setItem('sessionId', sessionId);
    }
    document.cookie = `sessionId=${sessionId}; path=/; max-age=${60 * 60 * 24 * 7}`;
    return sessionId;
}

// API Request Helper
async function apiRequest(endpoint, options = {}) {
    const sessionId = getSessionId();
    const headers = {
        'Content-Type': 'application/json',
        'X-Session-Id': sessionId,
        ...options.headers
    };

    try {
        const response = await fetch(endpoint, {
            ...options,
            headers
        });

        const contentType = response.headers.get("content-type");
        let data = {};
        if (contentType && contentType.indexOf("application/json") !== -1) {
            data = await response.json();
        }

        if (!response.ok) {
            let limitMsg = data.message || 'Bir hata oluştu';
            if (data.error) limitMsg = data.error;
            else if (data.errors) {
                const firstKey = Object.keys(data.errors)[0];
                if (firstKey && data.errors[firstKey].length > 0) limitMsg = data.errors[firstKey][0];
            }
            throw new Error(limitMsg);
        }

        return data;
    } catch (error) {
        console.error('Frontend API Error:', error);
        throw error;
    }
}

// Add to Cart
async function addToCart(productId, quantity = 1, variantId = null) {
    try {
        const payload = { productId, quantity };
        if (variantId) payload.variantId = parseInt(variantId);

        await apiRequest('/cart/add', {
            method: 'POST',
            body: JSON.stringify(payload)
        });

        updateCartBadge();
        showToast('Ürün sepete eklendi');
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Update Cart Quantity
async function updateCartQuantity(cartItemId, quantity) {
    try {
        await apiRequest('/cart/update-quantity', {
            method: 'PUT',
            body: JSON.stringify({ cartItemId, newQuantity: quantity })
        });
        updateCartBadge();
        location.reload();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Remove from Cart
async function removeFromCart(cartItemId) {
    try {
        await apiRequest(`/cart/remove/${cartItemId}`, { method: 'DELETE' });
        updateCartBadge();
        showToast('Ürün sepetten kaldırıldı');
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Clear Cart
async function clearCart() {
    try {
        await apiRequest('/cart/clear', { method: 'DELETE' });
        updateCartBadge();
        showToast('Sepet temizlendi');
    } catch (error) {
        showToast(error.message, 'error');
    }
}

// Update Cart Badge
async function updateCartBadge() {
    try {
        const cart = await apiRequest('/cart/data');
        const count = cart.items ? cart.items.length : 0;
        const badges = document.querySelectorAll('.cart-badge');
        badges.forEach(badge => {
            badge.textContent = count;
            badge.style.display = count > 0 ? 'flex' : 'none';
        });
    } catch (error) {
        // Silent error
    }
}

// Quantity Selector Logic
function initQuantitySelectors() {
    document.querySelectorAll('.quantity-selector').forEach(selector => {
        const minusBtn = selector.querySelector('.btn-minus, .btn-qty:first-child');
        const plusBtn = selector.querySelector('.btn-plus, .btn-qty:last-child');
        const input = selector.querySelector('input');

        if (!input) return;

        if (minusBtn) {
            minusBtn.addEventListener('click', async () => {
                let val = parseInt(input.value) || 0;
                if (val > 1) {
                    const newVal = val - 1;
                    input.value = newVal;
                    const cartItemId = input.dataset.cartItemId;
                    if (cartItemId) {
                        await updateCartQuantity(cartItemId, newVal);
                    }
                }
            });
        }

        if (plusBtn) {
            plusBtn.addEventListener('click', async () => {
                let val = parseInt(input.value) || 0;
                const newVal = val + 1;
                input.value = newVal;
                const cartItemId = input.dataset.cartItemId;
                if (cartItemId) {
                    await updateCartQuantity(cartItemId, newVal);
                }
            });
        }

        input.addEventListener('change', async () => {
            let val = parseInt(input.value) || 1;
            if (val < 1) val = 1;
            input.value = val;
            const cartItemId = input.dataset.cartItemId;
            if (cartItemId) {
                await updateCartQuantity(cartItemId, val);
            }
        });
    });
}

// Initialize on DOM Ready
document.addEventListener('DOMContentLoaded', () => {
    initQuantitySelectors();
    updateCartBadge();

    // Confirm Dialog (Custom SweetAlert)
    window.confirmAction = async function (message) {
        const result = await Swal.fire({
            title: 'Emin misiniz?',
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3d3d3d',
            cancelButtonColor: '#7a7a7a',
            confirmButtonText: 'Evet',
            cancelButtonText: 'Hayır'
        });
        return result.isConfirmed;
    };

    // Add to cart buttons listener
    document.body.addEventListener('click', async (e) => {
        const btn = e.target.closest('.btn-add-to-cart');
        if (btn) {
            e.preventDefault();
            const form = btn.closest('form');
            let productId, quantity, variantId = null;

            if (form) {
                const formData = new FormData(form);
                productId = formData.get('productId');
                quantity = formData.get('quantity') || 1;
                variantId = formData.get('variantId');
                const qtyInput = form.querySelector('input[name="quantity"]');
                if (qtyInput) quantity = qtyInput.value;
            } else {
                productId = btn.dataset.productId;
                quantity = btn.dataset.quantity || 1;
                variantId = btn.dataset.variantId;
            }

            if (!productId) {
                showToast('Ürün bilgisi bulunamadı', 'error');
                return;
            }

            const originalText = btn.innerHTML;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
            btn.disabled = true;

            try {
                await addToCart(productId, quantity, variantId);
            } catch (error) {
                console.error('Add to cart error:', error);
            } finally {
                btn.innerHTML = originalText;
                btn.disabled = false;
            }
        }
    });

    // Remove buttons logic
    document.body.addEventListener('click', async (e) => {
        const btn = e.target.closest('.btn-remove-from-cart');
        if (btn) {
            e.preventDefault();
            const cartItemId = btn.dataset.cartItemId;
            if (cartItemId) {
                const confirmed = await window.confirmAction('Ürünü sepetten kaldırmak istediğinize emin misiniz?');
                if (confirmed) {
                    await removeFromCart(cartItemId);
                    location.reload();
                }
            }
        }
    });

    // Clear cart logic
    document.body.addEventListener('click', async (e) => {
        const btn = e.target.closest('.btn-clear-cart');
        if (btn) {
            e.preventDefault();
            const confirmed = await window.confirmAction('Sepetinizi tamamen boşaltmak istediğinize emin misiniz?');
            if (confirmed) {
                await clearCart();
                location.reload();
            }
        }
    });
});