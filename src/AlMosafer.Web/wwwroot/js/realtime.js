// نبض المسافر الحي (P40): اتصال SignalR دائم — إشعارات لحظية + شارة عدّاد الجرس.
// يعمل تحت مبدأ «تحسين لا شرط»: أي عطب هنا لا يكسر الموقع (تبقى القاعدة مصدر الحقيقة).
(function () {
    'use strict';

    if (!window.signalR) { return; }

    var badge = document.getElementById('navNotifBadge');

    function setBadge(count) {
        if (!badge) { return; }
        var n = parseInt(count, 10) || 0;
        if (n > 0) {
            badge.textContent = n > 99 ? '99+' : String(n);
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    }

    function toast(title, message) {
        var el = document.createElement('div');
        el.className = 'alm-toast';
        el.setAttribute('role', 'status');
        var head = document.createElement('div');
        head.className = 'alm-toast-title';
        head.textContent = '🔔 ' + (title || '');
        var body = document.createElement('div');
        body.textContent = message || '';
        el.appendChild(head);
        el.appendChild(body);
        document.body.appendChild(el);
        setTimeout(function () {
            el.style.opacity = '0';
            setTimeout(function () { el.remove(); }, 600);
        }, 6000);
    }

    function refreshCount() {
        fetch('/Notifications/Count', { headers: { 'Accept': 'application/json' } })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (d) { if (d && typeof d.unread !== 'undefined') { setBadge(d.unread); } })
            .catch(function () { /* صامت */ });
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/app')
        .withAutomaticReconnect() // انقطع النت؟ يعود وحده عند رجوعه — بلا تدخل
        .build();

    connection.on('ReceiveNotification', function (payload) {
        if (!payload) { return; }
        if (typeof payload.unreadCount !== 'undefined') { setBadge(payload.unreadCount); }
        toast(payload.title, payload.message);
    });

    // الموجة ٢: الدردشة اللحظية — تحديث صندوق الرسائل فوراً أو تنبيه عائم إن كان خارجها
    connection.on('ReceiveMessage', function (p) {
        if (!p) { return; }
        var box = document.getElementById('messages-box');
        var openHere = box && String(box.getAttribute('data-conversation-id')) === String(p.conversationId);
        var mine = window.__uid && String(p.senderId) === String(window.__uid);

        if (openHere) {
            try { document.dispatchEvent(new CustomEvent('almosafer:chat-refresh')); } catch (e) { }
        }
        if (!mine && !(openHere && !document.hidden)) {
            toast('رسالة جديدة' + (p.senderName ? ' من ' + p.senderName : ''), p.preview || '');
        }
    });

    function broadcastReady() {
        try { document.dispatchEvent(new CustomEvent('almosafer:realtime-join')); } catch (e) { }
    }

    connection.onreconnected(function () {
        window.__almConnection = connection;
        refreshCount(); // ما فاتك أثناء الانقطاع يعود ظاهراً في العدّاد
        broadcastReady(); // إعادة الانضمام لمجموعات المحادثات المفتوحة
    });

    connection.start()
        .then(function () {
            window.__almConnection = connection;
            refreshCount();
            broadcastReady();
        })
        .catch(function () { /* صامت: ربما جلسة غير مكتملة أو خادم قديم */ });
})();
