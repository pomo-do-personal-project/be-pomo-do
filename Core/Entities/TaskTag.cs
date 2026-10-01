namespace be_pomo_do.Core.Entities;

public class TaskTag
{
    public Guid TaskId { get; set; }
    public Guid TagId { get; set; }

    // Navigation properties
    public TaskItem? Task { get; set; }
    public Tag? Tag { get; set; }
}
