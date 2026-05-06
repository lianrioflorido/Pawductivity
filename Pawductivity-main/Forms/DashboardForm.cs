using Pawductivity.Controls;
using Pawductivity.Managers;
using Pawductivity.Models;
using Pawductivity.Persistence;

namespace Pawductivity.Forms;

public class DashboardForm : Form
{
    private readonly GameManager _gm;

    // Pet panel widgets
    private PetAnimationControl _petCanvas = null!;
    private Label _lblPetName = null!;
    private Label _lblGreeting = null!;
    private Label _lblLevel = null!;
    private ProgressBar _pbHealth = null!;
    private ProgressBar _pbMood = null!;
    private ProgressBar _pbXp = null!;
    private Label _lblCoins = null!;

    // Task panel widgets
    private ListView _lvTasks = null!;
    private ComboBox _cmbFilter = null!;
    private ComboBox _cmbPriority = null!;
    private Button _btnAddTask = null!;
    private Button _btnComplete = null!;
    private Button _btnDelete = null!;
    private Button _btnEdit = null!;

    // Nav buttons
    private Button _btnSettings = null!;
    private Button _btnShop = null!;
    private Button _btnStats = null!;
    private Button _btnLogout = null!;

    // Stats labels
    private Label _lblToday = null!;
    private Label _lblStreak = null!;
    private Label _lblPending = null!;

    private System.Windows.Forms.Timer _decayTimer = null!;

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

        var change = _gm.ApplyOverduePenalties();
        if (change.Success) _petCanvas.ShowOverduePenalty(change);
        RefreshAll();

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
        MinimumSize = new Size(970, 790);
        ClientSize = new Size(970, 790);
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

