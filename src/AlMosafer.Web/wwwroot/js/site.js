/* ==========================================================================
   AlMosafer Platform — Client Interaction & Usability Scripts
   ========================================================================== */

document.addEventListener('DOMContentLoaded', function () {
    // 1. Password Visibility Toggle
    const togglePasswordButtons = document.querySelectorAll('.btn-toggle-password');
    togglePasswordButtons.forEach(button => {
        button.addEventListener('click', function () {
            const targetInputId = this.getAttribute('data-target');
            const targetInput = document.getElementById(targetInputId);
            if (targetInput) {
                if (targetInput.type === 'password') {
                    targetInput.type = 'text';
                    this.textContent = '🙈 إخفاء';
                    this.setAttribute('aria-label', 'إخفاء كلمة المرور');
                } else {
                    targetInput.type = 'password';
                    this.textContent = '👁️ إظهار';
                    this.setAttribute('aria-label', 'إظهار كلمة المرور');
                }
            }
        });
    });

    // 2. Dynamic Total Price Calculation for Trip Booking
    const seatsSelect = document.getElementById('seatsSelect');
    const totalAmountDisplay = document.getElementById('totalAmountDisplay');
    if (seatsSelect && totalAmountDisplay) {
        const pricePerSeat = parseFloat(seatsSelect.getAttribute('data-price-per-seat') || '0');
        const updateCalculation = function () {
            const count = parseInt(seatsSelect.value || '1', 10);
            const total = count * pricePerSeat;
            totalAmountDisplay.textContent = total.toLocaleString('ar-YE', { minimumFractionDigits: 0, maximumFractionDigits: 0 }) + ' ريال يمني';
        };
        seatsSelect.addEventListener('change', updateCalculation);
        updateCalculation(); // Initial call
    }

    // 3. Double-Submission Protection for Sensitive Forms
    const sensitiveForms = document.querySelectorAll('form[method="post"]:not(.no-double-submit-guard)');
    sensitiveForms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm ms-2" role="status" aria-hidden="true"></span> جاري المعالجة...';

                // Fallback timeout in case server responds with validation error without redirect
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }, 8000);
            }
        });
    });
});

// ⭐ حزمة «محبوب»: الوضع الليلي وحجم الخط — يُحفظان محلياً في متصفح المستخدم
(function () {
    var root = document.documentElement;
    function apply() {
        root.setAttribute('data-theme', localStorage.getItem('musafer-theme') || 'light');
        if (localStorage.getItem('musafer-font') === 'large') {
            root.classList.add('font-large');
        } else {
            root.classList.remove('font-large');
        }
    }
    apply();
    document.addEventListener('click', function (e) {
        if (e.target.closest('[data-toggle-theme]')) {
            localStorage.setItem('musafer-theme', root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark');
            apply();
        }
        if (e.target.closest('[data-toggle-font]')) {
            localStorage.setItem('musafer-font', root.classList.contains('font-large') ? '' : 'large');
            apply();
        }
    });
})();

/* P42 «التجديد البصري»: ظهور تدريجي أنيق للأقسام عند التمرير — يحترم تقليل الحركة */
(function () {
    const revealTargets = document.querySelectorAll('.rvl');
    if (revealTargets.length === 0) { return; }
    const reducedMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (!('IntersectionObserver' in window) || reducedMotion) {
        revealTargets.forEach(el => el.classList.add('rvl-in'));
        return;
    }
    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('rvl-in');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });
    revealTargets.forEach(el => observer.observe(el));
})();

/* P43 الموجة ١: شريط تقدم تنقل علوي — إحساس السرعة حتى قبل استجابة الخادم */
(function () {
    var bar = document.getElementById('navProgress');
    if (!bar) { return; }
    document.addEventListener('click', function (e) {
        var link = e.target.closest ? e.target.closest('a[href]') : null;
        if (!link) { return; }
        var href = link.getAttribute('href') || '';
        if (link.target === '_blank' || href.startsWith('#') || href.startsWith('http')) { return; }
        bar.classList.add('nav-progress-on');
        bar.style.width = '30%';
        setTimeout(function () { bar.style.width = '70%'; }, 250);
    });
    window.addEventListener('pageshow', function () {
        bar.classList.remove('nav-progress-on');
        bar.style.width = '0';
    });
})();

