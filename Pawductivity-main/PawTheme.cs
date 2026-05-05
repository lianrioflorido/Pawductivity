using Pawductivity.Models;

namespace Pawductivity;

/// Centralized theme system for the entire app.
/// Change the active palette here and forms can rebuild with the new colors.
public static class PawTheme
{
    public const string DefaultThemeKey = "pink";

    private static readonly AppTheme[] _themes =
    [
        new(
            "pink", "Pink Kawaii",
            Color.FromArgb(255, 240, 245), Color.FromArgb(255, 220, 230),
            Color.FromArgb(255, 105, 150), Color.FromArgb(240, 85, 130),
            Color.FromArgb(255, 182, 193), Color.FromArgb(255, 150, 200),
            Color.FromArgb(80, 30, 50), Color.FromArgb(160, 90, 120),
            Color.FromArgb(255, 80, 120), Color.FromArgb(255, 200, 80),
            Color.FromArgb(140, 200, 255), Color.FromArgb(150, 220, 150),
            Color.FromArgb(0, 100, 0), Color.FromArgb(255, 77, 77),
            Color.FromArgb(255, 160, 190)),

        new(
            "blue", "Blue Calm",
            Color.FromArgb(235, 247, 255), Color.FromArgb(211, 232, 246),
            Color.FromArgb(62, 135, 200), Color.FromArgb(45, 112, 176),
            Color.FromArgb(167, 211, 239), Color.FromArgb(94, 172, 220),
            Color.FromArgb(24, 60, 86), Color.FromArgb(83, 121, 148),
            Color.FromArgb(232, 84, 104), Color.FromArgb(95, 186, 170),
            Color.FromArgb(90, 145, 230), Color.FromArgb(165, 220, 190),
            Color.FromArgb(20, 116, 80), Color.FromArgb(230, 82, 92),
            Color.FromArgb(118, 178, 218)),

        new(
            "green", "Green Nature",
            Color.FromArgb(239, 248, 235), Color.FromArgb(218, 236, 207),
            Color.FromArgb(84, 150, 92), Color.FromArgb(63, 126, 73),
            Color.FromArgb(176, 218, 160), Color.FromArgb(132, 190, 116),
            Color.FromArgb(39, 78, 45), Color.FromArgb(92, 126, 82),
            Color.FromArgb(218, 92, 92), Color.FromArgb(236, 183, 72),
            Color.FromArgb(88, 164, 202), Color.FromArgb(177, 225, 165),
            Color.FromArgb(30, 112, 55), Color.FromArgb(212, 75, 78),
            Color.FromArgb(132, 184, 118)),

        new(
            "purple", "Purple Night",
            Color.FromArgb(39, 35, 56), Color.FromArgb(58, 51, 82),
            Color.FromArgb(172, 120, 220), Color.FromArgb(146, 94, 200),
            Color.FromArgb(111, 93, 150), Color.FromArgb(218, 140, 214),
            Color.FromArgb(245, 238, 255), Color.FromArgb(205, 191, 225),
            Color.FromArgb(238, 95, 136), Color.FromArgb(245, 192, 90),
            Color.FromArgb(132, 190, 245), Color.FromArgb(92, 145, 116),
            Color.FromArgb(154, 230, 182), Color.FromArgb(242, 104, 120),
            Color.FromArgb(124, 100, 166)),

        new(
            "strawberry", "Strawberry",
            Color.FromArgb(255, 246, 241), Color.FromArgb(255, 225, 218),
            Color.FromArgb(230, 82, 105), Color.FromArgb(205, 62, 86),
            Color.FromArgb(255, 186, 176), Color.FromArgb(125, 178, 98),
            Color.FromArgb(92, 42, 50), Color.FromArgb(154, 92, 92),
            Color.FromArgb(218, 70, 88), Color.FromArgb(238, 182, 64),
            Color.FromArgb(108, 176, 218), Color.FromArgb(184, 226, 156),
            Color.FromArgb(34, 122, 58), Color.FromArgb(218, 66, 76),
            Color.FromArgb(238, 142, 132)),
    ];

    private static AppTheme _activeTheme = _themes[0];

    public static IReadOnlyList<AppTheme> Themes => _themes;
    public static AppTheme ActiveTheme => _activeTheme;
    public static string ActiveThemeKey => _activeTheme.Key;

    public static Color Background => _activeTheme.Background;
    public static Color Surface => _activeTheme.Surface;
    public static Color Primary => _activeTheme.Primary;
    public static Color PrimaryHover => _activeTheme.PrimaryHover;
    public static Color Secondary => _activeTheme.Secondary;
    public static Color Accent => _activeTheme.Accent;
    public static Color TextDark => _activeTheme.TextDark;
    public static Color TextMuted => _activeTheme.TextMuted;
    public static Color HealthBar => _activeTheme.HealthBar;
    public static Color MoodBar => _activeTheme.MoodBar;
    public static Color XpBar => _activeTheme.XpBar;
    public static Color ButtonText => Color.White;
    public static Color CompletedTask => _activeTheme.CompletedTask;
    public static Color TextGreen => _activeTheme.TextGreen;
    public static Color OverdueTask => _activeTheme.OverdueTask;
    public static Color CardBorder => _activeTheme.CardBorder;

    public static readonly Font FontTitle = new("Segoe UI", 22f, FontStyle.Bold);
    public static readonly Font FontHeading = new("Segoe UI", 13f, FontStyle.Bold);
    public static readonly Font FontBody = new("Segoe UI", 9f, FontStyle.Regular);
    public static readonly Font FontSmall = new("Segoe UI", 8f, FontStyle.Regular);
    public static readonly Font FontEmoji = new("Segoe UI Emoji", 28f);

    public static void SetTheme(string? key)
    {
        _activeTheme = _themes.FirstOrDefault(t => t.Key == key) ?? _themes[0];
    }

    public static void StyleButton(Button btn, bool outlined = false)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Primary;
        btn.FlatAppearance.BorderSize = outlined ? 2 : 0;
        btn.BackColor = outlined ? Surface : Primary;
        btn.ForeColor = outlined ? Primary : ButtonText;
        btn.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        btn.Cursor = Cursors.Hand;
        btn.Height = 34;

        btn.MouseEnter += (s, e) =>
        {
            btn.BackColor = outlined ? Secondary : PrimaryHover;
        };
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = outlined ? Surface : Primary;
        };
    }

    public static void StyleCard(Panel panel)
    {
        panel.BackColor = Surface;
        panel.Padding = new Padding(12);
    }

    public static void StyleProgressBar(ProgressBar pb, Color fillColor)
    {
        pb.Style = ProgressBarStyle.Continuous;
        pb.Tag = fillColor;
        pb.Height = 14;
        pb.Minimum = 0;
        pb.Maximum = 100;
    }
}
