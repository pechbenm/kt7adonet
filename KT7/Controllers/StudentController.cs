using KT7.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KT7.Data;
using KT7.Dtos;

namespace PracticeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StudentsController(AppDbContext db) => _db = db;

    // GET api/students?group=IS-21
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetAll([FromQuery] string? group)
    {
        var query = _db.Students.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(group))
            query = query.Where(s => s.Group == group);

        return Ok(await query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Student>> GetById(int id)
    {
        var student = await _db.Students.FindAsync(id);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<Student>> Create(StudentRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Students.AnyAsync(s => s.Email == email))
            return Problem(title: "Конфликт данных",
                           detail: "Студент с таким Email уже существует.",
                           statusCode: StatusCodes.Status409Conflict);

        var student = new Student();
        Apply(student, request, email);

        _db.Students.Add(student);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, StudentRequest request)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is null) return NotFound();

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Students.AnyAsync(s => s.Email == email && s.Id != id))
            return Problem(title: "Конфликт данных",
                           detail: "Этот Email уже используется другим студентом.",
                           statusCode: StatusCodes.Status409Conflict);

        Apply(student, request, email);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student is null) return NotFound();

        _db.Students.Remove(student);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static void Apply(Student s, StudentRequest r, string email)
    {
        s.FirstName = r.FirstName.Trim();
        s.LastName = r.LastName.Trim();
        s.Email = email;
        s.Group = r.Group?.Trim();
        s.Age = r.Age;
    }
}