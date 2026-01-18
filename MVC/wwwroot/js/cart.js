const Toast = Swal.mixin({
    toast: true,
    position: 'bottom-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    background: '#343a40', // Bootstrap dark background
    color: '#f8f9fa',      // Bootstrap light text
    didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer)
        toast.addEventListener('mouseleave', Swal.resumeTimer)
    }
});

document.addEventListener('DOMContentLoaded', function () {
    updateCartItemCount();

    document.body.addEventListener('click', function (e) {
        const button = e.target.closest('.add-to-cart');
        if (button) {
            const productId = button.dataset.productId;
            const productName = button.dataset.productName;
            const quantityInput = button.closest('.input-group').querySelector('.product-quantity');
            const quantity = parseInt(quantityInput.value, 10) || 1;
            
            addToCart(productId, quantity, productName);
        }
    });
});

function updateCartItemCount() {
    fetch('/Cart/Count')
        .then(response => response.json())
        .then(data => {
            const cartCountElement = document.getElementById('cart-item-count');
            if (cartCountElement) {
                cartCountElement.innerText = data.count;
                cartCountElement.style.display = data.count > 0 ? 'inline-block' : 'none';
            }
        })
        .catch(error => console.error('Unable to get cart count.', error));
}

function addToCart(productId, quantity, productName) {
    fetch('/Cart/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ productId: productId, quantity: quantity })
    })
    .then(response => {
        if (response.ok) {
            updateCartItemCount();
            Toast.fire({
                icon: 'success',
                title: `${quantity}x ${productName || 'Item'} added to cart!`
            });
        } else {
            Toast.fire({
                icon: 'error',
                title: 'Could not add product'
            });
        }
    })
    .catch(error => {
        console.error('Error adding to cart:', error);
        Toast.fire({
            icon: 'error',
            title: 'An unexpected error occurred'
        });
    });
}