using Pawductivity.Managers;
using Pawductivity.Models;
using Pawductivity.Persistence;

namespace Pawductivity.Forms;
public class LoginForm : Form
{
    // ── Widgets ──────────────────────────────────────────────────────
    private ComboBox _cboProfiles = null!;
    private Button _btnContinue = null!;
    private Button _btnDelete = null!;
    private Button _btnNewToggle = null!;

    private Panel _pnlNew = null!;
    private TextBox _txtUsername = null!;
    private TextBox _txtPetName = null!;
    private ComboBox _cboPetType = null!;
    private Button _btnStart = null!;
    private Button _btnCancel = null!;

    // ── Fixed layout constants ────────────────────────────────────────
    private const int CardX = 24;
    private const int CardW = 372;
    private const int BtnH = 34;

    private const int EmojiY = 15;
    private const int EmojiH = 97;
    private const int TitleY = EmojiY + EmojiH + 16;
    private const int TitleH = 48;
    private const int TagY = TitleY + TitleH + 6;
    private const int TagH = 20;

    private const int ReturnCardY = TagY + TagH + 18;
    private const int ReturnCardH = 150;

    private const int ToggleBtnY = ReturnCardY + ReturnCardH + 10;

    private const int NewCardY = ToggleBtnY + BtnH + 10;
    private const int NewCardH = 270;

    private const int FormH_Compact = ToggleBtnY + BtnH + 46;
    private const int FormH_Expanded = NewCardY + NewCardH + 36;

    public LoginForm()
    {
        InitializeComponent();
        LoadProfiles();
    }

