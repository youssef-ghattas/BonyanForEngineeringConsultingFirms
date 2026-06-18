// langswitch.js

function applyLanguage(lang) {
    // 1. Fallback security check
    const data = TRANSLATIONS[lang];
    if (!data) return;

    // 2. Set structural HTML document settings
    document.documentElement.setAttribute("dir", lang === "ar" ? "rtl" : "ltr");
    document.documentElement.setAttribute("lang", lang);
    localStorage.setItem("lang", lang);

    // 3. Scan and translate standard text elements AND placeholders
    document.querySelectorAll("[data-lang]").forEach(el => {
        const key = el.getAttribute("data-lang");
        if (data[key]) {
            // Update standard element inner text
            el.innerText = data[key];

            // Support text placeholders on inputs/text areas
            if (el.tagName === "INPUT" || el.tagName === "TEXTAREA") {
                el.setAttribute("placeholder", data[key]);
            }
        }
    });

    // 4. Update the trigger button text safely
    const btn = document.getElementById("langBtn");
    if (btn) btn.innerText = lang === "ar" ? "English" : "عربي";

    // 5. Fire custom app events if needed
    document.dispatchEvent(new CustomEvent("languageChanged", { detail: { lang } }));
}

function toggleLanguage() {
    const current = localStorage.getItem("lang") || "ar";
    applyLanguage(current === "ar" ? "en" : "ar");
}

// Run immediately as soon as DOM environment parses
document.addEventListener("DOMContentLoaded", function () {
    const saved = localStorage.getItem("lang") || "ar";
    applyLanguage(saved);
});