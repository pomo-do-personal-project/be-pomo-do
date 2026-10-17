using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Tasks;

// Request payload for updating an existing subtask.
public class UpdateSubtaskRequest
{
    // Updated title of the subtask.
    [Required(ErrorMessage = "Tiêu đề subtask không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề subtask tối đa 255 ký tự.")]
    public string Title { get; set; } = string.Empty;

    // Updated completion status flag.
    public bool IsCompleted { get; set; }

    // Updated order index.
    public int OrderIndex { get; set; }
}
