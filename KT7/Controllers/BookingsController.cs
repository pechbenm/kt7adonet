using KT7.Data;
using KT7.Dtos;
using KT7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KT7.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BookingsController(AppDbContext db) => _db = db;

    // GET api/bookings?resourceId=1&status=Active
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Booking>>> GetAll(
        [FromQuery] int? resourceId, [FromQuery] BookingStatus? status)
    {
        var query = _db.Bookings.AsNoTracking().Include(b => b.Resource).AsQueryable();
        if (resourceId.HasValue) query = query.Where(b => b.ResourceId == resourceId.Value);
        if (status.HasValue) query = query.Where(b => b.Status == status.Value);

        return Ok(await query.OrderBy(b => b.Start).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Booking>> GetById(int id)
    {
        var booking = await _db.Bookings.Include(b => b.Resource).FirstOrDefaultAsync(b => b.Id == id);
        return booking is null ? NotFound() : Ok(booking);
    }

    // бронирование
    [HttpPost]
    public async Task<ActionResult<Booking>> Create(CreateBookingRequest request)
    {
        var start = request.Start!.Value.ToUniversalTime();
        var end = request.End!.Value.ToUniversalTime();

        var error = ValidatePeriod(start, end);
        if (error is not null)
            return Problem(title: "Некорректный период", detail: error,
                           statusCode: StatusCodes.Status400BadRequest);

        var resourceId = request.ResourceId!.Value;
        if (!await _db.Resources.AnyAsync(r => r.Id == resourceId))
            return Problem(title: "Ресурс не найден", detail: $"Ресурса с Id={resourceId} не существует.",
                           statusCode: StatusCodes.Status404NotFound);

        if (await HasConflictAsync(resourceId, start, end))
            return Problem(title: "Ресурс занят", detail: "В выбранный период ресурс уже забронирован.",
                           statusCode: StatusCodes.Status409Conflict);

        var booking = new Booking
        {
            ResourceId = resourceId,
            CustomerName = request.CustomerName.Trim(),
            Start = start,
            End = end,
            Status = BookingStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    // изменение даты и времени и имени
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Booking>> Update(int id, UpdateBookingRequest request)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();

        if (booking.Status == BookingStatus.Cancelled)
            return Problem(title: "Бронь отменена", detail: "Отменённую бронь изменить нельзя.",
                           statusCode: StatusCodes.Status409Conflict);

        var start = request.Start!.Value.ToUniversalTime();
        var end = request.End!.Value.ToUniversalTime();

        var error = ValidatePeriod(start, end);
        if (error is not null)
            return Problem(title: "Некорректный период", detail: error,
                           statusCode: StatusCodes.Status400BadRequest);

        if (await HasConflictAsync(booking.ResourceId, start, end, excludeId: booking.Id))
            return Problem(title: "Ресурс занят", detail: "В новый период ресурс уже забронирован.",
                           statusCode: StatusCodes.Status409Conflict);

        booking.Start = start;
        booking.End = end;
        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            booking.CustomerName = request.CustomerName.Trim();

        await _db.SaveChangesAsync();
        return Ok(booking);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();

        if (booking.Status == BookingStatus.Active)
        {
            booking.Status = BookingStatus.Cancelled;
            await _db.SaveChangesAsync();
        }
        return NoContent();   
    }

    private static string? ValidatePeriod(DateTime start, DateTime end)
    {
        if (end <= start) return "Время окончания должно быть позже времени начала.";
        if (start < DateTime.UtcNow) return "Нельзя бронировать на прошедшее время.";
        return null;
    }

    private Task<bool> HasConflictAsync(int resourceId, DateTime start, DateTime end, int excludeId = 0)
        => _db.Bookings.AnyAsync(b => b.ResourceId == resourceId
                                      && b.Status == BookingStatus.Active
                                      && b.Id != excludeId
                                      && b.Start < end && b.End > start);
}