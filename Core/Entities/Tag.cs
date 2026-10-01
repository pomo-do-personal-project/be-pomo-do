namespace be_pomo_do.Core.Entities;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#3B82F6";

    // Navigation properties
    public User? User { get; set; }
    public ICollection<TaskTag> TaskTags { get; set; } = [];
}
