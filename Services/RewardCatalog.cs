using SimpleReminders.Models;

namespace SimpleReminders.Services;

public static class RewardCatalog
{
    public static List<StoreItem> CreateStoreItems() =>
    [
        new()
        {
            Id = "theme.astral-bloom",
            Name = "Astral Bloom",
            Description = "A cosmic violet theme with an aurora glow and a dreamy premium feel.",
            Category = "Themes",
            Price = 160,
            PreviewBackground = "#171126",
            PreviewAccent = "#B18CFF",
            PreviewSecondary = "#5EE7FF",
            PreviewSoft = "#2B2146"
        },

        new()
        {
            Id = "theme.crimson-arcana",
            Name = "Crimson Arcana",
            Description = "A rich crimson-magenta theme inspired by enchanted royal halls and arcane light.",
            Category = "Themes",
            Price = 220,
            PreviewBackground = "#27111E",
            PreviewAccent = "#FF5E9C",
            PreviewSecondary = "#FFB86B",
            PreviewSoft = "#4A2034"
        },

        new()
        {
            Id = "theme.emerald-celestial",
            Name = "Emerald Celestial",
            Description = "A fantasy emerald theme with cool cyan highlights and a celestial atmosphere.",
            Category = "Themes",
            Price = 280,
            PreviewBackground = "#0E241F",
            PreviewAccent = "#5EF0CC",
            PreviewSecondary = "#8E7CFF",
            PreviewSoft = "#1D443B"
        },

    ];

    public static List<Achievement> CreateAchievements() => [];
}
