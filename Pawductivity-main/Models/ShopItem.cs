namespace Pawductivity.Models;

public class ShopItem
{
    public string Name        { get; init; } = string.Empty;
    public string Emoji       { get; init; } = "🎁";
    public string Description { get; init; } = string.Empty;
    public int    Cost        { get; init; }
    public int    HealthBoost { get; init; }
    public int    MoodBoost   { get; init; }

    public static List<ShopItem> DefaultShop() =>
    [
        new ShopItem { Name="Pink Ribbon",     Emoji="🎀", Description="A cute hair ribbon!",          Cost=10, MoodBoost=15 },
        new ShopItem { Name="Star Cookie",     Emoji="🍪", Description="A yummy star-shaped cookie.",  Cost=15, HealthBoost=20, MoodBoost=10 },
        new ShopItem { Name="Strawberry Milk", Emoji="🍓", Description="Refreshing strawberry milk!",  Cost=20, HealthBoost=30 },
        new ShopItem { Name="Flower Crown",    Emoji="🌸", Description="A beautiful flower crown.",    Cost=25, MoodBoost=30 },
        new ShopItem { Name="Cozy Blanket",    Emoji="🛏️", Description="Wrap your pet in warmth.",    Cost=30, HealthBoost=25, MoodBoost=20 },
        new ShopItem { Name="Rainbow Toy",     Emoji="🌈", Description="Hours of fun!",               Cost=40, MoodBoost=40 },
    ];
}
