/* ══════════════════════════════════════════════════════════
   navbar.js
   Behavior for: live clock, notification center (real system
   data), chatbot panel, and language-button label sync.
   Dark mode + sidebar toggle stay in _Layout.cshtml as-is.
══════════════════════════════════════════════════════════ */

document.addEventListener("DOMContentLoaded", function () {

    /* ── 1. LIVE DATE / TIME ─────────────────────────────── */
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
    setInterval(updateClock, 1000 * 30);

    /* ── 2. NOTIFICATION CENTER ──────────────────────────────
       Live data from GET /Home/GetNotifications, built from
       real overdue tasks, overdue invoices, and site-visit
       safety alerts (see HomeController.GetNotifications). */
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
        fetch("/Home/GetNotifications", {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
            .then(r => {
                if (!r.ok) throw new Error("Failed to load notifications");
                return r.json();
            })
            .then(renderNotifications)
            .catch(() => renderNotifications([]));
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
            <a href="${n.url || "#"}" class="navbar-notif-item ${n.isRead ? "" : "unread"}" data-id="${n.id}">
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
        });
    }

    fetchNotifications();
    setInterval(fetchNotifications, 120000); // poll every 2 minutes

    /* ── 3. CHATBOT PANEL ────────────────────────────────── */
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
    if (sender === "bot") {
        // Convert bold and newlines to HTML for prettier display
        const formatted = text.replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>").replace(/\n/g, "<br>");
        msg.innerHTML = formatted;
    } else {
        msg.textContent = text;
    }

    chatbotBody.appendChild(msg);
    chatbotBody.scrollTop = chatbotBody.scrollHeight;
}

    
    function sendChatMessage() {
        const text = chatbotInput.value.trim();
        if (!text) return;
        appendChatMessage(text, "user"); applyLanguage
        chatbotInput.value = "";

        fetch("/Ai/SendMessage", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ message: text })
        })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                appendChatMessage(data.message, "bot");
            } else {
                appendChatMessage("Error: " + data.message, "bot");
            }
        })
        .catch(() => {
            appendChatMessage("Something went wrong. Please try again.", "bot");
        });

       

       
    }

    

    chatbotSendBtn?.addEventListener("click", sendChatMessage);
    chatbotInput?.addEventListener("keypress", function (e) {
        if (e.key === "Enter") sendChatMessage();
    });
    /* ── 4. LANG BUTTON SYNC — handled by lang-switch.js ─── */

});