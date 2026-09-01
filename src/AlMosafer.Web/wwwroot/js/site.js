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
