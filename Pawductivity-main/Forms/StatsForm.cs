using Pawductivity.Managers;

namespace Pawductivity.Forms;

public class StatsForm : Form
{
    private readonly GameManager _gm;

    public StatsForm(GameManager gm)
    {
        _gm = gm;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text            = "📊 Productivity Stats";
        Size            = new Size(460, 480);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = PawTheme.Background;

        var title = new Label
        {
            Text      = "📊 Your Stats",
            Font      = PawTheme.FontTitle,
            ForeColor = PawTheme.Primary,
            AutoSize  = true,
            Location  = new Point(20, 15),
            BackColor = Color.Transparent,
        };
        Controls.Add(title);

        int y = 80;
        var stats = new (string Label, string Value)[]
        {
            ("Total tasks completed",       $"{_gm.TotalCompleted}"),
            ("Tasks completed today",       $"{_gm.CompletedToday}"),
            ("Pending tasks",               $"{_gm.PendingCount}"),
            ("Overall completion rate",     $"{_gm.CompletionRate:F1}%"),
            ("Current streak 🔥",           $"{_gm.CurrentStreak} day(s)"),
            ("Longest streak 🏆",           $"{_gm.LongestStreak} day(s)"),
            ("Pet level",                   $"Lv.{_gm.Pet.Level} — {_gm.Pet.Stage}"),
            ("Pet mood",                    $"{_gm.Pet.CurrentMood} {_gm.Pet.MoodEmoji}"),
            ("Coins earned",                $"🪙 {_gm.Pet.Coins}"),
        };

        foreach (var (label, value) in stats)
        {
            var row = new Panel
            {
                Location  = new Point(20, y),
                Size      = new Size(400, 36),
                BackColor = y % 72 == 8 ? PawTheme.Surface : PawTheme.Background,
            };
            row.Controls.Add(new Label
            {
                Text      = label,
                Font      = PawTheme.FontBody,
                ForeColor = PawTheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(8, 8),
                BackColor = Color.Transparent,
            });
            row.Controls.Add(new Label
            {
                Text      = value,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = PawTheme.Primary,
                AutoSize  = true,
                Location  = new Point(260, 8),
                BackColor = Color.Transparent,
            });
            Controls.Add(row);
            y += 38;
        }

        // Progress bar for completion rate
        y += 10;
        Controls.Add(new Label
        {
            Text      = "Overall Completion",
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = PawTheme.TextMuted,
            AutoSize  = true,
            Location  = new Point(20, y),
            BackColor = Color.Transparent,
        });
        y += 18;
        var pb = new ProgressBar
        {
            Location = new Point(20, y),
            Size     = new Size(400, 16),
            Minimum  = 0,
            Maximum  = 100,
            Value    = (int)_gm.CompletionRate,
            Style    = ProgressBarStyle.Continuous,
        };
        Controls.Add(pb);
    }
}