/* ═══════════ P43 «جولة التلميع» — الموجة ٢ ═══════════ */
// 1) أيقونات Lucide: استبدال كل <i data-lucide> برسمة SVG توارث لون السياق
if (window.lucide && typeof lucide.createIcons === 'function') { lucide.createIcons(); }

// 2) التاريخ بالعربية: كل input[type=date] يعرض «j F Y» عربية ويُرسل Y-m-d (النموذج لا يتغير)
if (window.flatpickr) {
    flatpickr('input[type="date"]', {
        locale: 'ar', dateFormat: 'Y-m-d', altInput: true, altFormat: 'j F Y',
        disableMobile: true, allowInput: true
    });
}

// 3) ترقيم فوري عام: [data-paginate="N"] على tbody (صفوف) أو حاوية (بطاقات) — عملياً بلا خادم
document.querySelectorAll('[data-paginate]').forEach(function (box) {
    var size = parseInt(box.getAttribute('data-paginate'), 10) || 10;
    var items = Array.prototype.slice.call(box.children);
    if (!items.length) return;
    if (items.length === 1 && items[0].querySelector && items[0].querySelector('td[colspan]')) return; // صف «لا توجد بيانات» يبقى كما هو
    if (items.length <= size) return;
    var pages = Math.ceil(items.length / size), cur = 1;
    var table = box.tagName === 'TBODY' ? box.closest('table') : null;
    var pager = document.createElement('nav');
    pager.className = 'alm-pager';
    pager.setAttribute('aria-label', 'ترقيم القائمة');
    function render() {
        items.forEach(function (el, i) { el.style.display = (i >= (cur - 1) * size && i < cur * size) ? '' : 'none'; });
        pager.innerHTML =
            '<button type="button" class="alm-pg-btn" data-mv="-1"' + (cur === 1 ? ' disabled' : '') + '>‹ السابق</button>' +
            '<span class="alm-pg-info">صفحة ' + cur + ' من ' + pages + '</span>' +
            '<button type="button" class="alm-pg-btn" data-mv="1"' + (cur === pages ? ' disabled' : '') + '>التالي ›</button>';
    }
    pager.addEventListener('click', function (e) {
        var b = e.target.closest('[data-mv]');
        if (!b || b.disabled) return;
        cur = Math.min(pages, Math.max(1, cur + parseInt(b.getAttribute('data-mv'), 10)));
        render();
        (table || box).scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
    ((table && table.parentElement) || box.parentElement).appendChild(pager);
    render();
});

/* ═══════════ P45 «جيب المسافر»: زر تثبيت التطبيق الذكي ═══════════ */
// لا يظهر إلا عندما يعلن المتصفح أن التثبيت متاح (beforeinstallprompt) ويختفي بعده
(function () {
    var deferred = null;
    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        deferred = e;
        var btn = document.getElementById('almInstallBtn');
        if (!btn) {
            btn = document.createElement('a');
            btn.id = 'almInstallBtn';
            btn.className = 'alm-install-btn';
            btn.href = '#';
            btn.innerHTML = '<i class="alm-ic" data-lucide="download"></i> ثبّت التطبيق';
            document.body.appendChild(btn);
            if (window.lucide) lucide.createIcons();
        }
        btn.hidden = false;
        btn.addEventListener('click', function (ev) {
            ev.preventDefault();
            if (!deferred) return;
            deferred.prompt();
            deferred.userChoice.finally(function () { btn.hidden = true; deferred = null; });
        });
    });
    window.addEventListener('appinstalled', function () {
        var btn = document.getElementById('almInstallBtn');
        if (btn) btn.hidden = true;
    });
})();

