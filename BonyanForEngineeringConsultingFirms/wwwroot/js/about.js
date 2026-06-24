// about.js — counter animation

(function () {
    function animateCounter(el, target) {
        var current = 0;
        var step = Math.ceil(target / 80);
        var timer = setInterval(function () {
            current += step;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            el.textContent = current.toLocaleString();
        }, 25);
    }

    function initCounters() {
        animateCounter(document.getElementById("statProjects"), 850);
        animateCounter(document.getElementById("statOffices"), 120);
        animateCounter(document.getElementById("statEngineers"), 3400);
        animateCounter(document.getElementById("statDocuments"), 12500);
    }

    // Run on load
    if (document.readyState === "complete") {
        initCounters();
    } else {
        document.addEventListener("DOMContentLoaded", initCounters);
    }
})();
