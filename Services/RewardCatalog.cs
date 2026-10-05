using SimpleReminders.Models;

namespace SimpleReminders.Services;

public static class RewardCatalog
{
    public static List<StoreItem> CreateStoreItems() =>
    [
        new StoreItem
        {
            Id = "theme.default",
            Name = "Classic Blue",
            Description = "The original SimpleReminders theme.",
            Category = "Themes",
            Price = 0
        },
        new StoreItem
        {
            Id = "theme.ocean",
            Name = "Ocean",
            Description = "A calm blue accent for your workspace.",
            Category = "Themes",
            Price = 100
        },
        new StoreItem
        {
            Id = "theme.violet",
            Name = "Violet",
            Description = "A rich purple accent for your reminders.",
            Category = "Themes",
            Price = 150
        },
        new StoreItem
        {
            Id = "theme.forest",
            Name = "Forest",
            Description = "A fresh green accent for your workspace.",
            Category = "Themes",
            Price = 200
        },
        new StoreItem
        {
            Id = "theme.sunset",
            Name = "Sunset",
            Description = "A warm orange accent for your workspace.",
            Category = "Themes",
            Price = 250
        }
    ];

    public static List<Achievement> CreateAchievements() => [];
}
