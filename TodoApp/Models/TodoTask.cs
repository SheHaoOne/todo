using System.Text.Json.Serialization;

namespace TodoApp.Models;

public class TodoTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsImportant { get; set; }
    public bool IsMyDay { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public Guid ListId { get; set; }
    public int SortOrder { get; set; }
    public List<TodoStep> Steps { get; set; } = [];

    [JsonIgnore]
    public bool HasDueDate => DueDate.HasValue;

    [JsonIgnore]
    public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.Today && !IsCompleted;

    [JsonIgnore]
    public bool IsDueToday => DueDate.HasValue && DueDate.Value.Date == DateTime.Today;

    [JsonIgnore]
    public int CompletedStepCount => Steps.Count(s => s.IsCompleted);

    [JsonIgnore]
    public int TotalStepCount => Steps.Count;

    [JsonIgnore]
    public bool HasSteps => Steps.Count > 0;
}
