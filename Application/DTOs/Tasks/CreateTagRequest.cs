using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Tasks;

/// <summary>Request payload for creating a new custom tag.</summary>
public class CreateTagRequest
{
    /// <summary>Display name of the tag.</summary>
    [Required(ErrorMessage = "Tên thẻ tag không được để trống.")]
    [MaxLength(50, ErrorMessage = "Tên thẻ tag tối đa 50 ký tự.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Hex color code for visual badge (e.g. #3B82F6).</summary>
    [MaxLength(7, ErrorMessage = "Mã màu hex tối đa 7 ký tự.")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "Mã màu phải ở định dạng Hex hợp lệ (vd: #3B82F6).")]
    public string ColorHex { get; set; } = "#3B82F6";
}
