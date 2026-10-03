using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Request payload for creating a new subtask.</summary>
public class CreateSubtaskRequest
{
    /// <summary>Title of the subtask.</summary>
    [Required(ErrorMessage = "Tiêu đề subtask không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề subtask tối đa 255 ký tự.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Display sequence index.</summary>
    public int OrderIndex { get; set; } = 0;
}
