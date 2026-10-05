using SimpleReminders.Models;

namespace SimpleReminders.Services;

public static class RewardCatalog
{
    public static IReadOnlyList<StoreItem> StoreItems { get; } =
    [
        new StoreItem
        {
            Id = "theme.default",
            Name = "Classic Blue",
            Description = "The original SimpleReminders look.",
            Category = "Themes",
            Price = 0
        },
        new StoreItem
        {
            Id = "theme.ocean",
            Name = "Ocean",
            Description = "A cool blue accent for a calmer workspace.",
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
            Description = "A warm orange accent with a bright feel.",
            Category = "Themes",
            Price = 250
        }
    ];

    public static IReadOnlyList<Achievement> Achievements { get; } =
    [
        new Achievement
        {
            Id = "achievement.first-task",
            Name = "First Step",
            Description = "Complete your first reminder.",
            RewardCoins = 25,
            RewardXP = 25
        },
        new Achievement
        {
            Id = "achievement.ten-tasks",
            Name = "Getting Things Done",
            Description = "Complete 10 reminders.",
            RewardCoins = 75,
            RewardXP = 50
        },
        new Achievement
        {
            Id = "achievement.first-list",
            Name = "Organizer",
            Description = "Create your first custom list.",
            RewardCoins = 25,
            RewardXP = 25
        },
        new Achievement
        {
            Id = "achievement.five-lists",
            Name = "Master Organizer",
            Description = "Create 5 custom lists.",
            RewardCoins = 100,
            RewardXP = 75
        },
        new Achievement
        {
            Id = "achievement.level-five",
            Name = "Level 5",
            Description = "Reach level 5.",
            RewardCoins = 150,
            RewardXP = 100
        }
    ];
}
