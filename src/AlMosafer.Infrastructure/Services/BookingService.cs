using System.Data;
using AlMosafer.Application.DTOs.Bookings;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly AlMosaferDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly IConversationService _conversationService;

    public BookingService(
        AlMosaferDbContext dbContext,
        IPaymentService paymentService,
        INotificationService notificationService,
        IConversationService conversationService)
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
        _notificationService = notificationService;
        _conversationService = conversationService;
    }

    public async Task<(bool Success, string Message, int? BookingId)> CreateBookingAsync(int travelerId, CreateBookingDto dto)
    {
        if (dto.SeatsBooked <= 0)
        {
            return (false, "عدد المقاعد المحجوزة يجب أن يكون مقعداً واحداً على الأقل.", null);
        }

        var traveler = await _dbContext.Users.FindAsync(travelerId);
        if (traveler == null)
        {
            return (false, "المستخدم غير موجود.", null);
        }

        // Execute inside an explicit Database Transaction to prevent Concurrency Race Conditions
        var isInMemory = _dbContext.Database.ProviderName?.Contains("InMemory") == true;
        using var transaction = isInMemory ? null : await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var trip = await _dbContext.Trips
                .Include(t => t.Driver)
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == dto.TripId);

            if (trip == null)
            {
                return (false, "الرحلة غير موجودة.", null);
            }

            if (trip.Status != TripStatus.Open)
            {
                return (false, "عذراً، هذه الرحلة مغلقة أو ملغاة أو منتهية.", null);
            }

            if (trip.TripTime <= DateTime.Now)
            {
                return (false, "عذراً، انتهمت الرحلة وتاريخ انطلاقها قد مضى.", null);
            }

            if (trip.DriverId == travelerId)
            {
                return (false, "لا يمكنك حجز مقعد في رحلة تقوم أنت بقيادتها!", null);
            }

            // Duplicate Booking Check
            var hasActiveBooking = trip.Bookings
                .Any(b => b.TravelerId == travelerId && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded));

            if (hasActiveBooking)
            {
                return (false, "لديك حجز مؤكد سابق في هذه الرحلة بالفعل.", null);
            }

            // Atomic Seat Availability Calculation
            int bookedSeats = trip.Bookings
                .Where(b => (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded))
                .Sum(b => b.SeatsBooked);

            int availableSeats = trip.Seats - bookedSeats;

            if (dto.SeatsBooked > availableSeats)
            {
                if (availableSeats <= 0)
                {
                    return (false, "عذراً، هذه الرحلة مكتملة المقاعد بالكامل (Full).", null);
                }
                return (false, $"عذراً، المقاعد المتبقية في هذه الرحلة هي {availableSeats} مقعد فقط.", null);
            }

            // Create Booking Entity — P43 التذكرة الموقّعة: رمز سري عشوائي 32 بايت لكل حجز جديد
            var booking = new Booking
            {
                TripId = dto.TripId,
                TravelerId = travelerId,
                SeatsBooked = dto.SeatsBooked,
                Status = BookingStatus.Confirmed,
                BookingTime = DateTime.UtcNow,
                TicketSecret = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            };

            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            // Delegate Payment Processing (تجريبي فوري أو نقد عند الركوب)
            decimal totalAmount = dto.SeatsBooked * trip.PricePerSeat;
            if (dto.PaymentMethod == PaymentMethod.Cash)
            {
                await _paymentService.RegisterCashPaymentAsync(booking.Id, totalAmount);
            }
            else
            {
                await _paymentService.ProcessBookingPaymentAsync(booking.Id, totalAmount);
            }

            // Update Driver Earnings Stats
            trip.Driver.TotalEarnings += totalAmount;
            await _dbContext.SaveChangesAsync();

            // Delegate Conversation Creation
            await _conversationService.EnsureBookingConversationExistsAsync(booking.Id, trip.Id, trip.DriverId, travelerId);

            // Delegate Notifications
            await _notificationService.SendNotificationAsync(
                travelerId,
                "تأكيد الحجز بنجاح 🎫",
                $"تم تأكيد حجزك لعدد {dto.SeatsBooked} مقعد في رحلة {trip.FromCity} ← {trip.ToCity}." + (dto.PaymentMethod == PaymentMethod.Cash ? " المبلغ مستحق نقداً عند الركوب للسائق مباشرة. أحضر المبلغ المطلوب معك." : string.Empty),
                NotificationType.Booking);

            await _notificationService.SendNotificationAsync(
                trip.DriverId,
                "حجز جديد في رحلتك 🚗",
                $"قام المسافر {traveler.Name} بحجز {dto.SeatsBooked} مقعد في رحلتك المتجهة إلى {trip.ToCity}.",
                NotificationType.Booking);

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return (true, "تم حجز المقعد وتأكيد العملية بنجاح!", booking.Id);
        }
        catch (Exception)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            throw;
        }
    }

    public async Task<(bool Success, string Message)> CancelBookingAsync(int userId, int bookingId)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Trip)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return (false, "الحجز غير موجود.");
        }

        var user = await _dbContext.Users.FindAsync(userId);
        if (booking.TravelerId != userId && booking.Trip.DriverId != userId && (user == null || user.Role != UserRole.Admin))
        {
            return (false, "لا تملك الصلاحية لإلغاء هذا الحجز.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return (false, "الحجز ملغى بالفعل.");
        }

        booking.Status = BookingStatus.Cancelled;

        if (booking.Payment != null)
        {
            booking.Payment.Status = PaymentStatus.Failed;
        }

        await _dbContext.SaveChangesAsync();

        return (true, "تم إلغاء الحجز بنجاح.");
    }

    public async Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _dbContext.Bookings
            .AsNoTracking()
            .Include(b => b.Trip)
                .ThenInclude(t => t.Driver)
            .Include(b => b.Traveler)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null) return null;

        return new BookingDetailsDto
        {
            BookingId = booking.Id,
            TripId = booking.TripId,
            FromCity = booking.Trip.FromCity,
            ToCity = booking.Trip.ToCity,
            TripTime = booking.Trip.TripTime,
            DriverName = booking.Trip.Driver.Name,
            DriverPhone = booking.Trip.Driver.Phone,
            TravelerId = booking.TravelerId,
            TravelerName = booking.Traveler.Name,
            TravelerPhone = booking.Traveler.Phone,
            SeatsBooked = booking.SeatsBooked,
            PricePerSeat = booking.Trip.PricePerSeat,
            TotalAmount = booking.SeatsBooked * booking.Trip.PricePerSeat,
            Status = booking.Status,
            PaymentStatus = booking.Payment?.Status ?? PaymentStatus.Pending,
            BookingTime = booking.BookingTime
        };
    }

    public async Task<IEnumerable<BookingDetailsDto>> GetUserBookingsAsync(int travelerId)
    {
        var bookings = await _dbContext.Bookings
            .AsNoTracking()
            .Include(b => b.Trip)
                .ThenInclude(t => t.Driver)
            .Include(b => b.Traveler)
            .Include(b => b.Payment)
            .Where(b => b.TravelerId == travelerId)
            .OrderByDescending(b => b.BookingTime)
            .ToListAsync();

        return bookings.Select(b => new BookingDetailsDto
        {
            BookingId = b.Id,
            TripId = b.TripId,
            FromCity = b.Trip.FromCity,
            ToCity = b.Trip.ToCity,
            TripTime = b.Trip.TripTime,
            DriverName = b.Trip.Driver.Name,
            DriverPhone = b.Trip.Driver.Phone,
            TravelerId = b.TravelerId,
            TravelerName = b.Traveler.Name,
            TravelerPhone = b.Traveler.Phone,
            SeatsBooked = b.SeatsBooked,
            PricePerSeat = b.Trip.PricePerSeat,
            TotalAmount = b.SeatsBooked * b.Trip.PricePerSeat,
            Status = b.Status,
            PaymentStatus = b.Payment?.Status ?? PaymentStatus.Pending,
            BookingTime = b.BookingTime
        });
    }

    public async Task<IEnumerable<BookingDetailsDto>> GetTripBookingsAsync(int driverId, int tripId)
    {
        var bookings = await _dbContext.Bookings
            .AsNoTracking()
            .Include(b => b.Trip)
                .ThenInclude(t => t.Driver)
            .Include(b => b.Traveler)
            .Include(b => b.Payment)
            .Where(b => b.TripId == tripId && b.Trip.DriverId == driverId)
            .OrderByDescending(b => b.BookingTime)
            .ToListAsync();

        return bookings.Select(b => new BookingDetailsDto
        {
            BookingId = b.Id,
            TripId = b.TripId,
            FromCity = b.Trip.FromCity,
            ToCity = b.Trip.ToCity,
            TripTime = b.Trip.TripTime,
            DriverName = b.Trip.Driver.Name,
            DriverPhone = b.Trip.Driver.Phone,
            TravelerId = b.TravelerId,
            TravelerName = b.Traveler.Name,
            TravelerPhone = b.Traveler.Phone,
            SeatsBooked = b.SeatsBooked,
            PricePerSeat = b.Trip.PricePerSeat,
            TotalAmount = b.SeatsBooked * b.Trip.PricePerSeat,
            Status = b.Status,
            PaymentStatus = b.Payment?.Status ?? PaymentStatus.Pending,
            BookingTime = b.BookingTime
        });
    }
    public async Task<TripManifestDto?> GetTripManifestAsync(int driverId, int tripId)
    {
        var trip = await _dbContext.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip == null)
        {
            return null;
        }

        var driver = await _dbContext.Users.FindAsync(driverId);
        if (trip.DriverId != driverId && (driver == null || driver.Role != UserRole.Admin))
        {
            return null; // حارس الملكية — كشف الركوب لسائق الرحلة أو الإدمن فقط
        }

        var bookings = await _dbContext.Bookings
            .AsNoTracking()
            .Include(b => b.Traveler)
            .Include(b => b.Payment)
            .Where(b => b.TripId == tripId && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Boarded))
            .OrderBy(b => b.BookingTime)
            .ToListAsync();

        var passengers = bookings.Select(b => new ManifestPassengerDto
        {
            BookingId = b.Id,
            TravelerName = b.Traveler != null ? b.Traveler.Name : "مسافر",
            Phone = b.Traveler != null ? b.Traveler.Phone : null,
            SeatsBooked = b.SeatsBooked,
            PaymentStatus = b.Payment?.Status,
            Amount = b.Payment?.Amount ?? 0m,
            IsBoarded = b.Status == BookingStatus.Boarded
        }).ToList();

        return new TripManifestDto
        {
            TripId = trip.Id,
            Route = $"{trip.FromCity} ← {trip.ToCity}",
            TripTime = trip.TripTime,
            TotalSeats = trip.Seats,
            SeatsBookedTotal = passengers.Sum(p => p.SeatsBooked),
            BoardedCount = passengers.Count(p => p.IsBoarded),
            PendingBoardCount = passengers.Count(p => !p.IsBoarded),
            CashDueTotal = passengers.Where(p => p.PaymentStatus == PaymentStatus.Pending).Sum(p => p.Amount),
            Passengers = passengers
        };
    }

    public async Task<(bool Success, string Message)> MarkBoardedAsync(int driverId, int bookingId)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Trip)
            .Include(b => b.Traveler)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return (false, "الحجز غير موجود.");
        }

        var driver = await _dbContext.Users.FindAsync(driverId);
        if (booking.Trip.DriverId != driverId && (driver == null || driver.Role != UserRole.Admin))
        {
            return (false, "لا تملك الصلاحية لتأكيد صعود هذا الحجز.");
        }

        if (booking.Status == BookingStatus.Boarded)
        {
            return (false, "هذا المسافر مسجل كصاعد بالفعل.");
        }
        if (booking.Status != BookingStatus.Confirmed)
        {
            return (false, "لا يمكن تأكيد صعود حجز بحالته الحالية (ملغي أو مكتمل).");
        }

        booking.Status = BookingStatus.Boarded;
        await _dbContext.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            booking.TravelerId,
            "تم تسجيل صعودك ✅",
            $"أهلاً بك على متن رحلة {booking.Trip.FromCity} ← {booking.Trip.ToCity}. نتمنى لك رحلة سعيدة وآمنة!",
            NotificationType.Booking);

        return (true, $"سُجّل صعود {booking.Traveler?.Name ?? "المسافر"} — أهلاً به على متن الرحلة!");
    }

}