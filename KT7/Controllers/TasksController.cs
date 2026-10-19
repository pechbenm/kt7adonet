using KT7.Data;
using KT7.Dtos;
using KT7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KT7.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    public TasksController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAll([FromQuery] TaskItemStatus? status)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
            return Problem(title: "Некорректный статус",
                           detail: "Допустимые значения: New, InProgress, Done.",
                           statusCode: StatusCodes.Status400BadRequest);

        var query = _db.TaskItems.AsNoTracking();
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        return Ok(await query.OrderByDescending(t => t.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _db.TaskItems.FindAsync(id);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(CreateTaskRequest request)
    {
        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = TaskItemStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskItem>> UpdateStatus(int id, UpdateTaskStatusRequest request)
    {
        var task = await _db.TaskItems.FindAsync(id);
        if (task is null) return NotFound();

        task.Status = request.Status!.Value;
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.TaskItems.FindAsync(id);
        if (task is null) return NotFound();

        _db.TaskItems.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}