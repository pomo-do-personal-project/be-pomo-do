namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Represents a tag associated with a task.</summary>
public class TagDto
{
    /// <summary>Unique identifier of the tag.</summary>
    public Guid Id { get; set; }

    /// <summary>Display name of the tag.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Hex color string for badge rendering (e.g. #3B82F6).</summary>
    public string ColorHex { get; set; } = "#3B82F6";
}
