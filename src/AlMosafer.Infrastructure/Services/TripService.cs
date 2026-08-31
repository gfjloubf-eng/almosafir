using AlMosafer.Application.DTOs.Trips;
using AlMosafer.Application.Interfaces;
using AlMosafer.Domain.Entities;
using AlMosafer.Domain.Enums;
using AlMosafer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Services;

public class TripService : ITripService
{
    private readonly AlMosaferDbContext _dbContext;

    public TripService(AlMosaferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool Success, string Message, int? TripId)> CreateTripAsync(int driverId, CreateTripDto dto)
    {
        var driver = await _dbContext.Users.FindAsync(driverId);
        if (driver == null || (driver.Role != UserRole.Driver && driver.Role != UserRole.Admin))
        {
            return (false, "عذراً، يجب أن تكون حساب سائق معتمد لإنشاء رحلة.", null);
        }

        if (dto.TripTime <= DateTime.Now)
        {
            return (false, "تاريخ ووقت الرحلة يجب أن يكون في المستقبل.", null);
        }

        if (dto.FromCity.Trim().Equals(dto.ToCity.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (false, "مدينة الانطلاق ومدينة الوصول لا يمكن أن تكونا متطابقتين.", null);
        }

        var trip = new Trip
        {
            DriverId = driverId,
            FromCity = dto.FromCity.Trim(),
            FromLocation = dto.FromLocation?.Trim() ?? string.Empty,
            ToCity = dto.ToCity.Trim(),
            TripTime = dto.TripTime,
            Seats = dto.Seats,
            PricePerSeat = dto.PricePerSeat,
            Description = dto.Description?.Trim(),
            VehicleInfo = string.IsNullOrWhiteSpace(dto.VehicleInfo) 
                ? $"{driver.VehicleModel} ({driver.PlateNumber})" 
                : dto.VehicleInfo.Trim(),
            Status = TripStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Trips.Add(trip);

        // Update driver total trips stats
        driver.TotalTrips += 1;

        await _dbContext.SaveChangesAsync();

        return (true, "تم إضافة الرحلة بنجاح!", trip.Id);
    }

    public async Task<(bool Success, string Message)> UpdateTripAsync(int driverId, UpdateTripDto dto)
    {
        var trip = await _dbContext.Trips.FindAsync(dto.TripId);
        if (trip == null)
        {
            return (false, "الرحلة غير موجودة.");
        }

        var driver = await _dbContext.Users.FindAsync(driverId);
        if (trip.DriverId != driverId && (driver == null || driver.Role != UserRole.Admin))
        {
            return (false, "لا تملك الصلاحية لتعديل هذه الرحلة.");
        }

        trip.FromCity = dto.FromCity.Trim();
        trip.FromLocation = dto.FromLocation?.Trim() ?? string.Empty;
        trip.ToCity = dto.ToCity.Trim();
        trip.TripTime = dto.TripTime;
        trip.Seats = dto.Seats;
        trip.PricePerSeat = dto.PricePerSeat;
        trip.Description = dto.Description?.Trim();
        trip.VehicleInfo = dto.VehicleInfo?.Trim();
        trip.Status = dto.Status;

        await _dbContext.SaveChangesAsync();

        return (true, "تم تحديث بيانات الرحلة بنجاح.");
    }

    public async Task<(bool Success, string Message)> CancelTripAsync(int driverId, int tripId)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Bookings)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null)
        {
            return (false, "الرحلة غير موجودة.");
        }

        var driver = await _dbContext.Users.FindAsync(driverId);
        if (trip.DriverId != driverId && (driver == null || driver.Role != UserRole.Admin))
        {
            return (false, "لا تملك الصلاحية لإلغاء هذه الرحلة.");
        }

        trip.Status = TripStatus.Cancelled;

        // Cancel all bookings associated with this trip
        foreach (var booking in trip.Bookings)
        {
            booking.Status = BookingStatus.Cancelled;
        }

        await _dbContext.SaveChangesAsync();

