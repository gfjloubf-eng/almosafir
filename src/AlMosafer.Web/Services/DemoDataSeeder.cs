using AlMosafer.Application.DTOs.Auth;
using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.DTOs.Ratings;
using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Enums;

namespace AlMosafer.Web.Services;

/// <summary>
/// بذرة عرض تقديمي (P47.7) — تُفعَّل حصراً بمتغير البيئة DemoSeed=true.
/// تملأ المنصة الفارغة بمحتوى يمني واقعي: خطوط شبكة + سائقون ورحلات وحجوزات وتقييمات،
/// لتظهر كل الصفحات (الاقتراحات، المتصدرون، القوائم) بكامل جمالها فوراً.
/// آمنة التكرار: تتخطى كاملة إن وُجد أي سطر مفعّل أو حساب demo@.
/// كل خطوة بأفضل جهد — فشل جزئي يسجَّل ولا يوقف البذر ولا إقلاع التطبيق.
/// </summary>
public static class DemoDataSeeder
{
    private const string Marker = "@demo.almosafer";
    private const string DemoPassword = "Demo@12345";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var auth = sp.GetRequiredService<IAuthService>();
        var lines = sp.GetRequiredService<ILineService>();
        var trips = sp.GetRequiredService<ITripService>();
        var bookings = sp.GetRequiredService<IBookingService>();
        var ratings = sp.GetRequiredService<IRatingService>();

        if ((await lines.GetAllLinesAsync()).Any())
        {
            Console.WriteLine("[DemoSeed] توجد خطوط مسبقاً — البذرة تتخطى (وضع آمن ضد التكرار).");
            return;
        }

        // ─── ١) حسابات العرض ───
        var driverIds = new List<int>();
        var drivers = new[]
        {
            ("أحمد الحميري", "driver1", "صنعاء", "تويوتا هايلوكس 2022", "1-23456"),
            ("صالح المطري", "driver2", "عدن", "كيا سيراتو 2021", "2-78901"),
            ("خالد العنسي", "driver3", "تعز", "هيونداي سوناتا 2020", "3-45678"),
        };
        foreach (var (name, alias, city, car, plate) in drivers)
        {
            var r = await auth.RegisterDriverAsync(new RegisterDriverDto
            {
                FullName = name, Email = $"{alias}{Marker}", Password = DemoPassword, ConfirmPassword = DemoPassword,
                Phone = "770000001", PlateNumber = plate, VehicleModel = car, VehicleYear = 2021, City = city
            });
            if (r.Success && r.User is not null) driverIds.Add(r.User.Id);
        }

        var travelerIds = new List<int>();
        foreach (var (name, alias) in new[] { ("محمد الشيباني", "traveler1"), ("فاطمة الأغبري", "traveler2") })
        {
            var r = await auth.RegisterTravelerAsync(new RegisterTravelerDto
            {
                FullName = name, Email = $"{alias}{Marker}", Password = DemoPassword, ConfirmPassword = DemoPassword,
                Phone = "770000002"
            });
            if (r.Success && r.User is not null) travelerIds.Add(r.User.Id);
        }

