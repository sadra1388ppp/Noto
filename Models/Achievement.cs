using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleReminders.Models;

public sealed class Achievement : INotifyPropertyChanged
{
    private bool _isUnlocked;

    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int RewardCoins { get; init; }

    public int RewardXP { get; init; }

    public bool IsUnlocked
    {
        get => _isUnlocked;
        set => SetField(ref _isUnlocked, value);
    }

    public string StatusText => IsUnlocked
        ? $"Unlocked • +{RewardCoins} coins"
        : $"Reward • {RewardCoins} coins";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName == nameof(IsUnlocked))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
        }
    }
}
