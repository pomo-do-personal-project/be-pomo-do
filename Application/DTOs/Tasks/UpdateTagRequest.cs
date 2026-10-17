using System.ComponentModel.DataAnnotations;

namespace be_pomo_do.Application.DTOs.Tasks;

// Request payload for updating an existing tag.
public class UpdateTagRequest
{
    // Display name of the tag.
    [Required(ErrorMessage = "Tên thẻ tag không được để trống.")]
    [MaxLength(50, ErrorMessage = "Tên thẻ tag tối đa 50 ký tự.")]
    public string Name { get; set; } = string.Empty;

    // Hex color code for visual badge (e.g. #3B82F6).
    [MaxLength(7, ErrorMessage = "Mã màu hex tối đa 7 ký tự.")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "Mã màu phải ở định dạng Hex hợp lệ (vd: #3B82F6).")]
    public string ColorHex { get; set; } = "#3B82F6";
}
