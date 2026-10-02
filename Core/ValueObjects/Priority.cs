namespace be_pomo_do.Core.ValueObjects;

// Eisenhower Matrix 4 Priority levels
public enum Priority
{
    ImportantUrgent = 1,      // Do first
    ImportantNotUrgent = 2,   // Schedule
    NotImportantUrgent = 3,   // Delegate
    NotImportantNotUrgent = 4 // Don't do
}
