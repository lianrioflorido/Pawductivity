namespace Pawductivity.Models;

public sealed record AppTheme(
    string Key,
    string DisplayName,
    Color Background,
    Color Surface,
    Color Primary,
    Color PrimaryHover,
    Color Secondary,
    Color Accent,
    Color TextDark,
    Color TextMuted,
    Color HealthBar,
    Color MoodBar,
    Color XpBar,
    Color CompletedTask,
    Color TextGreen,
    Color OverdueTask,
    Color CardBorder);