        _btnSettings = new Button
        {
            Text = "⚙",
            Location = new Point(ClientSize.Width - Margin - 40, 7),
            Size = new Size(40, 40),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Symbol", 13f, FontStyle.Bold),
            BackColor = PawTheme.Primary,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            TabStop = false,
        };
        _btnSettings.FlatAppearance.BorderSize = 0;
        _btnSettings.FlatAppearance.MouseOverBackColor = PawTheme.PrimaryHover;
        _btnSettings.Click += (s, e) => new SettingsForm(RebuildForTheme).ShowDialog(this);
        var lblApp = new Label
        {
            Text = "🐾 Pawductivity",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(Margin + 24, (TopBarH - 40) / 2),
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
        topBar.Controls.AddRange([_btnSettings, lblApp, lblUser]);
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

        _petCanvas = new PetAnimationControl(_gm)
        {
            Size = new Size(PetPanelW - InnerPad * 2, 180),
            Location = new Point(InnerPad, InnerPad + 4),
        };

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
        _btnShop.Click += (s, e) => new ShopForm(_gm, OnShopItemPurchased).ShowDialog(this);
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

        var lblFilterLabel = new Label
        {
            Text = "Status:",
            Font = PawTheme.FontSmall,
            ForeColor = PawTheme.TextMuted,
            AutoSize = true,
            BackColor = Color.Transparent,
        };

        _cmbFilter = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = PawTheme.FontBody,
            BackColor = PawTheme.Background,
            ForeColor = PawTheme.TextDark,
            FlatStyle = FlatStyle.Flat,
            Width = 130,
            Height = 26,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        _cmbFilter.Items.AddRange(new object[] { "All", "Pending", "Overdue", "Done" });
        _cmbFilter.SelectedIndex = 0;
        _cmbFilter.SelectedIndexChanged += (s, e) => RefreshTaskList();

        _cmbPriority = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = PawTheme.FontBody,
            BackColor = PawTheme.Background,
            ForeColor = PawTheme.TextDark,
            FlatStyle = FlatStyle.Flat,
            Width = 130,
            Height = 26,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        _cmbPriority.Items.AddRange(new object[] { "All", "High", "Medium", "Low" });
        _cmbPriority.SelectedIndex = 0;
        _cmbPriority.SelectedIndexChanged += (s, e) => RefreshTaskList();

        var lblPriorityLabel = new Label
        {
            Text = "Priority:",
            Font = PawTheme.FontSmall,
            ForeColor = PawTheme.TextMuted,
            AutoSize = true,
            BackColor = Color.Transparent,
        };

        int listTop = lblTaskTitle.Bottom + 50;
        int listH = petPanelH - listTop - ButtonH - InnerPad * 2 - 4;

        _lvTasks = new ListView
        {
            Location = new Point(InnerPad, listTop),
            Size = new Size(taskPanelW - InnerPad * 2, listH),
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            MultiSelect = true,
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
        typeof(ListView).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance)!
            .SetValue(_lvTasks, true);
        _lvTasks.ItemSelectionChanged += (s, e) => _lvTasks.Invalidate();
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

        taskPanel.Resize += (s, e) =>
        {
            _cmbFilter.Location = new Point(taskPanel.Width - _cmbFilter.Width - InnerPad, lblTaskTitle.Bottom + 18);
            lblFilterLabel.Location = new Point(taskPanel.Width - _cmbFilter.Width - InnerPad, lblTaskTitle.Bottom - 1);
            _cmbPriority.Location = new Point(taskPanel.Width - _cmbFilter.Width - _cmbPriority.Width - InnerPad - 16, lblTaskTitle.Bottom + 18);
            lblPriorityLabel.Location = new Point(taskPanel.Width - _cmbFilter.Width - _cmbPriority.Width - InnerPad - 16, lblTaskTitle.Bottom - 1);
        };
        _cmbFilter.Location = new Point(taskPanelW - _cmbFilter.Width - InnerPad, lblTaskTitle.Bottom + 18);
        lblFilterLabel.Location = new Point(taskPanelW - _cmbFilter.Width - InnerPad, lblTaskTitle.Bottom - 1);
        _cmbPriority.Location = new Point(taskPanelW - _cmbFilter.Width - _cmbPriority.Width - InnerPad - 16, lblTaskTitle.Bottom + 18);
        lblPriorityLabel.Location = new Point(taskPanelW - _cmbFilter.Width - _cmbPriority.Width - InnerPad - 16, lblTaskTitle.Bottom - 1);

        taskPanel.Controls.AddRange([
            lblTaskTitle, lblFilterLabel, _cmbFilter, lblPriorityLabel, _cmbPriority, _lvTasks,
            _btnAddTask, _btnComplete, _btnEdit, _btnDelete,
        ]);

        Controls.AddRange([topBar, petPanel, taskPanel]);
    }

    // ── CUSTOM LISTVIEW DRAW ─────────────────────────────────────────
    private void LvTasks_DrawItem(object? sender, DrawListViewItemEventArgs e)
    {
        e.DrawDefault = false;
        if (e.Item?.Tag is not TaskItem task) return;

        bool sel = e.Item.Selected;
        bool hot = (e.State & ListViewItemStates.Hot) != 0;

        Color bg = sel ? PawTheme.Primary :
                   task.IsCompleted ? PawTheme.CompletedTask :
                   task.IsOverdue ? PawTheme.OverdueTask :
                                     _lvTasks.BackColor;

        using var brush = new SolidBrush(bg);
        e.Graphics.FillRectangle(brush, e.Bounds);

        if (hot && !sel)
        {
            using var hoverBrush = new SolidBrush(Color.FromArgb(40, PawTheme.TextDark));
            e.Graphics.FillRectangle(hoverBrush, e.Bounds);
        }
    }

