document.addEventListener('DOMContentLoaded', function () {
    updateCartItemCount();

    document.body.addEventListener('click', function (e) {
        if (e.target.classList.contains('add-to-cart')) {
            const button = e.target;
            const productId = button.dataset.productId;
            
            addToCart(productId);
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

function addToCart(productId) {
    fetch('/Cart/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ productId: productId, quantity: 1 })
    })
    .then(response => {
        if (response.ok) {
            updateCartItemCount();
            Swal.fire('Added!', 'Product has been added to your cart.', 'success');
        } else {
            Swal.fire('Error!', 'Could not add product to cart.', 'error');
        }
    })
    .catch(error => {
        console.error('Error adding to cart:', error);
        Swal.fire('Error!', 'An unexpected error occurred.', 'error');
    });
}