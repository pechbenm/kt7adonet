using KT7.Models;
using System.ComponentModel.DataAnnotations;

namespace KT7.Dtos;

public class StudentRequest
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(50)] public string? Group { get; set; }
    [Range(16, 100)] public int Age { get; set; }
}

public class CreateTaskRequest
{
    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
}

public class UpdateTaskStatusRequest
{
    [Required]
    public TaskItemStatus? Status { get; set; }
}

public class CreateResourceRequest
{
    [Required, MaxLength(50)] public string Type { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Range(1, 10000)] public int Capacity { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
}

public class CreateBookingRequest
{
    [Required] public int? ResourceId { get; set; }
    [Required, MaxLength(100)] public string CustomerName { get; set; } = string.Empty;
    [Required] public DateTime? Start { get; set; }
    [Required] public DateTime? End { get; set; }
}

public class UpdateBookingRequest
{
    [MaxLength(100)] public string? CustomerName { get; set; }  
    [Required] public DateTime? Start { get; set; }
    [Required] public DateTime? End { get; set; }
}
