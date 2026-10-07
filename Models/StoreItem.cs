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

    public string PreviewBackground { get; init; } = "#16181D";

    public string PreviewAccent { get; init; } = "#FFFFFF";

    public string PreviewSecondary { get; init; } = "#8E96A3";

    public string PreviewSoft { get; init; } = "#272C34";

    public string PreviewGeometry { get; init; } = string.Empty;

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

    public string ActionText
    {
        get
        {
            if (Category.Equals("Guns", StringComparison.OrdinalIgnoreCase))
            {
                return IsOwned ? "Owned" : $"Buy • {Price}";
            }

            return IsEquipped ? "Equipped" :
                   IsOwned ? "Equip" :
                   Price == 0 ? "Free" :
                   $"Buy • {Price}";
        }
    }

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
