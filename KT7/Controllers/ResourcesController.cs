using KT7.Data;
using KT7.Dtos;
using KT7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KT7.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ResourcesController(AppDbContext db) => _db = db;

    // GET api/resources?type=room
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Resource>>> GetAll([FromQuery] string? type)
    {
        var query = _db.Resources.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.Trim().ToLowerInvariant();
            query = query.Where(r => r.Type == t);
        }
        return Ok(await query.OrderBy(r => r.Type).ThenBy(r => r.Name).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Resource>> GetById(int id)
    {
        var resource = await _db.Resources.FindAsync(id);
        return resource is null ? NotFound() : Ok(resource);
    }

    [HttpPost]
    public async Task<ActionResult<Resource>> Create(CreateResourceRequest request)
    {
        var resource = new Resource
        {
            Type = request.Type.Trim().ToLowerInvariant(),
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            Description = request.Description?.Trim()
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<Resource>>> GetAvailable(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] string? type, [FromQuery] int? minCapacity)
    {
        if (from is null || to is null)
            return Problem(title: "Не задан период", detail: "Укажите параметры from и to.",
                           statusCode: StatusCodes.Status400BadRequest);

        var start = from.Value.ToUniversalTime();
        var end = to.Value.ToUniversalTime();
        if (end <= start)
            return Problem(title: "Некорректный период", detail: "to должен быть позже from.",
                           statusCode: StatusCodes.Status400BadRequest);

        var query = _db.Resources.AsNoTracking().Where(r =>
            !_db.Bookings.Any(b => b.ResourceId == r.Id
                                   && b.Status == BookingStatus.Active
                                   && b.Start < end && b.End > start));

        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.Trim().ToLowerInvariant();
            query = query.Where(r => r.Type == t);
        }
        if (minCapacity.HasValue)
            query = query.Where(r => r.Capacity >= minCapacity.Value);

        return Ok(await query.OrderBy(r => r.Type).ThenBy(r => r.Name).ToListAsync());
    }
}