namespace be_pomo_do.Core.ValueObjects;

/// <summary>
/// Eisenhower Matrix (Matrix 4)
/// </summary>
public enum Priority
{
    ImportantUrgent = 1,      // Do first
    ImportantNotUrgent = 2,   // Schedule
    NotImportantUrgent = 3,   // Delegate
    NotImportantNotUrgent = 4 // Don't do
}
