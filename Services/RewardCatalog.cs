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

    public static List<Achievement> CreateAchievements() =>
    [
        new Achievement
        {
            Id = "achievement.first-task",
            Name = "First Step",
            Description = "Complete your first reminder.",
            RewardCoins = 25,
            RewardXP = 0
        },
        new Achievement
        {
            Id = "achievement.five-tasks",
            Name = "Getting Started",
            Description = "Complete 5 reminders.",
            RewardCoins = 50,
            RewardXP = 0
        },
        new Achievement
        {
            Id = "achievement.ten-tasks",
            Name = "Getting Things Done",
            Description = "Complete 10 reminders.",
            RewardCoins = 100,
            RewardXP = 0
        },
        new Achievement
        {
            Id = "achievement.twenty-five-tasks",
            Name = "Productive",
            Description = "Complete 25 reminders.",
            RewardCoins = 150,
            RewardXP = 0
        },
        new Achievement
        {
            Id = "achievement.fifty-tasks",
            Name = "Consistent",
            Description = "Complete 50 reminders.",
            RewardCoins = 250,
            RewardXP = 0
        },
        new Achievement
        {
            Id = "achievement.hundred-tasks",
            Name = "Master Planner",
            Description = "Complete 100 reminders.",
            RewardCoins = 500,
            RewardXP = 0
        }
    ];
}
