using Pawductivity.Models;

namespace Pawductivity.Forms;

public class SettingsForm : Form
{
    private readonly Action _onThemeChanged;

    public SettingsForm(Action onThemeChanged)
    {
        _onThemeChanged = onThemeChanged;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Settings";
        Size = new Size(430, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = PawTheme.Background;
        Font = PawTheme.FontBody;

        var title = new Label
        {
            Text = "⚙Settings",
            Font = PawTheme.FontTitle,
            ForeColor = PawTheme.Primary,
            AutoSize = true,
            Location = new Point(10, 18),
            BackColor = Color.Transparent,
        };

        var subtitle = new Label
        {
            Text = "Choose a dashboard theme",
            Font = PawTheme.FontBody,
            ForeColor = PawTheme.TextMuted,
            AutoSize = true,
            Location = new Point(24, 75),
            BackColor = Color.Transparent,
        };

        Controls.AddRange([title, subtitle]);

        int y = 104;
        foreach (var theme in PawTheme.Themes)
        {
            Controls.Add(MakeThemeButton(theme, y));
            y += 48;
        }

        var close = new Button
        {
            Text = "Close",
            Location = new Point(240, y + 8),
            Width = 142,
        };
        PawTheme.StyleButton(close, outlined: true);
        close.Click += (s, e) => Close();
        Controls.Add(close);
    }

    private Button MakeThemeButton(AppTheme theme, int y)
    {
        var button = new Button
        {
            Text = theme.Key == PawTheme.ActiveThemeKey
                ? $"✓ {theme.DisplayName}"
                : theme.DisplayName,
            Location = new Point(24, y),
            Size = new Size(358, 38),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(14, 0, 0, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            BackColor = theme.Surface,
            ForeColor = theme.Primary,
        };
        button.FlatAppearance.BorderColor = theme.Primary;
        button.FlatAppearance.BorderSize = theme.Key == PawTheme.ActiveThemeKey ? 3 : 1;

        button.Click += (s, e) =>
        {
            PawTheme.SetTheme(theme.Key);
            _onThemeChanged();
            Controls.Clear();
            InitializeComponent();
        };

        return button;
    }
}
