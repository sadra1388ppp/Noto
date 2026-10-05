using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleReminders.Models;

public sealed class Reminder : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _notes = string.Empty;
    private DateTime? _dueDate;
    private bool _isCompleted;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetField(ref _notes, value);
    }

    public DateTime? DueDate
    {
        get => _dueDate;
        set => SetField(ref _dueDate, value);
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetField(ref _isCompleted, value);
    }

    public string? ListId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
