namespace SimpleReminders.Models;

public sealed class ReminderList
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
}
