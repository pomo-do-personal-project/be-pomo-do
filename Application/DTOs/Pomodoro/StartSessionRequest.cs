using System.ComponentModel.DataAnnotations;
using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.DTOs.Pomodoro;

// Request payload for starting a new focus timer session.
public class StartSessionRequest
{
    // Optional task to track pomodoros for.
    public Guid? TaskId { get; set; }

    // Session type: Pomo (1) or Stopwatch (2).
    public FocusSessionMode Mode { get; set; } = FocusSessionMode.Pomo;

    // Planned duration in minutes (defaults to 25).
    [Range(1, 360, ErrorMessage = "Thời lượng phiên phải từ 1 đến 360 phút.")]
    public int DurationMinutes { get; set; } = 25;
}
