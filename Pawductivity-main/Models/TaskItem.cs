namespace Pawductivity.Models;

public enum TaskPriority { Low, Medium, High }

public class TaskItem
{
    public Guid         Id          { get; init; } = Guid.NewGuid();
    public string       Title       { get; set; }  = string.Empty;
    public string       Description { get; set; }  = string.Empty;
    public TaskPriority Priority    { get; set; }  = TaskPriority.Medium;
    public DateTime     DueDate     { get; set; }  = DateTime.Today;
    public bool         IsCompleted { get; set; }  = false;
    public DateTime?    CompletedAt { get; set; }

    public bool IsOverdue => !IsCompleted && DueDate.Date < DateTime.Today;

    public string PriorityEmoji => Priority switch
    {
        TaskPriority.High   => "🔴",
        TaskPriority.Medium => "🟡",
        TaskPriority.Low    => "🟢",
        _                   => "⚪"
    };

    public void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTime.Now;
    }
}
