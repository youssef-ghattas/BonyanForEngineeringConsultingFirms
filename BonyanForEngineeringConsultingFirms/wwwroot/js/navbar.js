/* ══════════════════════════════════════════════════════════
   navbar.js
   Behavior for: live clock, global search, notification center,
   chatbot panel, quick actions, and language-button label sync.
   Dark mode + sidebar toggle stay in _Layout.cshtml as-is.
══════════════════════════════════════════════════════════ */

document.addEventListener("DOMContentLoaded", function () {

    /* ── 1. LIVE DATE / TIME ────────────────────────────────
       Updates every second. Locale follows the active <html lang>
       so it automatically reads in Arabic or English digits/format. */
    function updateClock() {
        const timeEl = document.getElementById("navbarTime");
        const dateEl = document.getElementById("navbarDate");
        if (!timeEl || !dateEl) return;

        const lang = document.documentElement.getAttribute("lang") || "ar";
        const locale = lang === "ar" ? "ar-EG" : "en-US";
        const now = new Date();

        timeEl.textContent = now.toLocaleTimeString(locale, {
            hour: "2-digit",
            minute: "2-digit"
        });
        dateEl.textContent = now.toLocaleDateString(locale, {
            weekday: "short",
            day: "numeric",
            month: "short"
        });
    }
    updateClock();
    setInterval(updateClock, 1000 * 30); // refresh every 30s is enough for a clock without a seconds display

    /* ── 2. GLOBAL SEARCH ────────────────────────────────────
       Placeholder client logic. Wire this to a real endpoint,
       e.g. GET /api/search?q=... returning JSON:
       [{ type: "project", id, name, url, icon }, ...]
       Suggested controller: SearchController.Global(string q) */
    const searchInput = document.getElementById("globalSearchInput");
    const searchResults = document.getElementById("globalSearchResults");
    let searchDebounce = null;

    if (searchInput) {
        searchInput.addEventListener("input", function () {
            clearTimeout(searchDebounce);
            const query = this.value.trim();

            if (query.length < 2) {
                searchResults.classList.remove("show");
                searchResults.innerHTML = "";
                return;
            }

            // Debounce to avoid firing a request on every keystroke
            searchDebounce = setTimeout(() => runGlobalSearch(query), 300);
        });

        searchInput.addEventListener("focus", function () {
            if (this.value.trim().length >= 2) searchResults.classList.add("show");
        });

        document.addEventListener("click", function (e) {
            if (!e.target.closest(".navbar-search")) {
                searchResults.classList.remove("show");
            }
        });
    }

    function runGlobalSearch(query) {
        // ── TODO: replace with real fetch() call ──
        // fetch(`/Search/Global?q=${encodeURIComponent(query)}`)
        //     .then(r => r.json())
        //     .then(renderSearchResults)
        //     .catch(() => renderSearchResults([]));

        // Demo placeholder so the UI is visibly functional out of the box:
        const demoMatches = [
            { type: "project", name: `Project matching "${query}"`, icon: "fa-project-diagram", url: "#" },
            { type: "task", name: `Task matching "${query}"`, icon: "fa-tasks", url: "#" }
        ];
        renderSearchResults(demoMatches);
    }

    function renderSearchResults(items) {
        if (!items || items.length === 0) {
            searchResults.innerHTML = `<div class="navbar-search-empty">لا توجد نتائج</div>`;
        } else {
            searchResults.innerHTML = items.map(item => `
                <a href="${item.url}" class="navbar-search-result-item">
                    <i class="fas ${item.icon}"></i>
                    <span>${item.name}</span>
                </a>
            `).join("");
        }
        searchResults.classList.add("show");
    }

    // Ctrl+K / Cmd+K focuses the search box (common power-user shortcut)
    document.addEventListener("keydown", function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
            e.preventDefault();
            if (searchInput) searchInput.focus();
        }
    });

    // Mobile search bar open/close
    const mobileSearchBtn = document.getElementById("mobileSearchBtn");
    const mobileSearchBar = document.getElementById("mobileSearchBar");
    const mobileSearchClose = document.getElementById("mobileSearchClose");

    if (mobileSearchBtn && mobileSearchBar) {
        mobileSearchBtn.addEventListener("click", function () {
            mobileSearchBar.classList.add("show");
            document.getElementById("mobileSearchInput")?.focus();
        });
        mobileSearchClose?.addEventListener("click", function () {
            mobileSearchBar.classList.remove("show");
        });
    }

    /* ── 3. NOTIFICATION CENTER ─────────────────────────────
       Placeholder data shaped exactly like your dashboard's
       OverdueTaskRow + a couple of system-level alert types,
       so swapping in a real endpoint is a drop-in replacement.

       Suggested real endpoint: GET /Home/GetNotifications
       returning JSON: [{ id, type, title, sub, time, isRead }]
       where `type` is one of: warning | danger | info | success */
    const notifBadge = document.getElementById("notifBadge");
    const notifList = document.getElementById("notifList");
    const notifClear = document.getElementById("notifClearBtn");

    const notifIcons = {
        warning: "fa-exclamation-triangle",
        danger: "fa-clock",
        info: "fa-info-circle",
        success: "fa-check-circle"
    };

    function fetchNotifications() {
        // ── TODO: replace with real fetch() call ──
        // fetch("/Home/GetNotifications")
        //     .then(r => r.json())
        //     .then(renderNotifications)
        //     .catch(() => renderNotifications([]));

        // Demo data — mirrors the real "overdue tasks" the dashboard already tracks
        const demoNotifications = [
            { id: 1, type: "danger", title: "مهمة متأخرة: تركيب الأساسات", sub: "مشروع برج النيل", time: "منذ 2 ساعة", isRead: false },
            { id: 2, type: "warning", title: "فاتورة غير مدفوعة", sub: "مشروع كومباوند الواحة", time: "منذ 5 ساعات", isRead: false },
            { id: 3, type: "info", title: "زيارة موقع جديدة مجدولة", sub: "غدًا 9:00 صباحًا", time: "منذ يوم", isRead: true }
        ];
        renderNotifications(demoNotifications);
    }

    function renderNotifications(items) {
        if (!notifList) return;

        const unreadCount = items.filter(n => !n.isRead).length;
        if (notifBadge) {
            notifBadge.style.display = unreadCount > 0 ? "flex" : "none";
            notifBadge.textContent = unreadCount > 9 ? "9+" : unreadCount;
        }

        if (items.length === 0) {
            notifList.innerHTML = `<div class="navbar-notif-empty">لا توجد إشعارات جديدة</div>`;
            return;
        }

        notifList.innerHTML = items.map(n => `
            <a href="#" class="navbar-notif-item ${n.isRead ? "" : "unread"}" data-id="${n.id}">
                <div class="navbar-notif-icon ${n.type}">
                    <i class="fas ${notifIcons[n.type] || "fa-bell"}"></i>
                </div>
                <div class="navbar-notif-text">
                    <div class="navbar-notif-title">${n.title}</div>
                    <div class="navbar-notif-sub">${n.sub}</div>
                    <div class="navbar-notif-time">${n.time}</div>
                </div>
            </a>
        `).join("");
    }

    if (notifClear) {
        notifClear.addEventListener("click", function (e) {
            e.preventDefault();
            document.querySelectorAll(".navbar-notif-item.unread").forEach(el => el.classList.remove("unread"));
            if (notifBadge) notifBadge.style.display = "none";
            // TODO: POST /Home/MarkNotificationsRead to persist server-side
        });
    }

    fetchNotifications();
    // Optional: poll every 2 minutes for new alerts without a full page reload
    setInterval(fetchNotifications, 120000);

    /* ── 4. CHATBOT PANEL ────────────────────────────────────
       Toggle + minimal send handler. Wire chatbotInput's reply
       to your real bot endpoint, e.g. POST /Chatbot/Ask { message } */
    const chatbotToggleBtn = document.getElementById("chatbotToggleBtn");
    const chatbotPanel = document.getElementById("chatbotPanel");
    const chatbotCloseBtn = document.getElementById("chatbotCloseBtn");
    const chatbotInput = document.getElementById("chatbotInput");
    const chatbotSendBtn = document.getElementById("chatbotSendBtn");
    const chatbotBody = document.getElementById("chatbotBody");

    function openChatbot() {
        chatbotPanel?.classList.add("open");
        chatbotInput?.focus();
    }
    function closeChatbot() {
        chatbotPanel?.classList.remove("open");
    }

    chatbotToggleBtn?.addEventListener("click", function () {
        chatbotPanel?.classList.contains("open") ? closeChatbot() : openChatbot();
    });
    chatbotCloseBtn?.addEventListener("click", closeChatbot);

    function appendChatMessage(text, sender) {
        if (!chatbotBody) return;
        const msg = document.createElement("div");
        msg.className = `chatbot-msg chatbot-msg-${sender}`;
        msg.textContent = text;
        chatbotBody.appendChild(msg);
        chatbotBody.scrollTop = chatbotBody.scrollHeight;
    }

    function sendChatMessage() {
        const text = chatbotInput.value.trim();
        if (!text) return;
        appendChatMessage(text, "user");
        chatbotInput.value = "";

        // ── TODO: replace with real fetch() call to your bot ──
        // fetch("/Chatbot/Ask", {
        //     method: "POST",
        //     headers: { "Content-Type": "application/json" },
        //     body: JSON.stringify({ message: text })
        // })
        //     .then(r => r.json())
        //     .then(data => appendChatMessage(data.reply, "bot"));

        setTimeout(() => {
            appendChatMessage("هذه استجابة تجريبية — اربط هذا الجزء بخدمة الشات بوت الفعلية.", "bot");
        }, 500);
    }

    chatbotSendBtn?.addEventListener("click", sendChatMessage);
    chatbotInput?.addEventListener("keypress", function (e) {
        if (e.key === "Enter") sendChatMessage();
    });

    /* ── 5. LANGUAGE BUTTON LABEL SYNC ───────────────────────
       Keeps the visible "English" / "العربية" label in sync with
       whatever language is currently active, without touching
       your existing toggleLanguage()/langswitch.js logic. */
    function syncLangButtonLabel() {
        const langBtnText = document.getElementById("langBtnText");
        if (!langBtnText) return;
        const current = document.documentElement.getAttribute("lang") || "ar";
        langBtnText.textContent = current === "ar" ? "English" : "العربية";
    }
    syncLangButtonLabel();

    // If your langswitch.js calls toggleLanguage() and reloads/re-renders,
    // this re-sync runs again on DOMContentLoaded automatically.
    // If it swaps language WITHOUT a reload, also call syncLangButtonLabel()
    // and updateClock() at the end of your toggleLanguage() function.
});