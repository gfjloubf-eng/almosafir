// Service Worker — منصة المسافر (PWA)
// سياسة محافظة مقصودة: نخبئ الأصول الثابتة فقط. الصفحات دائماً «شبكة أولاً» حتى
// لا يُقدَّم للمستخدم محتوى حساب شخصي متقادم أبداً.
const CACHE = 'musafer-static-v1';
const STATIC_ASSETS = [
  '/offline.html',
  '/manifest.webmanifest',
  '/icons/icon.svg',
  '/icons/icon-512.png'
];

self.addEventListener('install', (event) => {
  event.waitUntil(caches.open(CACHE).then((c) => c.addAll(STATIC_ASSETS)));
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((k) => k !== CACHE).map((k) => caches.delete(k))))
  );
  self.clients.claim();
});

self.addEventListener('fetch', (event) => {
  const req = event.request;
  if (req.method !== 'GET') return; // لا نلمس أي طلب مُغيِّر للحالة

  const url = new URL(req.url);
  if (url.origin !== self.location.origin) return;

  // الأصول الثابتة: الكاش أولاً
  if (url.pathname.startsWith('/lib/') || url.pathname.startsWith('/css/') ||
      url.pathname.startsWith('/js/') || url.pathname.startsWith('/icons/')) {
    event.respondWith(
      caches.match(req).then((cached) =>
        cached || fetch(req).then((res) => {
          const copy = res.clone();
          caches.open(CACHE).then((c) => c.put(req, copy));
          return res;
        }))
    );
    return;
  }

  // الصفحات: الشبكة أولاً، وعند انقطاعها صفحة «لا يوجد اتصال» المهذبة
  if (req.headers.get('accept')?.includes('text/html')) {
    event.respondWith(
      fetch(req).catch(() => caches.match('/offline.html'))
    );
  }
});
