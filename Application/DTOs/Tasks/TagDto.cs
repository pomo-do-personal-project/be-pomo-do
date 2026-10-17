namespace be_pomo_do.Application.DTOs.Tasks;

// Represents a tag associated with a task.
public class TagDto
{
    // Unique identifier of the tag.
    public Guid Id { get; set; }

    // Display name of the tag.
    public string Name { get; set; } = string.Empty;

    // Hex color string for badge rendering (e.g. #3B82F6).
    public string ColorHex { get; set; } = "#3B82F6";
}
