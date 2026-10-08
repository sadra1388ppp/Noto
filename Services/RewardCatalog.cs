using SimpleReminders.Models;

namespace SimpleReminders.Services;

public static class RewardCatalog
{
    public static List<StoreItem> CreateStoreItems() =>
    [
        new()
        {
            Id = "theme.halloween-night",
            Name = "Halloween Night",
            Description = "A haunted night workspace with moonlight, hanging pumpkins, drifting fog, and animated Halloween atmosphere.",
            Category = "Themes",
            Price = 300,
            PreviewBackground = "#100B18",
            PreviewAccent = "#FF8A24",
            PreviewSecondary = "#A96BFF",
            PreviewSoft = "#2A1838"
        },
        new()
        {
            Id = "theme.moonlit-forest",
            Name = "Moonlit Forest",
            Description = "A dark enchanted forest with pine silhouettes, glowing fireflies, leafy canopy details, and a calm night atmosphere.",
            Category = "Themes",
            Price = 300,
            PreviewBackground = "#08170D",
            PreviewAccent = "#67BF62",
            PreviewSecondary = "#D8FF8B",
            PreviewSoft = "#163B20"
        }
    ];

    public static List<Achievement> CreateAchievements() => [];
}
