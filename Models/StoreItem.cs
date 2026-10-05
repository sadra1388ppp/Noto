using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleReminders.Models;

public sealed class StoreItem : INotifyPropertyChanged
{
    private bool _isOwned;
    private bool _isEquipped;

    public string Id { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public int Price { get; init; }

    public bool IsOwned
    {
        get => _isOwned;
        set => SetField(ref _isOwned, value);
    }

    public bool IsEquipped
    {
        get => _isEquipped;
        set => SetField(ref _isEquipped, value);
    }

    public string ActionText =>
        IsEquipped ? "Equipped" :
        IsOwned ? "Equip" :
        Price == 0 ? "Free" :
        $"Buy • {Price}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName is nameof(IsOwned) or nameof(IsEquipped))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionText)));
        }
    }
}
