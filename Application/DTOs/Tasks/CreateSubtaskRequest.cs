using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Tasks;

// Request payload for creating a new subtask.
public class CreateSubtaskRequest
{
    // Title of the subtask.
    [Required(ErrorMessage = "Tiêu đề subtask không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề subtask tối đa 255 ký tự.")]
    public string Title { get; set; } = string.Empty;

    // Display sequence index.
    public int OrderIndex { get; set; } = 0;
}