        return (true, "تم إلغاء الرحلة وكافة الحجوزات المرتبطة بها.");
    }

    public async Task<TripDetailsDto?> GetTripByIdAsync(int tripId)
    {
        var trip = await _dbContext.Trips
            .Include(t => t.Driver)
            .Include(t => t.Bookings)
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null) return null;

        int bookedSeats = trip.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Sum(b => b.SeatsBooked);

        int availableSeats = Math.Max(0, trip.Seats - bookedSeats);

        return new TripDetailsDto
        {
            Id = trip.Id,
            DriverId = trip.DriverId,
            DriverName = trip.Driver.Name,
            DriverPhone = trip.Driver.Phone,
            DriverRating = trip.Driver.Rating,
            PlateNumber = trip.Driver.PlateNumber,
            VehicleModel = trip.Driver.VehicleModel,
            FromCity = trip.FromCity,
            FromLocation = trip.FromLocation,
            ToCity = trip.ToCity,
            TripTime = trip.TripTime,
            TotalSeats = trip.Seats,
            AvailableSeats = availableSeats,
            PricePerSeat = trip.PricePerSeat,
            Description = trip.Description,
            VehicleInfo = trip.VehicleInfo,
            Status = trip.Status,
            CreatedAt = trip.CreatedAt
        };
    }

    public async Task<IEnumerable<TripDetailsDto>> SearchTripsAsync(TripSearchFilterDto filter)
    {
        var query = _dbContext.Trips
            .Include(t => t.Driver)
            .Include(t => t.Bookings)
            .Where(t => t.Status == TripStatus.Open && t.TripTime > DateTime.Now)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.FromCity))
        {
            query = query.Where(t => t.FromCity.ToLower().Contains(filter.FromCity.Trim().ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filter.ToCity))
        {
            query = query.Where(t => t.ToCity.ToLower().Contains(filter.ToCity.Trim().ToLower()));
        }

        if (filter.Date.HasValue)
        {
            var targetDate = filter.Date.Value.Date;
            query = query.Where(t => t.TripTime.Date == targetDate);
        }

        if (filter.MaxPrice.HasValue && filter.MaxPrice.Value > 0)
        {
            query = query.Where(t => t.PricePerSeat <= filter.MaxPrice.Value);
        }

        var tripsList = await query
            .OrderBy(t => t.TripTime)
            .ToListAsync();

        var result = new List<TripDetailsDto>();

        foreach (var trip in tripsList)
        {
            int bookedSeats = trip.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .Sum(b => b.SeatsBooked);

            int availableSeats = Math.Max(0, trip.Seats - bookedSeats);

            if (filter.AvailableSeatsOnly && availableSeats <= 0)
            {
                continue; // Skip full trips when filtering available seats
            }

            result.Add(new TripDetailsDto
            {
                Id = trip.Id,
                DriverId = trip.DriverId,
                DriverName = trip.Driver.Name,
                DriverPhone = trip.Driver.Phone,
                DriverRating = trip.Driver.Rating,
                PlateNumber = trip.Driver.PlateNumber,
                VehicleModel = trip.Driver.VehicleModel,
                FromCity = trip.FromCity,
                FromLocation = trip.FromLocation,
                ToCity = trip.ToCity,
                TripTime = trip.TripTime,
                TotalSeats = trip.Seats,
                AvailableSeats = availableSeats,
                PricePerSeat = trip.PricePerSeat,
                Description = trip.Description,
                VehicleInfo = trip.VehicleInfo,
                Status = trip.Status,
                CreatedAt = trip.CreatedAt
            });
        }

        return result;
    }

    public async Task<IEnumerable<TripDetailsDto>> GetDriverTripsAsync(int driverId)
    {
        var trips = await _dbContext.Trips
            .Include(t => t.Driver)
            .Include(t => t.Bookings)
            .Where(t => t.DriverId == driverId)
            .OrderByDescending(t => t.TripTime)
            .ToListAsync();

        return trips.Select(t =>
        {
            int bookedSeats = t.Bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.SeatsBooked);
            return new TripDetailsDto
            {
                Id = t.Id,
                DriverId = t.DriverId,
                DriverName = t.Driver.Name,
                DriverPhone = t.Driver.Phone,
                DriverRating = t.Driver.Rating,
                PlateNumber = t.Driver.PlateNumber,
                VehicleModel = t.Driver.VehicleModel,
                FromCity = t.FromCity,
                FromLocation = t.FromLocation,
                ToCity = t.ToCity,
                TripTime = t.TripTime,
                TotalSeats = t.Seats,
                AvailableSeats = Math.Max(0, t.Seats - bookedSeats),
                PricePerSeat = t.PricePerSeat,
                Description = t.Description,
                VehicleInfo = t.VehicleInfo,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            };
        });
    }

    public async Task<IEnumerable<TripDetailsDto>> GetInternalLinesAsync()
    {
        // «مرحلة 0» للمواصلات الداخلية: الخط الداخلي = رحلة مدينتها واحدة (FromCity == ToCity)
        // بلا أي عمود أو هجرة مخطط — الحي/المنطقة تُذكر في نقطة التجمع ووصف الرحلة
        var tripsList = await _dbContext.Trips
            .AsNoTracking()
            .Include(t => t.Driver)
            .Include(t => t.Bookings)
            .Where(t => t.FromCity == t.ToCity && t.Status == TripStatus.Open && t.TripTime > DateTime.Now)
            .OrderBy(t => t.TripTime)
            .ToListAsync();

        var result = new List<TripDetailsDto>();
        foreach (var trip in tripsList)
        {
            int bookedSeats = trip.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .Sum(b => b.SeatsBooked);

            result.Add(new TripDetailsDto
            {
                Id = trip.Id,
                DriverId = trip.DriverId,
                DriverName = trip.Driver.Name,
                DriverPhone = trip.Driver.Phone,
                DriverRating = trip.Driver.Rating,
                PlateNumber = trip.Driver.PlateNumber,
                VehicleModel = trip.Driver.VehicleModel,
                FromCity = trip.FromCity,
                FromLocation = trip.FromLocation,
                ToCity = trip.ToCity,
                TripTime = trip.TripTime,
                TotalSeats = trip.Seats,
                AvailableSeats = Math.Max(0, trip.Seats - bookedSeats),
                PricePerSeat = trip.PricePerSeat,
                Description = trip.Description,
                VehicleInfo = trip.VehicleInfo,
                Status = trip.Status,
                CreatedAt = trip.CreatedAt
            });
        }

        return result;
    }

    public async Task<(bool Success, string Message)> StartTripAsync(int driverId, int tripId)
    {
        var trip = await _dbContext.Trips.FindAsync(tripId);
        if (trip == null)
        {
            return (false, "الرحلة غير موجودة.");
        }

        var driver = await _dbContext.Users.FindAsync(driverId);
        if (trip.DriverId != driverId && (driver == null || driver.Role != UserRole.Admin))
        {
            return (false, "لا تملك الصلاحية لبدء هذه الرحلة.");
        }

        if (trip.Status != TripStatus.Open)
        {
            return (false, "لا يمكن بدء رحلة غير مفتوحة (قد تكون منطلقة أو ملغاة أو مكتملة بالفعل).");
        }

        trip.Status = TripStatus.Started;
        await _dbContext.SaveChangesAsync();

        return (true, "تمنياتنا برحلة آمنة! تم تسجيل انطلاق الرحلة ولم يعد الحجز متاحاً لمقاعد جديدة.");
    }

    public async Task<(bool Success, string Message)> CompleteTripAsync(int driverId, int tripId)
    {
        var trip = await _dbContext.Trips.FindAsync(tripId);
        if (trip == null)
        {
            return (false, "الرحلة غير موجودة.");
        }

        var driver = await _dbContext.Users.FindAsync(driverId);
        if (trip.DriverId != driverId && (driver == null || driver.Role != UserRole.Admin))
        {
            return (false, "لا تملك الصلاحية لإنهاء هذه الرحلة.");
        }

        if (trip.Status != TripStatus.Started)
        {
            return (false, "لا يمكن إنهاء رحلة لم تنطلق بعد (ابدأ الرحلة أولاً).");
        }

        trip.Status = TripStatus.Completed;
        await _dbContext.SaveChangesAsync();

        return (true, "وصلت الرحلة بسلامة! تم إنهاء الرحلة وتسجيلها مكتملة.");
    }
}
