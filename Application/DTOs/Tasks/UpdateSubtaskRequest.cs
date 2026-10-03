using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Request payload for updating an existing subtask.</summary>
public class UpdateSubtaskRequest
{
    /// <summary>Updated title of the subtask.</summary>
    [Required(ErrorMessage = "Tiêu đề subtask không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề subtask tối đa 255 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Updated completion status flag.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Updated order index.</summary>
    public int OrderIndex { get; set; }
}
