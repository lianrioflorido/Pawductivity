using Pawductivity.Managers;
using Pawductivity.Models;
using Pawductivity.Persistence;

namespace Pawductivity.Forms;

public class DashboardForm : Form
{
    private readonly GameManager _gm;

    // Pet panel widgets
    private PictureBox _petCanvas = null!;
    private Label _lblPetName = null!;
    private Label _lblGreeting = null!;
    private Label _lblLevel = null!;
    private ProgressBar _pbHealth = null!;
    private ProgressBar _pbMood = null!;
    private ProgressBar _pbXp = null!;
    private Label _lblCoins = null!;

    // Task panel widgets
    private ListView _lvTasks = null!;
    private Button _btnAddTask = null!;
    private Button _btnComplete = null!;
    private Button _btnDelete = null!;
    private Button _btnEdit = null!;

    // Nav buttons
    private Button _btnShop = null!;
    private Button _btnStats = null!;
    private Button _btnLogout = null!;

    // Stats labels
    private Label _lblToday = null!;
    private Label _lblStreak = null!;
    private Label _lblPending = null!;

    private System.Windows.Forms.Timer _decayTimer = null!;

    // ── Pet animation ─────────────────────────────────────────────────
    private System.Windows.Forms.Timer _petAnimTimer = null!;
    private int _petFrame = 0;
    private int _petBounceOffset = 0;
    private int _petBounceStep = 0;
    private PetAnimState _petState = PetAnimState.Idle;

    // Blink — tracked separately so it's slow and visible
    private int _blinkTick = 0;       // counts up each anim tick
    private bool _isBlinking = false; // true for a few ticks = closed eyes

    private enum PetAnimState { Idle, Happy, Sad, Bounce }

    // ── Speech bubble ─────────────────────────────────────────────────
    private System.Windows.Forms.Timer _bubbleTimer = null!;
    private string _bubbleText = "";
    private int _bubbleVisible = 0;

    // ── Layout constants ─────────────────────────────────────────────
    private new int Margin = 16;
    private const int InnerPad = 14;
    private const int TopBarH = 52;
    private const int PetPanelW = 288;
    private const int ButtonH = 34;
    private const int NavButtonW = 126;
    private const int ToolbarButtonW = 126;
    private const int StatBarH = 14;
    private const int StatBarLblGap = 4;

    public DashboardForm(GameManager gm)
    {
        _gm = gm;
        InitializeComponent();
        RefreshAll();
        StartDecayTimer();
        StartPetAnimation();
        StartBubbleTimer();

        // ── Save on exit ─────────────────────────────────────────────
        FormClosed += OnFormClosed;
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        SaveManager.Save(_gm);
        Application.Exit();
    }

    private void InitializeComponent()
    {
        Text = "Pawductivity 🐾 — Dashboard";
        MinimumSize = new Size(970, 840);
        ClientSize  = new Size(970, 840);
        Size = new Size(930, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = PawTheme.Background;
        Font = PawTheme.FontBody;

        // ── TOP BAR ──────────────────────────────────────────────────
        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = TopBarH,
            BackColor = PawTheme.Primary,
        };

        var lblApp = new Label
        {
            Text = "🐾 Pawductivity",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(Margin, (TopBarH - 28) / 2),
        };

        var lblUser = new Label
        {
            Text = $"Hi, {_gm.UserName}! 💕",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        topBar.Controls.AddRange([lblApp, lblUser]);
        lblUser.Location = new Point(topBar.Width - 200, (TopBarH - 18) / 2);

        // ── LEFT: PET PANEL ──────────────────────────────────────────
        int panelTop = TopBarH + Margin;
        int panelBottom = ClientSize.Height - Margin;
        int petPanelH = panelBottom - panelTop;

        var petPanel = new Panel
        {
            Location = new Point(Margin, panelTop),
            Size = new Size(PetPanelW, petPanelH),
            BackColor = PawTheme.Surface,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
        };
        petPanel.Paint += (s, e) => PaintBorder(e, petPanel);

        _petCanvas = new PictureBox
        {
            AutoSize = false,
            Size = new Size(PetPanelW - InnerPad * 2, 180),
            Location = new Point(InnerPad, InnerPad + 4),
            BackColor = Color.Transparent,
        };
        _petCanvas.Paint += PetCanvas_Paint;

        _lblPetName = new Label
        {
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = PawTheme.Primary,
            AutoSize = false,
            Size = new Size(PetPanelW - InnerPad * 2, 26),
            Location = new Point(InnerPad, _petCanvas.Bottom + 8),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };

        _lblGreeting = new Label
        {
            Font = PawTheme.FontSmall,
            ForeColor = PawTheme.TextMuted,
            AutoSize = false,
            Size = new Size(PetPanelW - InnerPad * 2, 36),
            Location = new Point(InnerPad, _lblPetName.Bottom + 4),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };

        _lblLevel = new Label
        {
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = PawTheme.TextDark,
            AutoSize = true,
            Location = new Point(InnerPad, _lblGreeting.Bottom + 10),
            BackColor = Color.Transparent,
        };

        int barSlotH = 10 + StatBarLblGap + StatBarH + 8;
        int firstBarY = _lblLevel.Location.Y + 20 + 10;

        var (lblH, _pbHealthOut) = MakeStatBar("❤️ Health", firstBarY, PawTheme.HealthBar);
        var (lblM, _pbMoodOut) = MakeStatBar("😸 Mood", firstBarY + barSlotH, PawTheme.MoodBar);
        var (lblX, _pbXpOut) = MakeStatBar("⭐ XP", firstBarY + barSlotH * 2, PawTheme.XpBar);

        _pbHealth = _pbHealthOut;
        _pbMood = _pbMoodOut;
        _pbXp = _pbXpOut;

        _lblCoins = new Label
        {
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = PawTheme.Primary,
            AutoSize = true,
            Location = new Point(InnerPad, _pbXp.Bottom + 12),
            BackColor = Color.Transparent,
        };

        var statPanel = new Panel
        {
            Location = new Point(InnerPad, _lblCoins.Location.Y + 24 + 8),
            Size = new Size(PetPanelW - InnerPad * 2, 90),
            BackColor = PawTheme.Background,
        };

        _lblToday = MakeStatLabel("Tasks today: 0", new Point(6, 6));
        _lblStreak = MakeStatLabel("🔥 Streak: 0 days", new Point(6, 34));
        _lblPending = MakeStatLabel("📋 Pending: 0", new Point(6, 62));
        statPanel.Controls.AddRange([_lblToday, _lblStreak, _lblPending]);

        int navBtnY = statPanel.Bottom + 12;
        _btnShop = new Button { Text = "🛍 Shop", Location = new Point(InnerPad, navBtnY), Width = NavButtonW, Height = ButtonH };
        _btnStats = new Button { Text = "📊 Stats", Location = new Point(InnerPad + NavButtonW + 8, navBtnY), Width = NavButtonW, Height = ButtonH };
        _btnLogout = new Button { Text = "🚪 Logout", Location = new Point(InnerPad, navBtnY + ButtonH + 8), Width = PetPanelW - InnerPad * 2, Height = ButtonH };

        PawTheme.StyleButton(_btnShop, outlined: true);
        PawTheme.StyleButton(_btnStats, outlined: true);
        PawTheme.StyleButton(_btnLogout, outlined: true);
        _btnLogout.ForeColor = Color.FromArgb(180, 60, 60);
        _btnLogout.FlatAppearance.BorderColor = Color.FromArgb(180, 60, 60);
        _btnShop.Click += (s, e) => new ShopForm(_gm, RefreshAll).ShowDialog(this);
        _btnStats.Click += (s, e) => new StatsForm(_gm).ShowDialog(this);
        _btnLogout.Click += BtnLogout_Click;

        petPanel.Controls.AddRange([
            _petCanvas, _lblPetName, _lblGreeting, _lblLevel,
            lblH, _pbHealth, lblM, _pbMood, lblX, _pbXp,
            _lblCoins, statPanel, _btnShop, _btnStats, _btnLogout,
        ]);

        // ── RIGHT: TASK PANEL ────────────────────────────────────────
        int taskPanelX = Margin + PetPanelW + Margin;
        int taskPanelW = Width - taskPanelX - Margin - 16;

        var taskPanel = new Panel
        {
            Location = new Point(taskPanelX, panelTop),
            Size = new Size(taskPanelW, petPanelH),
            BackColor = PawTheme.Surface,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
        };
        taskPanel.Paint += (s, e) => PaintBorder(e, taskPanel);

        var lblTaskTitle = new Label
        {
            Text = "📋 My Tasks",
            Font = PawTheme.FontHeading,
            ForeColor = PawTheme.Primary,
            AutoSize = true,
            Location = new Point(InnerPad, InnerPad),
            BackColor = Color.Transparent,
        };

        int listTop = lblTaskTitle.Bottom + 10;
        int listH = petPanelH - listTop - ButtonH - InnerPad * 2 - 4;

        _lvTasks = new ListView
        {
            Location = new Point(InnerPad, listTop),
            Size = new Size(taskPanelW - InnerPad * 2, listH),
            View = View.Details,
            FullRowSelect = true,
            GridLines = false,
            HotTracking = true,
            BackColor = PawTheme.Background,
            ForeColor = PawTheme.TextDark,
            Font = PawTheme.FontBody,
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Left |
                            AnchorStyles.Bottom | AnchorStyles.Right,
        };

        _lvTasks.Columns.Add("", 30);
        _lvTasks.Columns.Add("Task", 220);
        _lvTasks.Columns.Add("Priority", 96);
        _lvTasks.Columns.Add("Due Date", 96);
        _lvTasks.Columns.Add("Status", 96);

        _lvTasks.OwnerDraw = true;
        _lvTasks.ColumnWidthChanging += (s, e) =>
        {
            e.Cancel = true;
            e.NewWidth = _lvTasks.Columns[e.ColumnIndex].Width;
        };
        _lvTasks.Resize += (s, e) =>
        {
            int used = _lvTasks.Columns[0].Width + _lvTasks.Columns[2].Width
                     + _lvTasks.Columns[3].Width + _lvTasks.Columns[4].Width;
            _lvTasks.Columns[1].Width = _lvTasks.ClientSize.Width - used;
        };
        _lvTasks.DrawColumnHeader += (s, e) =>
        {
            using var bg = new SolidBrush(PawTheme.Secondary);
            e.Graphics.FillRectangle(bg, e.Bounds);
            using var pen = new Pen(Color.FromArgb(40, PawTheme.TextDark));
            e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1,
                                     e.Bounds.Right, e.Bounds.Bottom - 1);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, _lvTasks.Font, e.Bounds,
                                  PawTheme.TextDark,
                                  TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        };
        _lvTasks.DrawItem += LvTasks_DrawItem;
        _lvTasks.DrawSubItem += LvTasks_DrawSubItem;

        int toolY = petPanelH - InnerPad - ButtonH;
        int toolGap = 10;

        _btnAddTask = new Button { Text = "+ Add Task", Width = ToolbarButtonW, Height = ButtonH, Location = new Point(InnerPad, toolY) };
        _btnComplete = new Button { Text = "✔ Complete", Width = ToolbarButtonW, Height = ButtonH, Location = new Point(InnerPad + (ToolbarButtonW + toolGap), toolY) };
        _btnEdit = new Button { Text = "✏️ Edit", Width = ToolbarButtonW, Height = ButtonH, Location = new Point(InnerPad + (ToolbarButtonW + toolGap) * 2, toolY) };
        _btnDelete = new Button { Text = "🗑 Delete", Width = ToolbarButtonW, Height = ButtonH, Location = new Point(InnerPad + (ToolbarButtonW + toolGap) * 3, toolY) };

        foreach (var btn in new[] { _btnAddTask, _btnComplete, _btnEdit, _btnDelete })
            btn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

        PawTheme.StyleButton(_btnAddTask);
        PawTheme.StyleButton(_btnComplete);
        PawTheme.StyleButton(_btnEdit, outlined: true);
        PawTheme.StyleButton(_btnDelete, outlined: true);

        _btnAddTask.Click += BtnAdd_Click;
        _btnComplete.Click += BtnComplete_Click;
        _btnEdit.Click += BtnEdit_Click;
        _btnDelete.Click += BtnDelete_Click;

        taskPanel.Controls.AddRange([
            lblTaskTitle, _lvTasks,
            _btnAddTask, _btnComplete, _btnEdit, _btnDelete,
        ]);

        Controls.AddRange([topBar, petPanel, taskPanel]);
    }

