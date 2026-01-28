// Cart logic ----------------------------------------------------------
const cart = JSON.parse(localStorage.getItem('bjCart')) || [];
const badge   = document.getElementById('cartBadge');
const bar     = document.getElementById('checkoutBar');
const totalEl = document.getElementById('checkoutTotal');

function renderCart() {
    const qty   = cart.reduce((sum, item) => sum + item.qty, 0);
    const total = cart.reduce((sum, item) => sum + (item.qty * item.price), 0);
    if (badge) badge.textContent = qty;
    if (totalEl) totalEl.textContent = `Total: ${total.toLocaleString('en-ZA', { style: 'currency', currency: 'ZAR' })}`;
    if (bar) bar.classList.toggle('d-none', qty === 0);
}

// Add-to-cart buttons -------------------------------------------------
document.addEventListener('DOMContentLoaded', () => {
    renderCart();                       // show on first load
    document.querySelectorAll('.add-to-cart').forEach(btn => {
        btn.addEventListener('click', e => {
            const btnEl = e.currentTarget;
            const id    = btnEl.dataset.id;
            const name  = btnEl.dataset.name;
            const price = parseFloat(btnEl.dataset.price);

            const existing = cart.find(x => x.id === id);
            existing ? existing.qty++ : cart.push({ id, name, price, qty: 1 });
            localStorage.setItem('bjCart', JSON.stringify(cart));
            renderCart();

            // fly animation
            const img = btnEl.closest('.card').querySelector('img');
            if (!img) return;
            const clone = img.cloneNode();
            clone.classList.add('fly-to-cart');
            document.body.appendChild(clone);
            gsap.to(clone, {
                x: document.getElementById('cartToggle').offsetLeft - clone.offsetLeft,
                y: document.getElementById('cartToggle').offsetTop  - clone.offsetTop,
                scale: 0.1, opacity: 0, duration: 0.7,
                onComplete: () => clone.remove()
            });

            // toast
            Swal.fire({ toast: true, position: 'top-end', icon: 'success',
                        title: 'Added to cart', showConfirmButton: false, timer: 1200 });
        });
    });
});

// Checkout click ------------------------------------------------------
function checkout() {
    const items = cart.map(it => ({
        BurgerId: it.id,
        BurgerName: it.name,
        Qty: it.qty,
        UnitPrice: it.price,
        AddBeer: false
    }));
    if (items.length === 0) {
        Swal.fire('Cart empty', 'Add burgers first', 'info');
        return;
    }
    // simple GET → receipt page
    window.location = '/Order/Order?itemsJson=' + encodeURIComponent(JSON.stringify(items));
}