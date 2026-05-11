// ── Default Language is Arabic ────────────────────────
document.addEventListener("DOMContentLoaded", function () {
    const savedLang = localStorage.getItem("lang") || "ar";
    applyLanguage(savedLang);
});

function applyLanguage(lang) {
    // ── Set Direction ─────────────────────────────────
    document.documentElement.setAttribute("dir", lang === "ar" ? "rtl" : "ltr");
    document.documentElement.setAttribute("lang", lang);

    // ── Save Choice ───────────────────────────────────
    localStorage.setItem("lang", lang);

    // ── Load Translation File ─────────────────────────
    fetch(`/translations/${lang}.json`)
        .then(response => response.json())
        .then(data => {
            // apply translations to all elements with data-lang key
            document.querySelectorAll("[data-lang]").forEach(el => {
                const key = el.getAttribute("data-lang");
                if (data[key]) {
                    el.innerText = data[key];
                }
            });
        });

    // ── Update Button Text ────────────────────────────
    const btn = document.getElementById("langBtn");
    if (btn) {
        btn.innerText = lang === "ar" ? "English" : "عربي";
    }
}

function toggleLanguage() {
    const currentLang = localStorage.getItem("lang") || "ar";
    const newLang = currentLang === "ar" ? "en" : "ar";
    applyLanguage(newLang);
}