    // ── CUSTOM LISTVIEW DRAW ─────────────────────────────────────────
    private void LvTasks_DrawItem(object? sender, DrawListViewItemEventArgs e)
    {
        e.DrawDefault = false;
        if (e.Item?.Tag is not TaskItem task) return;

        bool sel  = (e.State & ListViewItemStates.Selected) != 0;
        bool hot  = (e.State & ListViewItemStates.Hot)      != 0;

        Color bg = sel  ? PawTheme.Secondary :
                   hot  ? Color.FromArgb(255, 235, 245) :   // soft hover tint
                   task.IsCompleted ? PawTheme.CompletedTask :
                   task.IsOverdue   ? PawTheme.OverdueTask :
                                      _lvTasks.BackColor;

        using var brush = new SolidBrush(bg);
        e.Graphics.FillRectangle(brush, e.Bounds);
    }

    private void LvTasks_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
    {
        if (e.Item?.Tag is not TaskItem task) return;

        bool sel = (e.ItemState & ListViewItemStates.Selected) != 0;
        bool hot = (e.ItemState & ListViewItemStates.Hot)      != 0;

        // Always use a visible foreground regardless of hover/selection state
        Color fg = sel                ? PawTheme.TextDark :
                   task.IsCompleted  ? PawTheme.TextGreen :
                                       PawTheme.TextDark;

        var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
        TextRenderer.DrawText(e.Graphics, e.SubItem!.Text, _lvTasks.Font, e.Bounds, fg, flags);
    }

