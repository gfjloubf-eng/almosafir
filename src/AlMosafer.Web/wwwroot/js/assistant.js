/* ==========================================================================
   P54 «مساعد المسافر الذكي» — نافذة محادثة + إدخال صوتي + نطق الردود
   يعمل تحت مبدأ «تحسين لا شرط»: أي عطب هنا لا يكسر المنصة إطلاقاً.
   ========================================================================== */
(function () {
    'use strict';

    var fab = document.getElementById('almAssistFab');
    var panel = document.getElementById('almAssistPanel');
    if (!fab || !panel) return;

    var messages = document.getElementById('almAssistMessages');
    var chips = document.getElementById('almAssistChips');
    var input = document.getElementById('almAssistInput');
    var sendBtn = document.getElementById('almAssistSend');
    var closeBtn = document.getElementById('almAssistClose');
    var micBtn = document.getElementById('almAssistMic');

    var opened = false;

    function scrollDown() { messages.scrollTop = messages.scrollHeight; }

    function addBubble(text, who) {
        var div = document.createElement('div');
        div.className = 'alm-assist-msg ' + (who === 'user' ? 'user' : 'bot');
        var span = document.createElement('span');
        span.textContent = text; // نص آمن دائماً (لا innerHTML لبيانات ديناميكية)
        div.appendChild(span);

        // زر نطق الرد صوتياً (إن دعم المتصفح)
        if (who === 'bot' && 'speechSynthesis' in window) {
            var sp = document.createElement('button');
            sp.type = 'button';
            sp.className = 'alm-assist-speak';
            sp.setAttribute('data-text', text);
            sp.setAttribute('aria-label', 'استماع للرد');
            sp.innerHTML = '<i class="alm-ic" data-lucide="volume-2" aria-hidden="true"></i>';
            div.appendChild(sp);
        }

        messages.appendChild(div);
        if (window.lucide && typeof lucide.createIcons === 'function') lucide.createIcons();
        scrollDown();
        return div;
    }

    function showChips(list) {
        chips.innerHTML = '';
        (list || []).forEach(function (s) {
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'alm-assist-chip';
            b.textContent = s;
            b.addEventListener('click', function () { send(s); });
            chips.appendChild(b);
        });
    }

    function showTyping() {
        var div = document.createElement('div');
        div.className = 'alm-assist-msg bot';
        div.innerHTML = '<span class="alm-assist-dots"><i></i><i></i><i></i></span>';
        messages.appendChild(div);
        scrollDown();
        return div;
    }

    function openPanel() {
        panel.hidden = false;
        fab.setAttribute('aria-expanded', 'true');
        if (!opened) {
            opened = true;
            addBubble('أهلاً بك 👋 أنا مساعد المسافر الذكي.\nأسألني عن الحجز أو الدفع أو الرحلات، وسأجيبك فوراً — وأقرأ إجابتي بصوتٍ مسموع 🔊', 'bot');
            showChips(['كيف أحجز رحلة؟', 'كم رحلة متاحة الآن؟', 'طرق الدفع', 'كيف أتواصل مع السائق؟']);
        }
        input.focus();
    }

    function closePanel() {
        panel.hidden = true;
        fab.setAttribute('aria-expanded', 'false');
    }

    fab.addEventListener('click', function () {
        if (panel.hidden) openPanel(); else closePanel();
    });
    closeBtn.addEventListener('click', closePanel);

    function send(text) {
        var t = (text || '').trim();
        if (!t) return;
        if (input.value === t) input.value = '';

        addBubble(t, 'user');
        var typing = showTyping();

        fetch('/Assistant/Ask?q=' + encodeURIComponent(t), { headers: { 'Accept': 'application/json' } })
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (d) {
                typing.remove();
                if (d && d.answer) {
                    addBubble(d.answer, 'bot');
                    showChips(d.suggestions);
                } else {
                    addBubble('تعذّر الرد الآن — تأكد أن السيرفر يعمل ثم حاول مجدداً.', 'bot');
                    showChips(['كيف أحجز رحلة؟', 'طرق الدفع']);
                }
            })
            .catch(function () {
                typing.remove();
                addBubble('تعذّر الاتصال بالسيرفر — جرّب بعد لحظات.', 'bot');
            });
    }

    sendBtn.addEventListener('click', function () { send(input.value); });
    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); send(input.value); }
    });

    // ─── نطق الردود صوتياً (TTS) — يعمل حتى بدون إنترنت ───
    if ('speechSynthesis' in window) {
        messages.addEventListener('click', function (e) {
            var sp = e.target.closest ? e.target.closest('.alm-assist-speak') : null;
            if (!sp) return;
            var text = sp.getAttribute('data-text') || '';
            if (!text) return;
            var u = new SpeechSynthesisUtterance(text);
            u.lang = 'ar';
            u.rate = 0.95;
            window.speechSynthesis.cancel();
            window.speechSynthesis.speak(u);
        });
    }

    // ─── الإدخال الصوتي (STT) — يتطلب سياقاً آمناً (localhost أو HTTPS) ───
    var SR = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (SR) {
        micBtn.hidden = false;
        var rec = new SR();
        rec.lang = 'ar-SA';
        rec.interimResults = false;
        rec.maxAlternatives = 1;
        var listening = false;

        micBtn.addEventListener('click', function () {
            if (listening) { rec.stop(); return; }
            try {
                rec.start();
                listening = true;
                micBtn.classList.add('alm-assist-mic-on');
            } catch (err) { /* تجاهل */ }
        });

        rec.onresult = function (e) {
            listening = false;
            micBtn.classList.remove('alm-assist-mic-on');
            var t = e.results && e.results[0] && e.results[0][0] ? e.results[0][0].transcript : '';
            if (t) { input.value = t; send(t); }
        };
        rec.onend = function () { listening = false; micBtn.classList.remove('alm-assist-mic-on'); };
        rec.onerror = function () { listening = false; micBtn.classList.remove('alm-assist-mic-on'); };
    }
})();
