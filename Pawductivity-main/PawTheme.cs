namespace Pawductivity;

/// Centralized pink kawaii theme for the entire app.
/// Change colors here and they update everywhere!
public static class PawTheme
{
    // ── Colors ──────────────────────────────────────────────────────
    public static readonly Color Background      = Color.FromArgb(255, 240, 245);  // soft blush
    public static readonly Color Surface         = Color.FromArgb(255, 220, 230);  // light pink card
    public static readonly Color Primary         = Color.FromArgb(255, 105, 150);  // rose pink
    public static readonly Color PrimaryHover    = Color.FromArgb(240,  85, 130);
    public static readonly Color Secondary       = Color.FromArgb(255, 182, 193);  // pastel pink
    public static readonly Color Accent          = Color.FromArgb(255, 150, 200);  // bubblegum
    public static readonly Color TextDark        = Color.FromArgb( 80,  30,  50);  // deep rose-brown
    public static readonly Color TextMuted       = Color.FromArgb(160,  90, 120);
    public static readonly Color HealthBar       = Color.FromArgb(255,  80, 120);
    public static readonly Color MoodBar         = Color.FromArgb(255, 200,  80);  // sunny yellow
    public static readonly Color XpBar           = Color.FromArgb(140, 200, 255);  // periwinkle
    public static readonly Color ButtonText      = Color.White;
    public static readonly Color CompletedTask   = Color.FromArgb(150, 220, 150);  // soft green
    public static readonly Color TextGreen       = Color.FromArgb(0, 100, 0);  // dark green
    public static readonly Color OverdueTask     = Color.FromArgb(255,  77,  77);  // soft red
    public static readonly Color CardBorder      = Color.FromArgb(255, 160, 190);

    // ── Fonts ────────────────────────────────────────────────────────
    public static readonly Font FontTitle   = new("Segoe UI", 22f, FontStyle.Bold);
    public static readonly Font FontHeading = new("Segoe UI", 13f, FontStyle.Bold);
    public static readonly Font FontBody    = new("Segoe UI",  9f, FontStyle.Regular);
    public static readonly Font FontSmall   = new("Segoe UI",  8f, FontStyle.Regular);
    public static readonly Font FontEmoji   = new("Segoe UI Emoji", 28f);

    // ── Helpers ──────────────────────────────────────────────────────
    /// Apply cute pink styling to a Button.</summary>
    public static void StyleButton(Button btn, bool outlined = false)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Primary;
        btn.FlatAppearance.BorderSize  = outlined ? 2 : 0;
        btn.BackColor  = outlined ? Surface : Primary;
        btn.ForeColor  = outlined ? Primary : ButtonText;
        btn.Font       = new Font("Segoe UI", 9f, FontStyle.Bold);
        btn.Cursor     = Cursors.Hand;
        btn.Height     = 34;

        btn.MouseEnter += (s, e) => {
            btn.BackColor = outlined ? Secondary : PrimaryHover;
        };
        btn.MouseLeave += (s, e) => {
            btn.BackColor = outlined ? Surface : Primary;
        };
    }

    /// Apply soft-pink styling to a Panel used as a card.</summary>
    public static void StyleCard(Panel panel)
    {
        panel.BackColor = Surface;
        panel.Padding   = new Padding(12);
    }

    /// Apply pink styling to a ProgressBar.</summary>
    public static void StyleProgressBar(ProgressBar pb, Color fillColor)
    {
        pb.Style = ProgressBarStyle.Continuous;
        // Custom draw is done in the form; just store the color as Tag
        pb.Tag     = fillColor;
        pb.Height  = 14;
        pb.Minimum = 0;
        pb.Maximum = 100;
    }
}