    // ── BUTTON HANDLERS ──────────────────────────────────────────────
    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        using var dlg = new TaskEditForm();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _gm.AddTask(dlg.Result!);
            RefreshAll();
        }
    }

    private void BtnComplete_Click(object? sender, EventArgs e)
    {
        if (_lvTasks.SelectedItems.Count == 0) return;
        var selected = _lvTasks.SelectedItems[0];
        if (selected?.Tag is not TaskItem task) { ShowInfo("No task selected."); return; }
        if (task.IsCompleted) { ShowInfo("This task is already done! 🎉"); return; }

        _gm.CompleteTask(task.Id);
        int coins = task.Priority switch
        {
            TaskPriority.High => 15,
            TaskPriority.Medium => 10,
            _ => 5,
        };
        ShowInfo($"{_gm.Pet.GetGreeting()}\n\n+XP gained! 🌟 Coins earned: {coins} 🪙");
        RefreshAll();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (_lvTasks.SelectedItems.Count == 0) return;
        var task = (TaskItem)_lvTasks.SelectedItems[0].Tag!;
        using var dlg = new TaskEditForm(task);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _gm.EditTask(task.Id, dlg.Result!.Title, dlg.Result.Description,
                         dlg.Result.Priority, dlg.Result.DueDate);
            RefreshAll();
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_lvTasks.SelectedItems.Count == 0) return;
        var task = (TaskItem)_lvTasks.SelectedItems[0].Tag!;
        if (MessageBox.Show($"Delete \"{task.Title}\"?", "Confirm",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _gm.DeleteTask(task.Id);
            RefreshAll();
        }
    }

    // ── REFRESH ──────────────────────────────────────────────────────
    public void RefreshAll()
    {
        var pet = _gm.Pet;
        _petCanvas.Invalidate();   // trigger repaint with current mood/stage
        _lblPetName.Text = pet.Name;
        _lblGreeting.Text = pet.GetGreeting();
        _lblLevel.Text = $"Lv.{pet.Level} • Stage: {pet.Stage}";
        _lblCoins.Text = $"🪙 Coins: {pet.Coins}";
        _pbHealth.Value = pet.Health;
        _pbMood.Value = pet.Mood;
        _pbXp.Value = Math.Min(100, (int)((double)pet.XP / pet.XpForNextLevel * 100));

        _lblToday.Text = $"✅ Completed today: {_gm.CompletedToday}";
        _lblStreak.Text = $"🔥 Streak: {_gm.CurrentStreak} day(s)";
        _lblPending.Text = $"📋 Pending: {_gm.PendingCount}";

        _lvTasks.Items.Clear();
        foreach (var t in _gm.Tasks.OrderBy(t => t.IsCompleted).ThenBy(t => t.DueDate))
        {
            var item = new ListViewItem(t.IsCompleted ? "✅" : t.IsOverdue ? "⚠️" : "⬜");
            item.SubItems.Add(t.Title);
            item.SubItems.Add($"{t.PriorityEmoji} {t.Priority}");
            item.SubItems.Add(t.DueDate.ToString("MMM dd"));
            item.SubItems.Add(t.IsCompleted ? "Done 🎉" : t.IsOverdue ? "Overdue!" : "Pending");
            item.Tag = t;
            _lvTasks.Items.Add(item);
        }
    }

    // ── DECAY TIMER ──────────────────────────────────────────────────
    private void StartDecayTimer()
    {
        _decayTimer = new System.Windows.Forms.Timer { Interval = 60_000 };
        _decayTimer.Tick += (s, e) => { _gm.ApplyOverduePenalties(); RefreshAll(); };
        _decayTimer.Start();
    }

    // ── LOGOUT ───────────────────────────────────────────────────────
    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Save and return to the login screen?",
            "Log out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        SaveManager.Save(_gm);
        _decayTimer.Stop();
        _petAnimTimer?.Stop();
        _bubbleTimer?.Stop();

        // Re-show the LoginForm that was hidden behind us
        foreach (Form f in Application.OpenForms)
        {
            if (f is LoginForm login)
            {
                login.LoadProfiles();
                login.Show();
                break;
            }
        }

        // Close just the dashboard (not the whole app)
        FormClosed -= OnFormClosed;   // prevent double-save / Application.Exit from the close handler
        Close();
    }

    // ── PET ANIMATION ────────────────────────────────────────────────
    private void StartPetAnimation()
    {
        _petAnimTimer = new System.Windows.Forms.Timer { Interval = 120 };
        _petAnimTimer.Tick += PetAnimTick;
        _petAnimTimer.Start();
    }

    private void PetAnimTick(object? sender, EventArgs e)
    {
        _petFrame = (_petFrame + 1) % 4;

        // Bounce
        int[] sineOffsets = [0, -2, -4, -6, -7, -6, -4, -2];
        _petBounceStep = (_petBounceStep + 1) % sineOffsets.Length;
        _petBounceOffset = sineOffsets[_petBounceStep];

        // Mood state
        var mood = _gm.Pet.CurrentMood;
        _petState = mood switch
        {
            PetMood.Happy => PetAnimState.Happy,
            PetMood.Sad   => PetAnimState.Sad,
            PetMood.Sick  => PetAnimState.Sad,
            _             => PetAnimState.Idle,
        };

        // Blink — every ~4 seconds (120ms × 33 ticks), stays closed for 3 ticks (~360ms)
        _blinkTick++;
        if (_blinkTick >= 33)
        {
            _isBlinking = true;
            _blinkTick = 0;
        }
        else if (_isBlinking && _blinkTick >= 3)
        {
            _isBlinking = false;
        }

        // Bubble countdown — 18 ticks × 120ms = ~2 seconds visible
        if (_bubbleVisible > 0) _bubbleVisible--;

        _petCanvas.Invalidate();
    }

    // ── PET CANVAS PAINT ─────────────────────────────────────────────
    private void PetCanvas_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int cx = _petCanvas.Width / 2;
        int cy = 100 + _petBounceOffset;

        bool isCat    = _gm.Pet is CatPet;
        bool isHappy  = _petState == PetAnimState.Happy;
        bool isSad    = _petState == PetAnimState.Sad;
        var  stage    = _gm.Pet.Stage;
        bool blinking = _isBlinking;

        if (isCat) DrawCat(g, cx, cy, isHappy, isSad, _petFrame, stage, blinking);
        else        DrawDog(g, cx, cy, isHappy, isSad, _petFrame, stage, blinking);

        // ── Speech bubble ────────────────────────────────────────────
        if (_bubbleVisible > 0 && !string.IsNullOrEmpty(_bubbleText))
            DrawSpeechBubble(g, cx + 30, cy - 70, _bubbleText);
    }

    // ── SPEECH BUBBLE ─────────────────────────────────────────────────
    private static void DrawSpeechBubble(Graphics g, int x, int y, string text)
    {
        using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
        var size = TextRenderer.MeasureText(text, font);
        int pad = 8;
        int bw  = size.Width  + pad * 2;
        int bh  = size.Height + pad * 2;

        if (x - bw / 2 < 2) x = bw / 2 + 2;

        var rect = new Rectangle(x - bw / 2, y - bh, bw, bh);

        // Shadow
        using var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
        using var shadowPath  = MakeRoundedRect(rect.X + 2, rect.Y + 2, rect.Width, rect.Height, 10);
        g.FillPath(shadowBrush, shadowPath);

        // Bubble fill + border
        using var fillBrush  = new SolidBrush(Color.White);
        using var borderPen  = new Pen(Color.FromArgb(200, 150, 180), 1.5f);
        using var bubblePath = MakeRoundedRect(rect.X, rect.Y, rect.Width, rect.Height, 10);
        g.FillPath(fillBrush,  bubblePath);
        g.DrawPath(borderPen,  bubblePath);

        // Tail pointing down toward the pet
        var tail = new Point[] { new(x - 6, rect.Bottom), new(x - 18, rect.Bottom + 10), new(x + 4, rect.Bottom) };
        using var tailFill = new SolidBrush(Color.White);
        using var tailPen  = new Pen(Color.FromArgb(200, 150, 180), 1.5f);
        g.FillPolygon(tailFill, tail);
        g.DrawLine(tailPen, tail[0], tail[1]);
        g.DrawLine(tailPen, tail[1], tail[2]);

        TextRenderer.DrawText(g, text, font, new Point(rect.X + pad, rect.Y + pad), Color.FromArgb(80, 30, 60));
    }

    private static System.Drawing.Drawing2D.GraphicsPath MakeRoundedRect(int x, int y, int w, int h, int r)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(x,           y,           r*2, r*2, 180, 90);
        path.AddArc(x + w - r*2, y,           r*2, r*2, 270, 90);
        path.AddArc(x + w - r*2, y + h - r*2, r*2, r*2,   0, 90);
        path.AddArc(x,           y + h - r*2, r*2, r*2,  90, 90);
        path.CloseFigure();
        return path;
    }

    // ── BUBBLE TIMER ─────────────────────────────────────────────────
    private static readonly string[] CatHappy  = ["Purrrr~ 💜", "Meow! ✨", "Mrrrow~", "*purring*", "Nyaa~ 🎵", "Purrfect! 💜"];
    private static readonly string[] CatIdle   = ["Meow?", "...zzzz", "*yawns*", "Mrrp?", "*stares*", "Mew~"];
    private static readonly string[] CatSad    = ["Mrrrow... 😢", "*whimpers*", "Mew... 💧", "Nyo... 😿"];
    private static readonly string[] DogHappy  = ["Woof! 🐾", "WOOF WOOF!!", "*tail wags*", "Bork bork!", "Arf arf! 💛", "Hewwo!! 🐕"];
    private static readonly string[] DogIdle   = ["Woof?", "*sniffs*", "Bork?", "*yawns*", "Arf?", "*tilts head*"];
    private static readonly string[] DogSad    = ["Awoo... 😢", "*whines*", "Woof... 💧", "Arf... 😞"];
    private static readonly Random   _rng      = new();

    private void StartBubbleTimer()
    {
        // Show first bubble after 3 seconds, then every 5 seconds check
        _bubbleTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        _bubbleTimer.Tick += BubbleTick;
        _bubbleTimer.Start();

        // Trigger first bubble after a short delay
        var firstBubble = new System.Windows.Forms.Timer { Interval = 3000 };
        firstBubble.Tick += (s, e) => { ((System.Windows.Forms.Timer)s!).Stop(); ShowBubble(); };
        firstBubble.Start();
    }

    private void ShowBubble()
    {
        bool isCat = _gm.Pet is CatPet;
        string[] pool = (_petState, isCat) switch
        {
            (PetAnimState.Happy, true)  => CatHappy,
            (PetAnimState.Sad,   true)  => CatSad,
            (_,                  true)  => CatIdle,
            (PetAnimState.Happy, false) => DogHappy,
            (PetAnimState.Sad,   false) => DogSad,
            _                           => DogIdle,
        };
        _bubbleText    = pool[_rng.Next(pool.Length)];
        _bubbleVisible = 18; // 18 × 120ms = ~2.2 seconds visible
        _petCanvas.Invalidate();
    }

    private void BubbleTick(object? sender, EventArgs e)
    {
        // 70% chance to show a new bubble each 5-second tick
        if (_rng.Next(10) < 7) ShowBubble();
    }

    // ═══════════════════════════════════════════════════════════════
    // CAT  —  5 evolution stages  (same cute style as dog)
    // ═══════════════════════════════════════════════════════════════
    private static void DrawCat(Graphics g, int cx, int cy,
                                 bool happy, bool sad, int frame, PetEvolution stage, bool blinking)
    {
        switch (stage)
        {
            case PetEvolution.Egg:    DrawCatEgg(g, cx, cy, frame);                           break;
            case PetEvolution.Baby:   DrawCatBaby(g, cx, cy, happy, sad, frame, blinking);    break;
            case PetEvolution.Junior: DrawCatJunior(g, cx, cy, happy, sad, frame, blinking);  break;
            case PetEvolution.Adult:  DrawCatAdult(g, cx, cy, happy, sad, frame, blinking);   break;
            case PetEvolution.Legend: DrawCatLegend(g, cx, cy, happy, sad, frame, blinking);  break;
        }
    }

    // ── Egg ──────────────────────────────────────────────────────────
    private static void DrawCatEgg(Graphics g, int cx, int cy, int frame)
    {
        int w = frame % 2 == 0 ? -3 : 3;
        using var sh = new SolidBrush(Color.FromArgb(252, 248, 240));
        using var sp = new Pen(Color.FromArgb(180, 160, 120), 2f);
        g.FillEllipse(sh, cx - 22 + w, cy - 38, 44, 56);
        g.DrawEllipse(sp, cx - 22 + w, cy - 38, 44, 56);
        if (frame >= 2)
        {
            using var cp = new Pen(Color.FromArgb(160, 140, 100), 1.5f);
            g.DrawLine(cp, cx + w, cy - 10, cx + 6 + w, cy - 22);
            g.DrawLine(cp, cx + 6 + w, cy - 22, cx + 2 + w, cy - 30);
        }
        if (frame == 3)
        {
            using var eb = new SolidBrush(Color.FromArgb(60, 40, 20));
            g.FillEllipse(eb, cx - 9 + w, cy - 20, 5, 5);
            g.FillEllipse(eb, cx + 4 + w, cy - 20, 5, 5);
        }
        using var zf = new Font("Segoe UI", 8f, FontStyle.Bold);
        using var zb = new SolidBrush(Color.FromArgb(frame % 2 == 0 ? 200 : 60, 140, 100, 180));
        g.DrawString("z", zf, zb, cx + 20 + w, cy - 44);
        g.DrawString("Z", zf, zb, cx + 28 + w, cy - 54);
    }

    // ── Baby cat — chubby kitten, big head, tiny ears ─────────────────
    private static void DrawCatBaby(Graphics g, int cx, int cy, bool happy, bool sad, int frame, bool blinking)
    {
        var fur  = Color.FromArgb(255, 220, 170);  // warm cream-gold
        var ear  = Color.FromArgb(210, 175, 120);  // darker ear

        // Stubby wagging tail
        int tw = happy ? (frame % 2 == 0 ? 12 : -4) : 4;
        using var tailPen = new Pen(fur, 5f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
        g.DrawLine(tailPen, cx + 16, cy + 18, cx + 26 + tw, cy + 8);

        // Chubby body
        using var bb = new SolidBrush(fur);
        g.FillEllipse(bb, cx - 20, cy + 4, 40, 30);
        using var bellyb = new SolidBrush(Color.FromArgb(255, 240, 215));
        g.FillEllipse(bellyb, cx - 10, cy + 10, 20, 18);

        // Big round head — baby proportions like dog
        g.FillEllipse(bb, cx - 24, cy - 36, 48, 44);

        // Small triangular ears — sit on top of head
        using var earb = new SolidBrush(ear);
        Point[] el = [new(cx - 20, cy - 32), new(cx - 28, cy - 52), new(cx - 6, cy - 36)];
        Point[] er = [new(cx + 20, cy - 32), new(cx + 28, cy - 52), new(cx + 6, cy - 36)];
        g.FillPolygon(earb, el); g.FillPolygon(earb, er);
        using var innerb = new SolidBrush(Color.FromArgb(255, 185, 170));
        Point[] il = [new(cx - 19, cy - 34), new(cx - 24, cy - 48), new(cx - 8, cy - 36)];
        Point[] ir = [new(cx + 19, cy - 34), new(cx + 24, cy - 48), new(cx + 8, cy - 36)];
        g.FillPolygon(innerb, il); g.FillPolygon(innerb, ir);

        // Small oval snout — same as dog but smaller + cat nose on top
        using var snoutb = new SolidBrush(Color.FromArgb(255, 235, 210));
        g.FillEllipse(snoutb, cx - 10, cy - 16, 20, 14);
        using var noseb = new SolidBrush(Color.FromArgb(210, 120, 120));
        g.FillPolygon(noseb, new Point[] { new(cx, cy - 15), new(cx - 4, cy - 10), new(cx + 4, cy - 10) });

        DrawFace(g, cx, cy - 20, happy, sad, blinking, 8, false);

        // Whiskers
        using var wp = new Pen(Color.FromArgb(160, 200, 190, 180), 1.2f);
        g.DrawLine(wp, cx - 24, cy - 12, cx - 6, cy - 11); g.DrawLine(wp, cx + 6, cy - 11, cx + 24, cy - 12);

        // Little paws
        g.FillEllipse(bb, cx - 24, cy + 28, 16, 10);
        g.FillEllipse(bb, cx + 8, cy + 28 - (happy && frame % 2 == 0 ? 4 : 0), 16, 10);

        if (happy) DrawSparkles(g, cx, cy, frame, Color.FromArgb(255, 220, 100), 3, small: true);
    }

    // ── Junior cat — growing up, longer tail, more whiskers ───────────
    private static void DrawCatJunior(Graphics g, int cx, int cy, bool happy, bool sad, int frame, bool blinking)
    {
        var fur  = Color.FromArgb(210, 175, 125);  // warm tan
        var ear  = Color.FromArgb(175, 138, 88);

        // Curving tail — sweeps side to side
        float wagAngle = happy ? (frame % 2 == 0 ? 120f : 150f) : 135f;
        using var tailPen = new Pen(fur, 7f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
        var tailBase = new PointF(cx - 24, cy + 16);
        var tailTip  = new PointF(
            tailBase.X + (float)(36 * Math.Cos(wagAngle * Math.PI / 180)),
            tailBase.Y - (float)(36 * Math.Sin(wagAngle * Math.PI / 180)));
        g.DrawLine(tailPen, tailBase, tailTip);

        // Body
        using var bb = new SolidBrush(fur);
        g.FillEllipse(bb, cx - 28, cy + 2, 56, 42);
        using var bellyb = new SolidBrush(Color.FromArgb(240, 215, 175));
        g.FillEllipse(bellyb, cx - 14, cy + 12, 28, 24);

        // Head
        g.FillEllipse(bb, cx - 28, cy - 42, 56, 50);

        // Triangular ears — perky, upright
        using var earb = new SolidBrush(ear);
        Point[] el = [new(cx - 22, cy - 38), new(cx - 34, cy - 62), new(cx - 6, cy - 44)];
        Point[] er = [new(cx + 22, cy - 38), new(cx + 34, cy - 62), new(cx + 6, cy - 44)];
        g.FillPolygon(earb, el); g.FillPolygon(earb, er);
        using var innerb = new SolidBrush(Color.FromArgb(255, 185, 165));
        Point[] il = [new(cx - 21, cy - 40), new(cx - 29, cy - 58), new(cx - 8, cy - 44)];
        Point[] ir = [new(cx + 21, cy - 40), new(cx + 29, cy - 58), new(cx + 8, cy - 44)];
        g.FillPolygon(innerb, il); g.FillPolygon(innerb, ir);

        // Snout oval + cat triangle nose
        using var snoutb = new SolidBrush(Color.FromArgb(255, 230, 205));
        g.FillEllipse(snoutb, cx - 13, cy - 18, 26, 18);
        using var noseb = new SolidBrush(Color.FromArgb(210, 120, 120));
        g.FillPolygon(noseb, new Point[] { new(cx, cy - 17), new(cx - 5, cy - 11), new(cx + 5, cy - 11) });
        using var noseShineb = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
        g.FillEllipse(noseShineb, cx - 2, cy - 17, 4, 3);

        DrawFace(g, cx, cy - 24, happy, sad, blinking, 10, false);

        // Tongue when happy — cats lick!
        if (happy && frame % 2 == 0)
        {
            using var tongueb = new SolidBrush(Color.FromArgb(255, 140, 150));
            g.FillEllipse(tongueb, cx - 5, cy - 4, 10, 8);
        }

        // Whiskers — two rows each side
        using var wp = new Pen(Color.FromArgb(160, 210, 200, 190), 1.3f);
        g.DrawLine(wp, cx - 30, cy - 12, cx - 8, cy - 11); g.DrawLine(wp, cx - 30, cy - 8, cx - 8, cy - 8);
        g.DrawLine(wp, cx + 8, cy - 11, cx + 30, cy - 12); g.DrawLine(wp, cx + 8, cy - 8, cx + 30, cy - 8);

        // Paws
        using var pawb = new SolidBrush(fur);
        int lpY = happy && frame % 2 == 0 ? cy + 36 : cy + 42;
        int rpY = happy && frame % 2 == 1 ? cy + 36 : cy + 42;
        g.FillEllipse(pawb, cx - 30, lpY, 22, 13); g.FillEllipse(pawb, cx + 8, rpY, 22, 13);

        if (happy)
        {
            using var blushb = new SolidBrush(Color.FromArgb(70, 255, 130, 100));
            g.FillEllipse(blushb, cx - 30, cy - 12, 16, 10);
            g.FillEllipse(blushb, cx + 14, cy - 12, 16, 10);
            DrawSparkles(g, cx, cy, frame, Color.FromArgb(255, 200, 80), 4, small: false);
        }
        if (sad) DrawTears(g, cx, cy, 2);
    }

    // ── Adult cat — full grown, tabby stripes, long sweeping tail ─────
    private static void DrawCatAdult(Graphics g, int cx, int cy, bool happy, bool sad, int frame, bool blinking)
    {
        var fur    = Color.FromArgb(220, 180, 130);  // warm golden tabby
        var stripe = Color.FromArgb(170, 132, 85);
        var ear    = Color.FromArgb(180, 140, 90);

        // Long sweeping tail — elegant curve
        float wagAngle = happy ? (frame % 2 == 0 ? 110f : 145f) : 128f;
        using var tailPen = new Pen(fur, 9f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
        var tailBase = new PointF(cx - 26, cy + 18);
        var tailTip  = new PointF(
            tailBase.X + (float)(42 * Math.Cos(wagAngle * Math.PI / 180)),
            tailBase.Y - (float)(42 * Math.Sin(wagAngle * Math.PI / 180)));
        g.DrawLine(tailPen, tailBase, tailTip);

        // Body with tabby stripes
        using var bb = new SolidBrush(fur);
        g.FillEllipse(bb, cx - 30, cy, 60, 46);
        using var bellyb = new SolidBrush(Color.FromArgb(255, 235, 205));
        g.FillEllipse(bellyb, cx - 16, cy + 12, 32, 26);
        using var stripePen = new Pen(stripe, 2f);
        g.DrawArc(stripePen, cx - 14, cy + 6, 12, 8, -10, 180);
        g.DrawArc(stripePen, cx + 2,  cy + 6, 12, 8, -10, 180);

        // Head — round like dog
        g.FillEllipse(bb, cx - 30, cy - 44, 60, 50);

        // Forehead stripes
        g.DrawLine(stripePen, cx - 5, cy - 42, cx - 5, cy - 30);
        g.DrawLine(stripePen, cx + 5, cy - 42, cx + 5, cy - 30);

        // Triangular ears
        using var earb = new SolidBrush(ear);
        Point[] el = [new(cx - 24, cy - 40), new(cx - 36, cy - 66), new(cx - 8, cy - 46)];
        Point[] er = [new(cx + 24, cy - 40), new(cx + 36, cy - 66), new(cx + 8, cy - 46)];
        g.FillPolygon(earb, el); g.FillPolygon(earb, er);
        using var innerb = new SolidBrush(Color.FromArgb(255, 185, 162));
        Point[] il = [new(cx - 22, cy - 42), new(cx - 31, cy - 62), new(cx - 10, cy - 46)];
        Point[] ir = [new(cx + 22, cy - 42), new(cx + 31, cy - 62), new(cx + 10, cy - 46)];
        g.FillPolygon(innerb, il); g.FillPolygon(innerb, ir);

        // Snout + cat nose (same as dog snout approach)
        using var snoutb = new SolidBrush(Color.FromArgb(255, 225, 195));
        g.FillEllipse(snoutb, cx - 14, cy - 18, 28, 20);
        using var noseb = new SolidBrush(Color.FromArgb(210, 110, 110));
        g.FillPolygon(noseb, new Point[] { new(cx, cy - 17), new(cx - 5, cy - 11), new(cx + 5, cy - 11) });
        using var noseShineb = new SolidBrush(Color.FromArgb(100, 255, 255, 255));
        g.FillEllipse(noseShineb, cx - 2, cy - 17, 5, 3);

        DrawFace(g, cx, cy - 24, happy, sad, blinking, 11, false);

        if (happy && frame % 2 == 0)
        {
            using var tongueb = new SolidBrush(Color.FromArgb(255, 140, 150));
            g.FillEllipse(tongueb, cx - 5, cy - 2, 10, 8);
        }

        // Three whiskers each side
        using var wp = new Pen(Color.FromArgb(170, 220, 210, 200), 1.4f);
        g.DrawLine(wp, cx - 32, cy - 14, cx - 9, cy - 13); g.DrawLine(wp, cx - 32, cy - 9, cx - 9, cy - 9);
        g.DrawLine(wp, cx - 30, cy - 18, cx - 9, cy - 14);
        g.DrawLine(wp, cx + 9, cy - 13, cx + 32, cy - 14); g.DrawLine(wp, cx + 9, cy - 9, cx + 32, cy - 9);
        g.DrawLine(wp, cx + 9, cy - 14, cx + 30, cy - 18);

        // Paws with toe beans
        using var pawb = new SolidBrush(fur);
        using var beansb = new SolidBrush(Color.FromArgb(215, 165, 135));
        int lpY = happy && frame % 2 == 0 ? cy + 34 : cy + 40;
        int rpY = happy && frame % 2 == 1 ? cy + 34 : cy + 40;
        g.FillEllipse(pawb, cx - 32, lpY, 24, 14); g.FillEllipse(pawb, cx + 8, rpY, 24, 14);
        foreach (int bx in new[] { cx - 28, cx - 22, cx - 16 }) g.FillEllipse(beansb, bx, lpY + 3, 5, 4);
        foreach (int bx in new[] { cx + 12, cx + 18, cx + 24 }) g.FillEllipse(beansb, bx, rpY + 3, 5, 4);

        if (happy)
        {
            using var blushb = new SolidBrush(Color.FromArgb(80, 255, 130, 100));
            g.FillEllipse(blushb, cx - 32, cy - 14, 16, 10);
            g.FillEllipse(blushb, cx + 16, cy - 14, 16, 10);
            DrawHearts(g, cx, cy, frame, 3);
        }
        if (sad) DrawTears(g, cx, cy, 2);
    }

    // ── Legend cat — silver-white with crown, magic aura ──────────────
    private static void DrawCatLegend(Graphics g, int cx, int cy, bool happy, bool sad, int frame, bool blinking)
    {
        // Purple/blue aura
        int alpha = frame % 2 == 0 ? 55 : 22;
        using var aura1 = new SolidBrush(Color.FromArgb(alpha, 140, 80, 255));
        using var aura2 = new SolidBrush(Color.FromArgb(alpha / 2, 80, 180, 255));
        g.FillEllipse(aura1, cx - 50, cy - 58, 100, 100);
        g.FillEllipse(aura2, cx - 60, cy - 68, 120, 120);

        // Crown
        using var crownb = new SolidBrush(Color.FromArgb(255, 200, 40));
        Point[] crown = [new(cx-18,cy-66),new(cx-18,cy-80),new(cx-9,cy-70),new(cx,cy-84),new(cx+9,cy-70),new(cx+18,cy-80),new(cx+18,cy-66)];
        g.FillPolygon(crownb, crown);
        using var j1 = new SolidBrush(Color.FromArgb(255, 70, 90));
        using var j2 = new SolidBrush(Color.FromArgb(70, 170, 255));
        g.FillEllipse(j1, cx - 4, cy - 82, 8, 8);
        g.FillEllipse(j2, cx - 16, cy - 78, 6, 6);
        g.FillEllipse(j2, cx + 10, cy - 78, 6, 6);

        // Silver-white adult cat as base — same structure, lighter palette
        var fur    = Color.FromArgb(238, 234, 228);
        var stripe = Color.FromArgb(205, 200, 192);
        var ear    = Color.FromArgb(200, 190, 178);

        float wagAngle = happy ? (frame % 2 == 0 ? 110f : 145f) : 128f;
        using var tailPen = new Pen(fur, 9f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
        var tailBase = new PointF(cx - 26, cy + 18);
        var tailTip  = new PointF(tailBase.X + (float)(42 * Math.Cos(wagAngle * Math.PI / 180)), tailBase.Y - (float)(42 * Math.Sin(wagAngle * Math.PI / 180)));
        g.DrawLine(tailPen, tailBase, tailTip);

        using var bb = new SolidBrush(fur);
        g.FillEllipse(bb, cx - 30, cy, 60, 46);
        using var bellyb = new SolidBrush(Color.FromArgb(252, 250, 247));
        g.FillEllipse(bellyb, cx - 16, cy + 12, 32, 26);
        using var stripePen = new Pen(stripe, 1.5f);
        g.DrawArc(stripePen, cx - 14, cy + 6, 12, 8, -10, 180);
        g.DrawArc(stripePen, cx + 2,  cy + 6, 12, 8, -10, 180);

        g.FillEllipse(bb, cx - 30, cy - 44, 60, 50);
        g.DrawLine(stripePen, cx - 5, cy - 42, cx - 5, cy - 30);
        g.DrawLine(stripePen, cx + 5, cy - 42, cx + 5, cy - 30);

        using var earb = new SolidBrush(ear);
        Point[] el = [new(cx-24,cy-40),new(cx-36,cy-66),new(cx-8,cy-46)];
        Point[] er = [new(cx+24,cy-40),new(cx+36,cy-66),new(cx+8,cy-46)];
        g.FillPolygon(earb, el); g.FillPolygon(earb, er);
        using var innerb = new SolidBrush(Color.FromArgb(255, 200, 215));
        Point[] il = [new(cx-22,cy-42),new(cx-31,cy-62),new(cx-10,cy-46)];
        Point[] ir = [new(cx+22,cy-42),new(cx+31,cy-62),new(cx+10,cy-46)];
        g.FillPolygon(innerb, il); g.FillPolygon(innerb, ir);

        using var snoutb = new SolidBrush(Color.FromArgb(252, 248, 244));
        g.FillEllipse(snoutb, cx - 14, cy - 18, 28, 20);
        using var noseb = new SolidBrush(Color.FromArgb(200, 140, 170));
        g.FillPolygon(noseb, new Point[] { new(cx, cy - 17), new(cx - 5, cy - 11), new(cx + 5, cy - 11) });

        DrawFace(g, cx, cy - 24, happy, sad, blinking, 11, false);

        if (happy && frame % 2 == 0)
        {
            using var tongueb = new SolidBrush(Color.FromArgb(255, 140, 160));
            g.FillEllipse(tongueb, cx - 5, cy - 2, 10, 8);
        }

        using var wp = new Pen(Color.FromArgb(170, 230, 225, 240), 1.4f);
        g.DrawLine(wp, cx - 32, cy - 14, cx - 9, cy - 13); g.DrawLine(wp, cx - 32, cy - 9, cx - 9, cy - 9);
        g.DrawLine(wp, cx + 9, cy - 13, cx + 32, cy - 14); g.DrawLine(wp, cx + 9, cy - 9, cx + 32, cy - 9);

        using var pawb = new SolidBrush(fur);
        int lpY = happy && frame % 2 == 0 ? cy + 34 : cy + 40;
        int rpY = happy && frame % 2 == 1 ? cy + 34 : cy + 40;
        g.FillEllipse(pawb, cx - 32, lpY, 24, 14); g.FillEllipse(pawb, cx + 8, rpY, 24, 14);

        // Magic sparkles alternating purple/blue
        using var m1 = new SolidBrush(Color.FromArgb(frame%2==0?210:80, 200, 100, 255));
        using var m2 = new SolidBrush(Color.FromArgb(frame%2==0?80:210, 100, 200, 255));
        DrawSparkle(g, m1, cx + 42, cy - 42, 7); DrawSparkle(g, m2, cx - 44, cy - 34, 6);
        DrawSparkle(g, m1, cx + 30, cy - 62, 5); DrawSparkle(g, m2, cx - 32, cy - 60, 5);
        DrawSparkle(g, m1, cx, cy - 70, 6);
    }

    // ═══════════════════════════════════════════════════════════════
    // DOG  —  5 evolution stages
    // ═══════════════════════════════════════════════════════════════
    private static void DrawDog(Graphics g, int cx, int cy,
                                 bool happy, bool sad, int frame, PetEvolution stage, bool blinking)
    {
        switch (stage)
        {
            case PetEvolution.Egg:    DrawDogEgg(g, cx, cy, frame);                                break;
            case PetEvolution.Baby:   DrawDogBaby(g, cx, cy, happy, sad, frame, blinking);         break;
            case PetEvolution.Junior: DrawDogJunior(g, cx, cy, happy, sad, frame, blinking);       break;
            case PetEvolution.Adult:  DrawDogAdult(g, cx, cy, happy, sad, frame, blinking);        break;
            case PetEvolution.Legend: DrawDogLegend(g, cx, cy, happy, sad, frame, blinking);       break;
        }
    }

    // ── Egg (Lv 1) ───────────────────────────────────────────────────
    private static void DrawDogEgg(Graphics g, int cx, int cy, int frame)
    {
        int wobble = frame % 2 == 0 ? -4 : 4;

        using var shellBrush = new SolidBrush(Color.FromArgb(255, 245, 228));
        using var shellPen   = new Pen(Color.FromArgb(210, 185, 140), 2f);
        var eggRect = new Rectangle(cx - 22 + wobble, cy - 38, 44, 56);
        g.FillEllipse(shellBrush, eggRect);
        g.DrawEllipse(shellPen,   eggRect);

        // Spots on dog egg
        using var spotBrush = new SolidBrush(Color.FromArgb(60, 190, 155, 100));
        g.FillEllipse(spotBrush, cx - 14 + wobble, cy - 20, 10, 8);
        g.FillEllipse(spotBrush, cx + 4  + wobble, cy - 10, 8,  6);

        if (frame >= 2)
        {
            using var crackPen = new Pen(Color.FromArgb(180, 155, 110), 1.5f);
            g.DrawLine(crackPen, cx + wobble,     cy - 8, cx + 8 + wobble, cy - 18);
            g.DrawLine(crackPen, cx + 8 + wobble, cy - 18, cx + 4 + wobble, cy - 26);
        }

        // Dog ear peek on frame 3
        if (frame == 3)
        {
            using var earBrush = new SolidBrush(Color.FromArgb(200, 165, 110));
            g.FillEllipse(earBrush, cx - 24 + wobble, cy - 30, 14, 20);
        }

        using var zFont  = new Font("Segoe UI", 8f, FontStyle.Bold);
        using var zBrush = new SolidBrush(Color.FromArgb(frame % 2 == 0 ? 200 : 80, 150, 180, 120));
        g.DrawString("z", zFont, zBrush, cx + 20 + wobble, cy - 42);
        g.DrawString("Z", zFont, zBrush, cx + 28 + wobble, cy - 52);
    }

    // ── Baby dog (Lv 2-3) ────────────────────────────────────────────
    private static void DrawDogBaby(Graphics g, int cx, int cy, bool happy, bool sad, int frame, bool blinking)
    {
        var bodyColor = Color.FromArgb(240, 210, 165);

        // Chubby body
        using var bodyBrush = new SolidBrush(bodyColor);
        g.FillEllipse(bodyBrush, cx - 20, cy + 4, 40, 30);
        using var bellyBrush = new SolidBrush(Color.FromArgb(255, 240, 220));
        g.FillEllipse(bellyBrush, cx - 10, cy + 10, 20, 18);

        // Huge round head (puppy proportions)
        g.FillEllipse(bodyBrush, cx - 24, cy - 36, 48, 44);

        // Big floppy ears
        int earFlap = happy ? (frame % 2 == 0 ? 3 : -3) : 0;
        using var earBrush = new SolidBrush(Color.FromArgb(210, 175, 120));
        g.FillEllipse(earBrush, cx - 36, cy - 28 + earFlap,  16, 26);
        g.FillEllipse(earBrush, cx + 20, cy - 28 - earFlap,  16, 26);

        // Round snout
        using var snoutBrush = new SolidBrush(Color.FromArgb(255, 235, 210));
        g.FillEllipse(snoutBrush, cx - 12, cy - 16, 24, 18);
        using var noseBrush = new SolidBrush(Color.FromArgb(80, 50, 20));
        g.FillEllipse(noseBrush, cx - 6, cy - 15, 12, 8);

        DrawFace(g, cx, cy - 20, happy, sad, blinking, 8, true);

        // Tiny wagging tail stub
        int tw = happy ? (frame % 2 == 0 ? 14 : -4) : 4;
        using var tailPen = new Pen(bodyColor, 6f)
              { StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap   = System.Drawing.Drawing2D.LineCap.Round };
        g.DrawLine(tailPen, cx + 18, cy + 16, cx + 28 + tw, cy + 6);

        // Little paws
        g.FillEllipse(bodyBrush, cx - 24, cy + 28, 18, 10);
        g.FillEllipse(bodyBrush, cx + 6,  cy + 28 - (happy && frame % 2 == 0 ? 4 : 0), 18, 10);

        if (happy) DrawSparkles(g, cx, cy, frame, Color.FromArgb(255, 180, 80), 3, small: true);
    }

    // ── Junior dog (Lv 4-6) ──────────────────────────────────────────
    private static void DrawDogJunior(Graphics g, int cx, int cy, bool happy, bool sad, int frame, bool blinking)
    {
        var bodyColor = Color.FromArgb(200, 160, 100);
        var earColor  = Color.FromArgb(170, 128, 75);

        // Tail — enthusiastic wag when happy
        float wagAngle = happy ? (frame % 2 == 0 ? 35f : -5f) : 10f;
        using var tailPen = new Pen(bodyColor, 8f)
              { StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap   = System.Drawing.Drawing2D.LineCap.Round };
        var tailBase = new PointF(cx + 26, cy + 12);
        var tailTip  = new PointF(
            tailBase.X + (float)(30 * Math.Cos(wagAngle * Math.PI / 180)),
            tailBase.Y - (float)(30 * Math.Sin(wagAngle * Math.PI / 180)));
        g.DrawLine(tailPen, tailBase, tailTip);

        // Body
        using var bodyBrush = new SolidBrush(bodyColor);
        g.FillEllipse(bodyBrush, cx - 28, cy + 2, 56, 42);
        using var bellyBrush = new SolidBrush(Color.FromArgb(230, 200, 155));
        g.FillEllipse(bellyBrush, cx - 14, cy + 12, 28, 24);

        // Head
        g.FillEllipse(bodyBrush, cx - 28, cy - 42, 56, 50);

        // Floppy ears — bounce with happiness
        int earFlap = happy ? (frame % 2 == 0 ? 4 : -4) : 0;
        using var earBrush = new SolidBrush(earColor);
        g.FillEllipse(earBrush, cx - 40, cy - 34 + earFlap, 18, 32);
        g.FillEllipse(earBrush, cx + 22, cy - 34 - earFlap, 18, 32);

        // Snout
        using var snoutBrush = new SolidBrush(Color.FromArgb(240, 215, 175));
        g.FillEllipse(snoutBrush, cx - 14, cy - 18, 28, 20);
        using var noseBrush = new SolidBrush(Color.FromArgb(80, 50, 20));
        g.FillEllipse(noseBrush, cx - 7, cy - 17, 14, 9);
        using var noseShineBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
        g.FillEllipse(noseShineBrush, cx - 4, cy - 16, 5, 4);

        DrawFace(g, cx, cy - 24, happy, sad, blinking, 10, true);

        // Tongue when happy
        if (happy && frame % 2 == 0)
        {
            using var tongueBrush = new SolidBrush(Color.FromArgb(255, 120, 130));
            g.FillEllipse(tongueBrush, cx - 6, cy - 2, 12, 10);
        }

        // Paws
        using var pawBrush = new SolidBrush(bodyColor);
        int lpY = happy && frame % 2 == 0 ? cy + 36 : cy + 42;
        int rpY = happy && frame % 2 == 1 ? cy + 36 : cy + 42;
        g.FillEllipse(pawBrush, cx - 30, lpY, 22, 13);
        g.FillEllipse(pawBrush, cx + 8,  rpY, 22, 13);

        if (happy) DrawSparkles(g, cx, cy, frame, Color.FromArgb(255, 200, 80), 4, small: false);
        if (sad)   DrawTears(g, cx, cy, 2);

        // Blush cheeks when happy
        if (happy)
        {
            using var blushBrush = new SolidBrush(Color.FromArgb(70, 255, 130, 100));
            g.FillEllipse(blushBrush, cx - 30, cy - 14, 16, 10);
            g.FillEllipse(blushBrush, cx + 14, cy - 14, 16, 10);
        }
    }

    // ── Adult dog (Lv 7-9) ───────────────────────────────────────────
    private static void DrawDogAdult(Graphics g, int cx, int cy, bool happy, bool sad, int frame, bool blinking)
    {
        var bodyColor = Color.FromArgb(230, 190, 140);
        var earColor  = Color.FromArgb(190, 145, 95);

        // Big wagging tail
        float wagAngle = happy ? (frame % 2 == 0 ? 40f : -10f) : 12f;
        using var tailPen = new Pen(bodyColor, 10f)
              { StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap   = System.Drawing.Drawing2D.LineCap.Round };
        var tailBase = new PointF(cx + 28, cy + 14);
        var tailTip  = new PointF(
            tailBase.X + (float)(34 * Math.Cos(wagAngle * Math.PI / 180)),
            tailBase.Y - (float)(34 * Math.Sin(wagAngle * Math.PI / 180)));
        g.DrawLine(tailPen, tailBase, tailTip);

        // Body
        using var bodyBrush = new SolidBrush(bodyColor);
        g.FillEllipse(bodyBrush, cx - 30, cy, 60, 46);
        using var bellyBrush = new SolidBrush(Color.FromArgb(255, 235, 210));
        g.FillEllipse(bellyBrush, cx - 16, cy + 12, 32, 26);

        // Head
        g.FillEllipse(bodyBrush, cx - 30, cy - 44, 60, 50);

        // Floppy ears
        int earFlap = happy ? (frame % 2 == 0 ? 4 : -4) : 0;
        using var earBrush = new SolidBrush(earColor);
        g.FillEllipse(earBrush, cx - 42, cy - 36 + earFlap, 20, 34);
        g.FillEllipse(earBrush, cx + 22, cy - 36 - earFlap, 20, 34);

        // Snout
        using var snoutBrush = new SolidBrush(Color.FromArgb(255, 225, 195));
        g.FillEllipse(snoutBrush, cx - 14, cy - 18, 28, 20);
        using var noseBrush = new SolidBrush(Color.FromArgb(80, 50, 20));
        g.FillEllipse(noseBrush, cx - 7, cy - 17, 14, 9);
        using var noseShineBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
        g.FillEllipse(noseShineBrush, cx - 4, cy - 16, 5, 4);

        DrawFace(g, cx, cy - 24, happy, sad, blinking, 11, true);

        if (happy && frame % 2 == 0)
        {
            using var tongueBrush = new SolidBrush(Color.FromArgb(255, 120, 130));
            g.FillEllipse(tongueBrush, cx - 6, cy - 2, 12, 10);
            using var tongueLine = new Pen(Color.FromArgb(200, 80, 100), 1.5f);
            g.DrawLine(tongueLine, cx, cy - 2, cx, cy + 8);
        }

        using var pawBrush = new SolidBrush(bodyColor);
        int lpY = happy && frame % 2 == 0 ? cy + 34 : cy + 40;
        int rpY = happy && frame % 2 == 1 ? cy + 34 : cy + 40;
        g.FillEllipse(pawBrush, cx - 32, lpY, 24, 14);
        g.FillEllipse(pawBrush, cx + 8,  rpY, 24, 14);

        if (happy)
        {
            using var blushBrush = new SolidBrush(Color.FromArgb(80, 255, 130, 100));
            g.FillEllipse(blushBrush, cx - 32, cy - 14, 16, 10);
            g.FillEllipse(blushBrush, cx + 16, cy - 14, 16, 10);
            DrawHearts(g, cx, cy, frame, 4);
        }
        if (sad) DrawTears(g, cx, cy, 2);
    }

    // ── Legend dog (Lv 10+) ──────────────────────────────────────────
    private static void DrawDogLegend(Graphics g, int cx, int cy, bool happy, bool sad, int frame, bool blinking)
    {
        // Golden glow aura
        int alpha = frame % 2 == 0 ? 70 : 30;
        using var auraBrush = new SolidBrush(Color.FromArgb(alpha, 255, 200, 80));
        g.FillEllipse(auraBrush, cx - 50, cy - 58, 100, 100);
        using var aura2Brush = new SolidBrush(Color.FromArgb(alpha / 2, 255, 240, 160));
        g.FillEllipse(aura2Brush, cx - 60, cy - 68, 120, 120);

        // Crown  
        using var crownBrush = new SolidBrush(Color.FromArgb(255, 200, 40));
        Point[] crown =
        [
            new(cx - 18, cy - 64), new(cx - 18, cy - 80),
            new(cx - 8,  cy - 70), new(cx,      cy - 84),
            new(cx + 8,  cy - 70), new(cx + 18, cy - 80),
            new(cx + 18, cy - 64),
        ];
        g.FillPolygon(crownBrush, crown);
        using var jewel1 = new SolidBrush(Color.FromArgb(100, 200, 100));
        using var jewel2 = new SolidBrush(Color.FromArgb(255, 80, 80));
        g.FillEllipse(jewel1, cx - 4,  cy - 82, 8, 8);
        g.FillEllipse(jewel2, cx - 16, cy - 78, 6, 6);
        g.FillEllipse(jewel2, cx + 10, cy - 78, 6, 6);

        // Draw adult dog as base
        DrawDogAdult(g, cx, cy, happy, sad, frame, blinking);

        // Golden sparkle stars
        using var star1 = new SolidBrush(Color.FromArgb(
            frame % 2 == 0 ? 220 : 80, 255, 215, 0));
        using var star2 = new SolidBrush(Color.FromArgb(
            frame % 2 == 0 ? 80 : 220, 255, 160, 40));
        DrawSparkle(g, star1, cx + 42, cy - 42, 8);
        DrawSparkle(g, star2, cx - 44, cy - 34, 7);
        DrawSparkle(g, star1, cx + 30, cy - 62, 6);
        DrawSparkle(g, star2, cx - 32, cy - 60, 6);
        DrawSparkle(g, star1, cx,      cy - 70, 7);
    }

    // ═══════════════════════════════════════════════════════════════
    // SHARED DRAWING HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// Draws eyes + mouth. eyeY is the centre of the eye row.
    private static void DrawFace(Graphics g, int cx, int eyeY,
                                  bool happy, bool sad, bool blinking,
                                  int eyeSpread, bool isDog)
    {
        int ex = eyeSpread;
        if (blinking)
        {
            // Closed eyes — curved lines ╰╯
            using var blinkPen = new Pen(Color.FromArgb(80, 50, isDog ? 20 : 50), 2.5f)
                  { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
            g.DrawArc(blinkPen, cx - ex - 5, eyeY, 12, 6, 0, -180);
            g.DrawArc(blinkPen, cx + ex - 6, eyeY, 12, 6, 0, -180);
        }
        else if (happy)
        {
            using var eyePen = new Pen(Color.FromArgb(80, 50, isDog ? 20 : 50), 3f)
                  { StartCap = System.Drawing.Drawing2D.LineCap.Round,
                    EndCap   = System.Drawing.Drawing2D.LineCap.Round };
            g.DrawArc(eyePen, cx - ex - 6, eyeY - 2, 13, 9, 180, 180);
            g.DrawArc(eyePen, cx + ex - 7, eyeY - 2, 13, 9, 180, 180);
            // Blush cheeks
            using var blushb = new SolidBrush(Color.FromArgb(60, 255, 120, 90));
            g.FillEllipse(blushb, cx - ex - 10, eyeY + 6, 14, 9);
            g.FillEllipse(blushb, cx + ex - 4,  eyeY + 6, 14, 9);
        }
        else if (sad)
        {
            using var eyePen = new Pen(Color.FromArgb(80, 50, isDog ? 20 : 50), 2.5f);
            g.DrawEllipse(eyePen, cx - ex - 5, eyeY - 4, 11, 10);
            g.DrawEllipse(eyePen, cx + ex - 6, eyeY - 4, 11, 10);
        }
        else
        {
            var eyeColor = Color.FromArgb(80, 50, isDog ? 20 : 50);
            using var eyeBrush = new SolidBrush(eyeColor);
            g.FillEllipse(eyeBrush, cx - ex - 6, eyeY - 4, 12, 11);
            g.FillEllipse(eyeBrush, cx + ex - 6, eyeY - 4, 12, 11);
            using var shineBrush = new SolidBrush(Color.White);
            g.FillEllipse(shineBrush, cx - ex - 3, eyeY - 2, 4, 4);
            g.FillEllipse(shineBrush, cx + ex - 3, eyeY - 2, 4, 4);
        }

        // Mouth
        using var mouthPen = new Pen(Color.FromArgb(isDog ? 140 : 200, isDog ? 90 : 80, isDog ? 50 : 100), 2f)
              { StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap   = System.Drawing.Drawing2D.LineCap.Round };
        int my = eyeY + 14;
        if (happy)
        {
            g.DrawArc(mouthPen, cx - 9, my - 2, 9, 7, 0, 180);
            g.DrawArc(mouthPen, cx,     my - 2, 9, 7, 0, 180);
        }
        else if (sad)
            g.DrawArc(mouthPen, cx - 7, my, 14, 7, 0, -180);
        else
        {
            g.DrawLine(mouthPen, cx - 4, my + 2, cx, my);
            g.DrawLine(mouthPen, cx,     my,     cx + 4, my + 2);
        }
    }

    private static void DrawSparkles(Graphics g, int cx, int cy,
                                      int frame, Color color, int count, bool small)
    {
        int sz   = small ? 4 : 6;
        int alpha = frame % 2 == 0 ? 220 : 90;
        using var brush = new SolidBrush(Color.FromArgb(alpha, color));
        int[][] positions = small
            ? [[cx + 24, cy - 32], [cx - 26, cy - 26], [cx + 18, cy - 14]]
            : [[cx + 34, cy - 42], [cx - 38, cy - 36], [cx + 26, cy - 18],
               [cx - 20, cy - 54], [cx,      cy - 58]];
        for (int i = 0; i < Math.Min(count, positions.Length); i++)
            DrawSparkle(g, brush, positions[i][0], positions[i][1], sz);
    }

    private static void DrawTears(Graphics g, int cx, int cy, int count)
    {
        using var tearBrush = new SolidBrush(Color.FromArgb(160, 140, 180, 255));
        if (count >= 1) g.FillEllipse(tearBrush, cx - 18, cy - 12, 5, 9);
        if (count >= 2) g.FillEllipse(tearBrush, cx + 13, cy - 12, 5, 9);
    }

    private static void DrawHearts(Graphics g, int cx, int cy, int frame, int count)
    {
        int alpha = frame % 2 == 0 ? 220 : 80;
        using var brush = new SolidBrush(Color.FromArgb(alpha, 255, 100, 130));
        int[][] pos = [[cx + 36, cy - 44], [cx - 40, cy - 38],
                       [cx + 28, cy - 60], [cx - 28, cy - 58]];
        for (int i = 0; i < Math.Min(count, pos.Length); i++)
            DrawHeart(g, brush, pos[i][0], pos[i][1], 7);
    }

    private static void DrawSparkle(Graphics g, Brush brush, int x, int y, int size)
    {
        g.FillEllipse(brush, x - size / 2, y - size / 2, size, size);
        Color c = brush is SolidBrush sb ? sb.Color : Color.Yellow;
        using var pen = new Pen(Color.FromArgb(Math.Max(c.A - 40, 0), c.R, c.G, c.B), 1.5f);
        g.DrawLine(pen, x - size, y, x + size, y);
        g.DrawLine(pen, x, y - size, x, y + size);
        g.DrawLine(pen, x - size + 2, y - size + 2, x + size - 2, y + size - 2);
        g.DrawLine(pen, x + size - 2, y - size + 2, x - size + 2, y + size - 2);
    }

    private static void DrawHeart(Graphics g, Brush brush, int x, int y, int size)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddBezier(x, y, x - size, y - size, x - size * 2, y + size / 2, x, y + size * 2);
        path.AddBezier(x, y + size * 2, x + size * 2, y + size / 2, x + size, y - size, x, y);
        g.FillPath(brush, path);
    }

    // ── HELPERS ──────────────────────────────────────────────────────
    private static (Label lbl, ProgressBar pb) MakeStatBar(string label, int y, Color color)
    {
        var lbl = new Label
        {
            Text = label,
            Font = PawTheme.FontSmall,
            ForeColor = PawTheme.TextMuted,
            AutoSize = true,
            Location = new Point(InnerPad, y),
            BackColor = Color.Transparent,
        };
        var pb = new ProgressBar
        {
            Location = new Point(InnerPad, y + 15 + StatBarLblGap),
            Width = PetPanelW - InnerPad * 2,
            Height = StatBarH,
            Style = ProgressBarStyle.Continuous,
            Maximum = 100,
            Minimum = 0,
        };
        return (lbl, pb);
    }

    private static Label MakeStatLabel(string text, Point loc) => new()
    {
        Text = text,
        Font = PawTheme.FontSmall,
        ForeColor = PawTheme.TextDark,
        AutoSize = true,
        Location = loc,
        BackColor = Color.Transparent,
    };

    private static void PaintBorder(PaintEventArgs e, Control c)
    {
        using var pen = new Pen(PawTheme.CardBorder, 1.5f);
        e.Graphics.DrawRectangle(pen, 0, 0, c.Width - 1, c.Height - 1);
    }

    private void ShowInfo(string msg) =>
        MessageBox.Show(msg, "Pawductivity 🐾", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
