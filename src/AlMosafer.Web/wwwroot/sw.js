// Service Worker — منصة المسافر (PWA / P45 «جيب المسافر»)
// السياسة:
//  · الأصول الثابتة (css/js/lib/icons/img): كاش أولاً (موسومة بإصدارات asp-append-version)
//  · الصفحات العامة (الرئيسية/الخطوط/المواصلات الداخلية): «قديم أثناء التحديث» SWR —
//    مسافر بلا شبكة في المحطة يقرأ آخر جداول معروفة. دون أي بيانات شخصية.
//  · الصفحات الشخصية (حجوزاتي/الملف/المحادثات…): شبكة أولاً دائماً؛ عند الانقطاع صفحة اعتذار مهذبة.
const STATIC_CACHE = 'musafer-static-v3';
const PAGES_CACHE = 'musafer-pages-v1';
const PUBLIC_PAGES = ['/', '/Lines', '/Trips/InternalLines'];
const STATIC_ASSETS = [
  '/offline.html',
  '/manifest.webmanifest',
  '/favicon.ico',
  '/icons/icon.svg',
  '/icons/icon-192.png',
  '/lib/lucide/lucide.min.js',
  '/lib/flatpickr/flatpickr.min.js',
  '/lib/flatpickr/flatpickr.min.css',
  '/lib/flatpickr/flatpickr.ar.js',
  '/fonts/cairo-arabic-400-normal.woff2',
  '/fonts/cairo-arabic-600-normal.woff2',
  '/fonts/cairo-arabic-700-normal.woff2',
  '/fonts/cairo-arabic-800-normal.woff2',
  '/fonts/cairo-latin-400-normal.woff2',
  '/fonts/cairo-latin-600-normal.woff2',
  '/fonts/cairo-latin-700-normal.woff2',
  '/fonts/cairo-latin-800-normal.woff2'
];

self.addEventListener('install', (event) => {
  event.waitUntil(caches.open(STATIC_CACHE).then((c) => c.addAll(STATIC_ASSETS)));
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  const keep = [STATIC_CACHE, PAGES_CACHE];
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((k) => !keep.includes(k)).map((k) => caches.delete(k))))
  );
  self.clients.claim();
});

// حد أقصى لحجم كاش الصفحات حتى لا تنفد مساحة الجهاز
async function trimCache(cacheName, maxItems) {
  const cache = await caches.open(cacheName);
  const keys = await cache.keys();
  while (keys.length > maxItems) {
    await cache.delete(keys.shift());
  }
}

self.addEventListener('fetch', (event) => {
  const req = event.request;
  if (req.method !== 'GET') return; // لا نلمس أي طلب مُغيِّر للحالة

  const url = new URL(req.url);
  if (url.origin !== self.location.origin) return;

  // 1) الأصول الثابتة: الكاش أولاً
  if (url.pathname.startsWith('/lib/') || url.pathname.startsWith('/css/') ||
      url.pathname.startsWith('/js/') || url.pathname.startsWith('/icons/') ||
      url.pathname.startsWith('/img/') || url.pathname.startsWith('/fonts/')) {
    event.respondWith(
      caches.match(req).then((cached) =>
        cached || fetch(req).then((res) => {
          const copy = res.clone();
          caches.open(STATIC_CACHE).then((c) => c.put(req, copy));
          return res;
        }))
    );
    return;
  }

  const isHtml = req.headers.get('accept')?.includes('text/html');
  if (!isHtml) return;

  const path = url.pathname.replace(/\/+$/, '') || '/';
  const isPublic = PUBLIC_PAGES.indexOf(path) !== -1;

  // 2) الصفحات العامة: قديم أثناء التحديث — آخر نسخة معروفة فوراً + تحديث خلفي
  if (isHtml && isPublic) {
    event.respondWith(
      caches.open(PAGES_CACHE).then(async (cache) => {
        const cached = await cache.match(req);
        const network = fetch(req).then((res) => {
          if (res.ok) {
            cache.put(req, res.clone());
            trimCache(PAGES_CACHE, 30);
          }
          return res;
        }).catch(() => null);
        return cached || (await network) || caches.match('/offline.html');
      })
    );
    return;
  }

  // 3) الصفحات الشخصية وكل ما تبقى: الشبكة أولاً، وعند انقطاعها صفحة «لا يوجد اتصال»
  event.respondWith(fetch(req).catch(() => caches.match('/offline.html')));
});