    private void LvTasks_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
    {
        if (e.Item?.Tag is not TaskItem task) return;

        bool sel = e.Item.Selected;

        Color bgSub = sel ? PawTheme.Primary : Color.Transparent;
        using var bgBrush = new SolidBrush(bgSub);
        e.Graphics.FillRectangle(bgBrush, e.Bounds);

        // Always use a visible foreground regardless of hover/selection state
        bool hot2 = (e.ItemState & ListViewItemStates.Hot) != 0;
        Color fg = sel ? Color.White :
                   task.IsCompleted ? PawTheme.TextGreen :
                   task.IsOverdue ? PawTheme.TextDark :
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

        int totalXp = 0;
        int totalCoins = 0;
        int completed = 0;
        PetChangeResult? lastChange = null;

        foreach (ListViewItem selected in _lvTasks.SelectedItems)
        {
            if (selected?.Tag is not TaskItem task) continue;
            if (task.IsCompleted) continue;

            var change = _gm.CompleteTask(task.Id);
            totalXp += change.XpDelta;
            totalCoins += change.CoinDelta;
            completed++;
            lastChange = change;
        }

        if (completed == 0) { ShowInfo("All selected tasks are already done! 🎉"); return; }

        ShowInfo($"{_gm.Pet.GetGreeting()}\n\n{completed} task(s) completed!\n+{totalXp} XP gained! 🌟 Coins earned: {totalCoins} 🪙");
        RefreshAll();
        if (lastChange is not null) _petCanvas.ShowTaskCompletion(lastChange);
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

        string confirmMsg = _lvTasks.SelectedItems.Count == 1
            ? $"Delete \"{((TaskItem)_lvTasks.SelectedItems[0].Tag!).Title}\"?"
            : $"Delete {_lvTasks.SelectedItems.Count} selected tasks?";

        if (MessageBox.Show(confirmMsg, "Confirm",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        var toDelete = _lvTasks.SelectedItems
            .Cast<ListViewItem>()
            .Select(i => (TaskItem)i.Tag!)
            .ToList();

        foreach (var task in toDelete)
            _gm.DeleteTask(task.Id);

        RefreshAll();
    }


    private void OnShopItemPurchased(PetChangeResult change)
    {
        RefreshAll();
        _petCanvas.ShowShopPurchase(change);
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
        if (pet.IsSick)
        {
            _lblGreeting.Text = "⚠️ Sick! Complete tasks to recover.";
            _lblGreeting.ForeColor = Color.FromArgb(200, 60, 60);
            if (pet.IsCriticallySick)
                _lblLevel.Text = $"Lv.{pet.Level} • ❌ XP locked";
        }
        else
        {
            _lblGreeting.ForeColor = PawTheme.TextMuted;  // restore normal color
        }
        _pbMood.Value = pet.Mood;
        _pbXp.Value = Math.Min(100, (int)((double)pet.XP / pet.XpForNextLevel * 100));

        _lblToday.Text = $"✅ Completed today: {_gm.CompletedToday}";
        _lblStreak.Text = $"🔥 Streak: {_gm.CurrentStreak} day(s)";
        _lblPending.Text = $"📋 Pending: {_gm.PendingCount}";

        RefreshTaskList();
    }
    private void RefreshTaskList()
    {
        string filter = _cmbFilter?.SelectedItem?.ToString() ?? "All";
        string priority = _cmbPriority?.SelectedItem?.ToString() ?? "All";

        var tasks = _gm.Tasks
            .Where(t => filter switch
            {
                "Pending" => !t.IsCompleted && !t.IsOverdue,
                "Overdue" => t.IsOverdue && !t.IsCompleted,
                "Done" => t.IsCompleted,
                _ => true,
            })
            .Where(t => priority switch
            {
                "High" => t.Priority == TaskPriority.High,
                "Medium" => t.Priority == TaskPriority.Medium,
                "Low" => t.Priority == TaskPriority.Low,
                _ => true,
            })
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate);

        _lvTasks.Items.Clear();
        foreach (var t in tasks)
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
        _decayTimer.Tick += (s, e) =>
        {
            var change = _gm.ApplyOverduePenalties();
            if (change.Success) _petCanvas.ShowOverduePenalty(change);
            RefreshAll();
        };
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
        _petCanvas.StopAnimation();

        PawTheme.SetTheme(PawTheme.DefaultThemeKey);

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


    private void RebuildForTheme()
    {
        SaveManager.Save(_gm);
        _petCanvas.StopAnimation();

        while (Controls.Count > 0)
        {
            var control = Controls[0];
            Controls.RemoveAt(0);
            control.Dispose();
        }

        InitializeComponent();
        RefreshAll();
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
