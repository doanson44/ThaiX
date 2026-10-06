(function () {
    const STORAGE_KEY = 'ThaiX-theme';
    const RADZEN_DARK = '_content/Radzen.Blazor/css/standard-dark-base.css';
    const RADZEN_LIGHT = '_content/Radzen.Blazor/css/standard-base.css';
    const HLJS_DARK = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css';
    const HLJS_LIGHT = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css';

    function normalize(theme) {
        return theme === 'light' ? 'light' : 'dark';
    }

    function applyBodyClasses(theme) {
        const body = document.body;
        if (!body) return;
        body.classList.toggle('ThaiX-theme-light', theme === 'light');
        body.classList.toggle('ThaiX-theme-dark', theme === 'dark');
    }

    window.ThaiXTheme = {
        get() {
            try {
                return normalize(window.localStorage.getItem(STORAGE_KEY));
            } catch {
                return 'dark';
            }
        },

        apply(theme) {
            const t = normalize(theme);
            document.documentElement.setAttribute('data-bs-theme', t);

            const rz = document.getElementById('ThaiX-radzen-theme');
            if (rz) {
                rz.href = t === 'light' ? RADZEN_LIGHT : RADZEN_DARK;
            }

            const hl = document.getElementById('ThaiX-hljs-theme');
            if (hl) {
                hl.href = t === 'light' ? HLJS_LIGHT : HLJS_DARK;
            }

            applyBodyClasses(t);
            return t;
        },

        set(theme) {
            const t = normalize(theme);
            try {
                window.localStorage.setItem(STORAGE_KEY, t);
            } catch {
                /* ignore quota / private mode */
            }
            return this.apply(t);
        },

        init() {
            return this.apply(this.get());
        }
    };

    window.ThaiXTheme.init();

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => applyBodyClasses(window.ThaiXTheme.get()));
    }
})();
