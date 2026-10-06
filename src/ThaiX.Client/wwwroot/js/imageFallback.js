(function () {
    if (window.__ThaiXImageFallbackRegistered) {
        return;
    }
    window.__ThaiXImageFallbackRegistered = true;

    document.addEventListener(
        'error',
        function (event) {
            var el = event.target;
            if (!(el instanceof HTMLImageElement)) {
                return;
            }
            if (el.dataset.fallbackApplied === '1') {
                return;
            }
            el.dataset.fallbackApplied = '1';
            el.removeAttribute('src');
            el.alt = el.alt || '';
            el.style.visibility = 'hidden';
            el.setAttribute('aria-hidden', 'true');
        },
        true
    );
})();
