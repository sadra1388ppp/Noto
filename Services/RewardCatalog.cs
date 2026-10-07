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

        new()
        {
            Id = "gun.phantom",
            Name = "Phantom",
            Description = "A sleek futuristic rifle silhouette with a sharp, stealth-first profile.",
            Category = "Guns",
            Price = 90,
            PreviewBackground = "#121722",
            PreviewAccent = "#8FA8FF",
            PreviewSecondary = "#52647F",
            PreviewSoft = "#202A3A",
            PreviewGeometry = "M 16,50 L 38,36 L 92,36 L 111,28 L 137,31 L 137,37 L 168,37 L 184,43 L 184,50 L 145,50 L 142,58 L 120,58 L 109,53 L 80,53 L 66,69 L 50,69 L 57,52 L 41,52 L 28,61 L 16,59 L 23,50 Z"
        },

        new()
        {
            Id = "gun.nebula",
            Name = "Nebula",
            Description = "A compact sci-fi sidearm with a smooth body, bright core, and clean silhouette.",
            Category = "Guns",
            Price = 150,
            PreviewBackground = "#171225",
            PreviewAccent = "#C08CFF",
            PreviewSecondary = "#6FE7FF",
            PreviewSoft = "#2C2042",
            PreviewGeometry = "M 25,46 L 47,34 L 89,34 L 111,39 L 142,39 L 159,45 L 159,51 L 112,51 L 105,58 L 92,58 L 96,51 L 68,51 L 58,69 L 42,69 L 48,51 L 25,54 Z"
        },

        new()
        {
            Id = "gun.vortex",
            Name = "Vortex",
            Description = "An aggressive fantasy-tech SMG silhouette built around a curved energy chamber.",
            Category = "Guns",
            Price = 210,
            PreviewBackground = "#20131C",
            PreviewAccent = "#FF8B6B",
            PreviewSecondary = "#FF55B3",
            PreviewSoft = "#432432",
            PreviewGeometry = "M 18,48 L 36,38 L 72,38 L 82,31 L 116,31 L 128,37 L 156,37 L 177,43 L 177,50 L 150,50 L 143,57 L 124,57 L 116,51 L 93,51 L 87,63 L 73,63 L 76,51 L 51,51 L 43,64 L 28,64 L 33,51 L 18,54 Z"
        }
    ];

    public static List<Achievement> CreateAchievements() => [];
}
