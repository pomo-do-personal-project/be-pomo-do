namespace be_pomo_do.Core.Entities;

public class Subtask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public int OrderIndex { get; set; } = 0;

    // Navigation property
    public TaskItem? Task { get; set; }
}
