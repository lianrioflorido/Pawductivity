namespace Pawductivity.Models;

public record PetChangeResult(
    bool Success,
    int XpDelta = 0,
    int MoodDelta = 0,
    int HealthDelta = 0,
    int CoinDelta = 0,
    int AffectedTasks = 0,
    ShopItem? Item = null)
{
    public static PetChangeResult None { get; } = new(false);
}
