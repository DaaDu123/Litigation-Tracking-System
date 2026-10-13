


window.ltsTheme = {
    STORAGE_KEY: 'lts-theme',

    apply: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
    },

    get: function () {
        return localStorage.getItem(window.ltsTheme.STORAGE_KEY) || 'light';
    },

    set: function (theme) {
        localStorage.setItem(window.ltsTheme.STORAGE_KEY, theme);
        window.ltsTheme.apply(theme);
    },

    init: function () {
        var stored = localStorage.getItem(window.ltsTheme.STORAGE_KEY);
        var theme = stored || (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
        window.ltsTheme.apply(theme);
    }
};

window.ltsTheme.init();
