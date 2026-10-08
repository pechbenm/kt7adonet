namespace KT7.Models;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Group { get; set; }
    public int Age { get; set; }
}

public enum TaskItemStatus { New, InProgress, Done }   

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.New;
    public DateTime CreatedAt { get; set; }            
}

public class Resource
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;   
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Description { get; set; }
}

public enum BookingStatus { Active, Cancelled }

public class Booking
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public Resource? Resource { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime Start { get; set; }                
    public DateTime End { get; set; }                  
    public BookingStatus Status { get; set; } = BookingStatus.Active;
    public DateTime CreatedAt { get; set; }
}