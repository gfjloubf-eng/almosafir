# الوثيقة الأكاديمية الماستر الشاملة للمشروع — AlMosafer نظام المسافر
## Comprehensive Academic Master System Manual (32 Master Sections)

---

### 1. Project Title
**AlMosafer — نظام المسافر للتنقل والحجز البري بين المدن اليمنية**

### 2. Introduction
منظومة برمجية متكاملة تعتمد تقنيات الويب الحديثة والمعمارية النظيفة Clean Architecture لبناء بيئة رقمية آمنة وموثوقة تجمع بين المسافرين والسائقين ومديري النظام في اليمن.

### 3. Problem Statement
عشوائية الحجز البري، غياب الموثوقية في توفر المقاعد، انعدام سجلات المدفوعات، وغياب الرؤية الإحصائية الموحدة.

### 4. Proposed Solution
منصة ويب رصينة تعتمد ASP.NET Core 10 و Clean Architecture و EF Core مع قواعد بيانات MySQL/MariaDB يوفر الحجز المباشر والإيصالات الرقمية والتقارير المتقدمة.

### 5. Objectives
1. أتمتة عمليات الحجز بين المدن.
2. منع ظاهرة الحجز الزائد Overbooking.
3. حفظ السجلات المالية والتقييمات بشفافية.
4. توفير لوحات تحكم متقدمة لمختلف الأدوار.

### 6. Scope
تغطية الرحلات البرية بين المحافظات والمدن اليمنية (صنعاء، عدن، تعز، الحديدة، إب، حضرموت، إلخ).

### 7. Target Users
1. الزائر (Visitor).
2. المسافر (Traveler).
3. السائق (Driver).
4. مدير النظام (Admin).

### 8. Functional Requirements
تسجيل ومصادقة، بحث عن رحلات، حجز فوري، سداد داخلي، إصدار إيصال، محادثات، إشعارات، تقييمات، تقارير وإحصائيات.

### 9. Non-Functional Requirements
الأمان (PBKDF2, IDOR, CSRF)، الأداء (EF Core AsNoTracking)، التوافقية (RTL, Responsive Design), وتوافقية الوصول (WCAG 2.2 AA).

### 10. Use Cases
1. UC-01: التسجيل وتسجيل الدخول.
2. UC-02: البحث عن الرحلات وحجز المقاعد.
3. UC-03: إدارة الرحلات وإنشائها بواسطة السائق.
4. UC-04: إدارة المنظومة والمستخدمين والتقارير من قِبل المشرف.

### 11. System Architecture
معمارية الطبقات الأربع الفصل التام بين المسؤوليات Separation of Concerns.

### 12. Clean Architecture
Domain, Application, Infrastructure, Web.

### 13. Technologies
C# 13, .NET 10, ASP.NET Core MVC, EF Core 9.0, Pomelo Provider, MySQL/MariaDB XAMPP, Bootstrap 5 RTL, Chart.js, xUnit.

### 14. Database Architecture
قاعدة بيانات `mosafir_db` على المنفذ `3306`.

### 15. ERD Explanation
8 جداول قياسية مترابطة بمفاتيح أجنبية وقيود سلامة مرجعية (`users`, `trips`, `bookings`, `payments`, `ratings`, `notifications`, `conversations`, `messages`).

### 16. Authentication
مصادقة الكوكيز الآمنة وقيم الانتهاء المنزلقة.

### 17. Authorization
التخويل الرأسي القائم على الأدوار `[Authorize(Roles = "...")]`.

### 18. Booking Logic
فحص المقاعد بالسيرفر والمعاملات الذرية لمنع الحجز الزائد.

### 19. Payment Logic
سجلات الدفع المحاكاة داخلياً وحفظ الحسابات بدقة `decimal(18,2)`.

### 20. Rating Logic
ارتباط التقييم بحجز حقيقي ورحلة مكتملة مع تحديث متوسط تقييم السائق آلياً.

### 21. Messaging
محادثات فورية بين السائق والركاب ضمن الرحلة الحالية.

### 22. Notifications
سجل تنبيهات تفاعلي للمستخدمين.

### 23. Reports
تقارير إحصائية وبيانية تفاعلية باستخدام Chart.js.

### 24. Admin Dashboard
لوحة تحكم مركزية شاملة لمراقبة المنظومة والمستخدمين والمالية.

### 25. HCI
تطبيق مبادئ Nielsen Norman الـ 10، وتوافقية الخطوط والتجاوب.

### 26. Security
تشفير PBKDF2/SHA-256، وقاية IDOR، CSRF، XSS، و Rate Limiting.

### 27. Testing
**59 اختبارًا آليًا ناجحًا (100% Pass Rate)**.

### 28. Performance
تحسين استعلامات EF Core ومنع مشكلة N+1 Queries.

### 29. Deployment
جاهزية الحاويات Docker وملف CI/CD عبر GitHub Actions والروابط السحابية المستهدفة.

### 30. Limitations
غياب ربط الدفع المباشر ببنوك محلية وخدمات الخرائط التفاعلية المباشرة GPS.

### 31. Future Work
إضافة بوابات دفع يمنية، تتبع GPS حي للرحلة، وتطبيق Flutter للهواتف.

### 32. Conclusion
مشروع AlMosafer يمثل نموذجاً أكاديمياً وهندسياً متكافئاً وجاهزاً للمناقشة والتخرج بعلامة متميزة.
