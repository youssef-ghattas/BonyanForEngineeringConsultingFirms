/* ══════════════════════════════════════════════════════════
   auth-theme.js
   Light/dark toggle for the standalone auth pages (Login,
   Register, ForgotPassword, ChangePassword). Shares the same
   "bonyan-dark" localStorage key as the main app layout, so
   switching theme on one is reflected everywhere.
══════════════════════════════════════════════════════════ */
(function () {
    function injectToggleButton() {
        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'auth-theme-toggle';
        btn.id = 'authThemeToggle';
        btn.title = 'Dark / Light';
        btn.setAttribute('aria-label', 'Toggle theme');
        btn.innerHTML = '<i class="fas fa-moon" id="authThemeIcon"></i>';
        document.body.appendChild(btn);
        return btn;
    }

    function syncIcon() {
        var icon = document.getElementById('authThemeIcon');
        if (!icon) return;
        icon.className = document.body.classList.contains('dark-mode') ? 'fas fa-sun' : 'fas fa-moon';
    }

    document.addEventListener('DOMContentLoaded', function () {
        var btn = injectToggleButton();
        syncIcon();

        btn.addEventListener('click', function () {
            var on = !document.body.classList.contains('dark-mode');
            document.body.classList.toggle('dark-mode', on);
            localStorage.setItem('bonyan-dark', on ? '1' : '0');
            syncIcon();
        });
    });
})();