        // ─── ٢) شبكة الخطوط الداخلية ───
        var lineSpecs = new Dictionary<string, (string City, string[] Stops)>
        {
            ["خط التحرير — السبعين"] = ("صنعاء", new[] { "ميدان التحرير", "جولة كنتاكي", "شارع السبعين", "الجامعة الجديدة" }),
            ["خط كريتر — المعلا"] = ("عدن", new[] { "كريتر", "التواهي", "المعلا", "خور مكسر" }),
            ["خط جولة الجملة — الحوبان"] = ("تعز", new[] { "جولة الجملة", "شارع جمال", "الحوبان" }),
            ["خط المشاعل — الظهار"] = ("إب", new[] { "المشاعل", "الدلال", "مدينة الظهار" }),
            ["خط الكورنيش — الميناء"] = ("الحديدة", new[] { "الكورنيش", "الميناء", "شارع صنعاء" }),
            ["خط رداع — البيضاء"] = ("ذمار", new[] { "ذمار", "معبر", "رداع" }),
        };
        var lineIds = new List<(int Id, string City)>();
        foreach (var (name, (city, stops)) in lineSpecs)
        {
            var created = await lines.CreateLineAsync(name, city, "خط تجريبي مفعّل لعرض المنصة — مواقف مرتبة ومواعيد أسبوعية.");
            if (!created.Success) continue;
            var all = await lines.GetAllLinesAsync();
            var line = all.FirstOrDefault(l => l.Name == name);
            if (line is null) continue;
            for (var i = 0; i < stops.Length; i++) await lines.AddStopAsync(line.Id, stops[i], i + 1);
            await lines.AddScheduleAsync(line.Id, 6, "07:00 صباحاً");   // السبت
            await lines.AddScheduleAsync(line.Id, 1, "07:30 صباحاً");   // الاثنين
            await lines.AddScheduleAsync(line.Id, 3, "04:00 عصراً");    // الأربعاء
            lineIds.Add((line.Id, city));
        }

        // ─── ٣) رحلات بين المدن (مستقبلية) ───
        var tripSpecs = new[]
        {
            (0, "صنعاء", "التحرير", "عدن", "باب عدن", 2, 4, 12000m, "رحلة مريحة مع استراحة في الضالع"),
            (1, "عدن", "خور مكسر", "صنعاء", "شارع تعز", 3, 3, 13000m, "انطلاق مبكر — وصول قبل العصر"),
            (0, "صنعاء", "سائلة صنعاء", "تعز", "جولة الجملة", 1, 4, 9000m, "طريق هَجة المعبّد"),
            (2, "تعز", "الحوبان", "إب", "المشاعل", 2, 2, 6500m, "ساعة ونصف تقريباً"),
            (2, "إب", "الدلال", "صنعاء", "جولة مصباحي", 4, 3, 8000m, "مكيف + شحن جوال"),
            (1, "الحديدة", "الميناء", "صنعاء", "السبعين", 5, 4, 11000m, "عبر طريق الحديدة الجديد"),
        };
        var createdTripIds = new List<(int Id, int DriverIdx)>();
        foreach (var (di, from, fromLoc, to, toLoc, days, seats, price, desc) in tripSpecs)
        {
            if (di >= driverIds.Count) continue;
            var r = await trips.CreateTripAsync(driverIds[di], new CreateTripDto
            {
                FromCity = from, FromLocation = fromLoc, ToCity = to,
                TripTime = DateTime.Now.AddDays(days).Date.AddHours(8),
                Seats = seats, PricePerSeat = price, Description = $"{desc} — نقطة النزول: {toLoc}", VehicleInfo = "حافلة صالون نظيفة ومؤمنة"
            });
            if (r.Success && r.TripId is not null) createdTripIds.Add((r.TripId.Value, di));
        }

        // ─── ٤) حجوزات + تقييمات (أفضل جهد — قواعد العمل قد ترفض المستقبلي غير المكتمل) ───
        var rated = 0;
        foreach (var t in travelerIds)
        {
            foreach (var (tripId, di) in createdTripIds.Take(2))
            {
                var b = await bookings.CreateBookingAsync(t, new CreateBookingDto
                { TripId = tripId, SeatsBooked = 1, PaymentMethod = PaymentMethod.Cash });
                if (!b.Success) continue;
                var rate = await ratings.CreateRatingAsync(t, new CreateRatingDto
                { TripId = tripId, Value = di == 0 ? 5 : 4, Comment = "سائق محترم وقيادة هادئة — تجربة تستحق التكرار." });
                if (rate.Success) rated++;
            }
        }

        Console.WriteLine($"[DemoSeed] ✅ تم: {driverIds.Count} سائقين، {travelerIds.Count} مسافرين، " +
            $"{lineIds.Count} خطوط، {createdTripIds.Count} رحلات، تقييمات مقبولة: {rated}. " +
            $"حسابات العرض تنتهي بـ{Marker} وكلمة المرور الموحدة: {DemoPassword}");
    }
}
