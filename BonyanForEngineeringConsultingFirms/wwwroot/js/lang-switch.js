// lang-switch.js

function applyLanguage(lang) {
    const data = TRANSLATIONS[lang];
    if (!data) return;

    // 1. Set dir + lang on <html>
    document.documentElement.setAttribute("dir", lang === "ar" ? "rtl" : "ltr");
    document.documentElement.setAttribute("lang", lang);
    localStorage.setItem("lang", lang);

    // 2. Apply font family on body
    document.body.style.fontFamily = lang === "ar"
        ? "'Cairo', sans-serif"
        : "'Poppins', sans-serif";

    // 3. Translate all [data-lang] elements
    document.querySelectorAll("[data-lang]").forEach(function (el) {
        var key = el.getAttribute("data-lang");
        if (!data[key]) return;

        // Don't overwrite elements that only contain child nodes (icons etc.)
        // Only set innerText if the element has no child elements, OR is a <span>/<p>/<td> etc.
        if (el.children.length === 0) {
            el.innerText = data[key];
        } else {
            // Find a text node child and update it, leave icons intact
            el.childNodes.forEach(function (node) {
                if (node.nodeType === Node.TEXT_NODE && node.textContent.trim() !== "") {
                    node.textContent = data[key];
                }
            });
        }

        // Also handle placeholder attribute
        if (el.tagName === "INPUT" || el.tagName === "TEXTAREA") {
            el.setAttribute("placeholder", data[key]);
        }
    });

    // 4. Update lang button text (only the <span> inside it, not the icon)
    var langBtnText = document.getElementById("langBtnText");
    if (langBtnText) {
        langBtnText.textContent = lang === "ar" ? "English" : "العربية";
    }

    // 5. Update placeholder-only elements
    document.querySelectorAll("[data-lang-placeholder]").forEach(function (el) {
        var key = el.getAttribute("data-lang-placeholder");
        if (data[key]) el.setAttribute("placeholder", data[key]);
    });

    // 5a. Update aria-label-only elements (icon-only buttons, e.g. lightbox close)
    document.querySelectorAll("[data-lang-aria]").forEach(function (el) {
        var key = el.getAttribute("data-lang-aria");
        if (data[key]) {
            el.setAttribute("aria-label", data[key]);
            el.setAttribute("title", data[key]);
        }
    });

    // 5b. Translate dynamic per-row values (status badges, roles, etc.)
    //     These carry their own pre-computed Arabic/English text via
    //     data-ar / data-en attributes (set server-side) instead of a
    //     shared dictionary key, since the text differs per row/record.
    document.querySelectorAll("[data-en][data-ar]").forEach(function (el) {
        el.textContent = lang === "ar" ? el.getAttribute("data-ar") : el.getAttribute("data-en");
    });

    // 6. Fire custom event for other scripts
    document.dispatchEvent(new CustomEvent("languageChanged", { detail: { lang: lang } }));
}

function toggleLanguage() {
    var current = localStorage.getItem("lang") || "ar";
    applyLanguage(current === "ar" ? "en" : "ar");
}

// Apply saved language on page load
document.addEventListener("DOMContentLoaded", function () {
    var saved = localStorage.getItem("lang") || "ar";
    applyLanguage(saved);
});