/* ═══════════ P49/UI «أرقام تصنع الثقة»: عدّ تصاعدي لإحصاءات الرئيسية ═══════════ */
// كل عنصر يحمل data-count = الرقم المستهدف؛ يعدّ من 0 إليه بسلاسة حين يدخل نطاق الرؤية
(function () {
    var targets = document.querySelectorAll('[data-count]');
    if (!targets.length) return;

    var reducedMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var fmt = function (n) {
        // فواصل آلاف بأرقام لاتينية واضحة (نفس أسلوب أسعار التذاكر في النظام)
        return n.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    };

    var run = function (el) {
        var target = parseInt(el.getAttribute('data-count'), 10) || 0;
        if (reducedMotion || target === 0) { el.textContent = fmt(target); return; }
        var dur = 1200, start = null;
        var step = function (ts) {
            if (start === null) start = ts;
            var p = Math.min((ts - start) / dur, 1);
            var eased = 1 - Math.pow(1 - p, 3); // easeOutCubic
            el.textContent = fmt(Math.round(target * eased));
            if (p < 1) requestAnimationFrame(step);
        };
        requestAnimationFrame(step);
    };

    if (!('IntersectionObserver' in window)) { targets.forEach(run); return; }
    var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (e) {
            if (e.isIntersecting) { run(e.target); io.unobserve(e.target); }
        });
    }, { threshold: 0.4 });
    targets.forEach(function (el) { io.observe(el); });
})();


// 1) حارس الإرسال المزدوج + زر «جارٍ التنفيذ…» لكل نموذج POST — يمنع الحجز المكرر بنقرتين متتاليتين
// 2) data-confirm: تأكيد موحد للإجراءات الهدامة (الحذف/الإلغاء) بنص عربي صريح
// 3) نجمة حمراء تلقائية على تسميات الحقول المطلوبة (required) — اتفاقية النماذج العالمية
document.querySelectorAll('form[method="post"], form[method="POST"]').forEach(function (form) {
    form.addEventListener('submit', function (e) {
        // تأكيد هدام إن طُلب
        if (form.dataset.confirm && !window.confirm(form.dataset.confirm)) { e.preventDefault(); return; }
        // لا تعطيل إن فشل تحقق jQuery-validate (النموذج لن يرسل)
        if (window.jQuery && typeof jQuery(form).valid === 'function' && !jQuery(form).valid()) return;
        if (form.dataset.busy) { e.preventDefault(); return; } // نقرة ثانية = تجاهل
        form.dataset.busy = '1';
        var btn = form.querySelector('button[type="submit"], input[type="submit"]');
        if (btn && !btn.dataset.noGuard && btn.tagName === 'BUTTON') {
            btn.disabled = true;
            btn.dataset.originalHtml = btn.innerHTML;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm ms-1" role="status" aria-hidden="true"></span> جارٍ التنفيذ…';
        }
    });
});
// العودة من ذاكرة المتصفح (bfcache): إحياء النماذج المعلّقة حتى لا تعلق الأزرار معطلة
window.addEventListener('pageshow', function (e) {
    if (!e.persisted) return;
    document.querySelectorAll('form[data-busy]').forEach(function (form) {
        delete form.dataset.busy;
        var btn = form.querySelector('button[type="submit"]');
        if (btn && btn.dataset.originalHtml) { btn.disabled = false; btn.innerHTML = btn.dataset.originalHtml; }
    });
});
// نجمة الحقول المطلوبة
document.querySelectorAll('input[required], select[required], textarea[required]').forEach(function (el) {
    var label = el.id ? document.querySelector('label[for="' + el.id + '"]')
                      : el.closest('.mb-3, .col-md-3, .col-md-4, .col-md-6, .col-12')?.querySelector('label');
    if (label && !label.querySelector('.alm-req')) {
        var star = document.createElement('span');
        star.className = 'alm-req';
        star.textContent = ' *';
        label.appendChild(star);
    }
});
