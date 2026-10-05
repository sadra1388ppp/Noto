using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleReminders.Models;

public sealed class UserProgress : INotifyPropertyChanged
{
    private int _coins = 50;
    private int _xp;
    private int _totalCompleted;

    public int Coins
    {
        get => _coins;
        set => SetField(ref _coins, Math.Max(0, value));
    }

    public int XP
    {
        get => _xp;
        set => SetField(ref _xp, Math.Max(0, value), nameof(XP));
    }

    public int TotalCompleted
    {
        get => _totalCompleted;
        set => SetField(ref _totalCompleted, Math.Max(0, value));
    }

    public List<Guid> RewardedReminderIds { get; set; } = [];

    public List<string> UnlockedAchievementIds { get; set; } = [];

    public List<string> OwnedStoreItemIds { get; set; } = ["theme.default"];

    public string EquippedThemeId { get; set; } = "theme.default";

    public int Level => XP / 100 + 1;

    public int LevelXP => XP % 100;

    public int XPToNextLevel => 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyProgressChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LevelXP)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(XPToNextLevel)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Coins)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(XP)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalCompleted)));
    }

    private void SetField<T>(ref T field, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName == nameof(XP))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LevelXP)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(XPToNextLevel)));
        }
    }
}
