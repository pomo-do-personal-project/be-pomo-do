using System.ComponentModel.DataAnnotations;
using be_pomo_do.Core.ValueObjects;

namespace be_pomo_do.Application.DTOs.Pomodoro;

/// <summary>Request payload for starting a new focus timer session.</summary>
public class StartSessionRequest
{
    /// <summary>Optional task to track pomodoros for.</summary>
    public Guid? TaskId { get; set; }

    /// <summary>Session type: Pomo (1) or Stopwatch (2).</summary>
    public FocusSessionMode Mode { get; set; } = FocusSessionMode.Pomo;

    /// <summary>Planned duration in minutes (defaults to 25).</summary>
    [Range(1, 360, ErrorMessage = "Thời lượng phiên phải từ 1 đến 360 phút.")]
    public int DurationMinutes { get; set; } = 25;
}