    private void InitializeComponent()
    {
        Text = "Pawductivity 🐾";
        ClientSize = new Size(420, FormH_Compact);
        MinimumSize = new Size(420, FormH_Compact);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = PawTheme.Background;
        Font = PawTheme.FontBody;

        // ── Emoji ────────────────────────────────────────────────────
        var lblEmoji = new Label
        {
            Text = "🐾",
            Font = new Font("Segoe UI Emoji", 48f),
            AutoSize = false,
            Size = new Size(420, EmojiH),
            Location = new Point(0, EmojiY),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };

        // ── Title ────────────────────────────────────────────────────
        var lblTitle = new Label
        {
            Text = "Pawductivity",
            Font = PawTheme.FontTitle,
            ForeColor = PawTheme.Primary,
            AutoSize = false,
            Size = new Size(420, TitleH),
            Location = new Point(0, TitleY),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };

        // ── Tagline ──────────────────────────────────────────────────
        var lblTag = new Label
        {
            Text = "Stay productive. Keep your pet happy! 💕",
            Font = PawTheme.FontSmall,
            ForeColor = PawTheme.TextMuted,
            AutoSize = false,
            Size = new Size(420, TagH),
            Location = new Point(0, TagY),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };

        // ── Returning-user card ───────────────────────────────────────
        var cardReturn = new Panel
        {
            Location = new Point(CardX, ReturnCardY),
            Size = new Size(CardW, ReturnCardH),
            BackColor = PawTheme.Surface,
        };
        cardReturn.Paint += PaintCardBorder;

        var lblWelcome = new Label
        {
            Text = "Welcome! 🐱",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = PawTheme.Primary,
            AutoSize = true,
            Location = new Point(14, 14),
            BackColor = Color.Transparent,
        };

        var lblPick = MakeSmallLabel("Choose your profile", new Point(14, 38));

        _cboProfiles = new ComboBox
        {
            Location = new Point(14, 58),
            Size = new Size(CardW - 28, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = PawTheme.Background,
            ForeColor = PawTheme.TextDark,
            Font = PawTheme.FontBody,
        };

        _btnContinue = new Button
        {
            Text = "▶  Continue",
            Location = new Point(14, 100),
            Size = new Size(160, BtnH),
        };
        PawTheme.StyleButton(_btnContinue);
        _btnContinue.Click += BtnContinue_Click;

        _btnDelete = new Button
        {
            Text = "🗑  Delete",
            Location = new Point(198, 100),
            Size = new Size(160, BtnH),
        };
        PawTheme.StyleButton(_btnDelete, outlined: true);
        _btnDelete.Click += BtnDelete_Click;

        cardReturn.Controls.AddRange([lblWelcome, lblPick, _cboProfiles, _btnContinue, _btnDelete]);

        // ── "+ New Profile" toggle button ─────────────────────────────
        _btnNewToggle = new Button
        {
            Text = "+ New Profile",
            Location = new Point(CardX, ToggleBtnY),
            Size = new Size(CardW, BtnH),
        };
        PawTheme.StyleButton(_btnNewToggle, outlined: true);
        _btnNewToggle.Click += BtnNewToggle_Click;

        // ── New-profile card (hidden by default) ──────────────────────
        _pnlNew = new Panel
        {
            Location = new Point(CardX, NewCardY),
            Size = new Size(CardW, NewCardH),
            BackColor = PawTheme.Surface,
            Visible = false,
        };
        _pnlNew.Paint += PaintCardBorder;

        var lblNewTitle = new Label
        {
            Text = "Create your profile ✨",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = PawTheme.Primary,
            AutoSize = true,
            Location = new Point(14, 14),
            BackColor = Color.Transparent,
        };

        var lblUser = MakeSmallLabel("Your name 👤", new Point(14, 43));
        _txtUsername = MakeTextBox(new Point(14, 64), CardW - 28);
        _txtUsername.PlaceholderText = "e.g. Marie";

        var lblPet = MakeSmallLabel("Pet name 🐱", new Point(14, 101));
        _txtPetName = MakeTextBox(new Point(14, 122), CardW - 28);
        _txtPetName.PlaceholderText = "e.g. Strawberry";

        var lblType = MakeSmallLabel("Pet type 🐾", new Point(14, 159));
        _cboPetType = new ComboBox
        {
            Location = new Point(14, 180),
            Size = new Size(CardW - 28, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = PawTheme.Background,
            ForeColor = PawTheme.TextDark,
            Font = PawTheme.FontBody,
        };
        _cboPetType.Items.AddRange(["🐱 Cat — earns XP faster!", "🐶 Dog — more forgiving!"]);
        _cboPetType.SelectedIndex = 0;

        _btnStart = new Button
        {
            Text = "Start! 🌸",
            Location = new Point(14, 220),
            Size = new Size(160, BtnH),
        };
        PawTheme.StyleButton(_btnStart);
        _btnStart.Click += BtnStart_Click;

        _btnCancel = new Button
        {
            Text = "Cancel",
            Location = new Point(198, 220),
            Size = new Size(160, BtnH),
        };
        PawTheme.StyleButton(_btnCancel, outlined: true);
        _btnCancel.Click += (s, e) => CollapseNewPanel();

        _pnlNew.Controls.AddRange([
            lblNewTitle,
            lblUser,  _txtUsername,
            lblPet,   _txtPetName,
            lblType,  _cboPetType,
            _btnStart, _btnCancel,
        ]);

        Controls.AddRange([lblEmoji, lblTitle, lblTag, cardReturn, _btnNewToggle, _pnlNew]);
    }

    // ─────────────────────────────────────────────────────────────────
    // PROFILE MANAGEMENT
    // ─────────────────────────────────────────────────────────────────
    public void LoadProfiles()
    {
        _cboProfiles.Items.Clear();
        var profiles = SaveManager.ListProfiles();

        if (profiles.Length == 0)
        {
            _cboProfiles.Items.Add("(no profiles yet)");
            _cboProfiles.SelectedIndex = 0;
            _btnContinue.Enabled = false;
            _btnDelete.Enabled = false;
            ExpandNewPanel();   // auto-open for first-time users
        }
        else
        {
            foreach (var name in profiles)
                _cboProfiles.Items.Add(name);

            _cboProfiles.SelectedIndex = 0;
            _btnContinue.Enabled = true;
            _btnDelete.Enabled = true;
        }
    }

    private void ExpandNewPanel()
    {
        _pnlNew.Visible = true;
        _btnNewToggle.Text = "↩  Back to profiles";
        ClientSize = new Size(420, FormH_Expanded);
        MinimumSize = new Size(420, FormH_Expanded);
    }

    private void CollapseNewPanel()
    {
        _pnlNew.Visible = false;
        _btnNewToggle.Text = "+ New Profile";
        ClientSize = new Size(420, FormH_Compact);
        MinimumSize = new Size(420, FormH_Compact);
        _txtUsername.Clear();
        _txtPetName.Clear();
        _cboPetType.SelectedIndex = 0;
    }

    // ─────────────────────────────────────────────────────────────────
    // BUTTON HANDLERS
    // ─────────────────────────────────────────────────────────────────
    private void BtnNewToggle_Click(object? sender, EventArgs e)
    {
        if (_pnlNew.Visible) CollapseNewPanel();
        else ExpandNewPanel();
    }

    private void BtnContinue_Click(object? sender, EventArgs e)
    {
        if (_cboProfiles.SelectedItem is not string username) return;

        var data = SaveManager.Load(username);
        if (data is null)
        {
            MessageBox.Show(
                "Could not load that profile. It may be corrupted.",
                "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        OpenDashboard(SaveManager.Restore(data));
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        string userName = _txtUsername.Text.Trim();
        string petName = _txtPetName.Text.Trim();

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(petName))
        {
            MessageBox.Show("Please fill in your name and a pet name! 🐾",
                            "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (SaveManager.ProfileExists(userName))
        {
            var confirm = MessageBox.Show(
                $"A profile named \"{userName}\" already exists.\n" +
                "Starting fresh will overwrite it. Continue?",
                "Overwrite?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;
        }

        Pet pet = _cboPetType.SelectedIndex == 0
            ? new CatPet(petName)
            : new DogPet(petName);

        var gm = new GameManager(pet) { UserName = userName };
        SaveManager.Save(gm);
        OpenDashboard(gm);
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_cboProfiles.SelectedItem is not string username) return;

        var result = MessageBox.Show(
            $"Permanently delete \"{username}\"?\nAll progress will be lost.",
            "Delete profile", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (result != DialogResult.Yes) return;

        SaveManager.DeleteProfile(username);
        LoadProfiles();
    }

    // ─────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────
    private void OpenDashboard(GameManager gm)
    {
        var dashboard = new DashboardForm(gm);
        dashboard.Show();
        Hide();
    }

    private static Label MakeSmallLabel(string text, Point loc) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
        ForeColor = PawTheme.TextMuted,
        AutoSize = true,
        Location = loc,
        BackColor = Color.Transparent,
    };

    private static TextBox MakeTextBox(Point loc, int width) => new()
    {
        Location = loc,
        Width = width,
        Height = 28,
        BackColor = PawTheme.Background,
        ForeColor = PawTheme.TextDark,
        BorderStyle = BorderStyle.FixedSingle,
        Font = PawTheme.FontBody,
    };

    private static void PaintCardBorder(object? sender, PaintEventArgs e)
    {
        if (sender is not Panel p) return;
        using var pen = new Pen(PawTheme.CardBorder, 2f);
        e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
    }
}
