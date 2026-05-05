using Pawductivity.Models;

namespace Pawductivity.Forms;

/// <summary>
/// Reusable dialog for both adding and editing tasks.
/// </summary>
public class TaskEditForm : Form
{
    public TaskItem? Result { get; private set; }

    private TextBox    _txtTitle = null!;
    private TextBox    _txtDesc  = null!;
    private ComboBox   _cboPriority = null!;
    private DateTimePicker _dtpDue = null!;
    private Button     _btnSave   = null!;
    private Button     _btnCancel = null!;

    private readonly TaskItem? _editTarget;

    public TaskEditForm(TaskItem? existing = null)
    {
        _editTarget = existing;
        InitializeComponent();
        if (existing is not null) PopulateExisting(existing);
    }

    private void InitializeComponent()
    {
        Text            = _editTarget is null ? "Add New Task 🌸" : "Edit Task ✏️";
        Size            = new Size(400, 340);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        BackColor       = PawTheme.Background;

        int y = 15;
        Controls.Add(MakeLabel("Task Title *", new Point(20, y)));
        _txtTitle = MakeTextBox(new Point(20, y + 20), 340);
        _txtTitle.PlaceholderText = "What do you need to do?";
        Controls.Add(_txtTitle);

        y += 65;
        Controls.Add(MakeLabel("Description (optional)", new Point(20, y)));
        _txtDesc = new TextBox
        {
            Location    = new Point(20, y + 20),
            Size        = new Size(340, 55),
            Multiline   = true,
            BackColor   = PawTheme.Surface,
            ForeColor   = PawTheme.TextDark,
            Font        = PawTheme.FontBody,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Any extra details...",
        };
        Controls.Add(_txtDesc);

        y += 92;
        Controls.Add(MakeLabel("Priority", new Point(20, y)));
        _cboPriority = new ComboBox
        {
            Location      = new Point(20, y + 20),
            Width         = 160,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor     = PawTheme.Surface,
            ForeColor     = PawTheme.TextDark,
            Font          = PawTheme.FontBody,
        };
        _cboPriority.Items.AddRange(["🟢 Low", "🟡 Medium", "🔴 High"]);
        _cboPriority.SelectedIndex = 1;
        Controls.Add(_cboPriority);

        Controls.Add(MakeLabel("Due Date", new Point(200, y)));
        _dtpDue = new DateTimePicker
        {
            Location  = new Point(200, y + 20),
            Width     = 160,
            Font      = PawTheme.FontBody,
            MinDate   = DateTime.Today,
            Value     = DateTime.Today,
            Format    = DateTimePickerFormat.Short,
        };
        Controls.Add(_dtpDue);

        y += 72;
        _btnSave = new Button
        {
            Text     = _editTarget is null ? "Add Task 🌸" : "Save Changes ✅",
            Location = new Point(20, y),
            Width    = 165,
        };
        _btnCancel = new Button
        {
            Text     = "Cancel",
            Location = new Point(195, y),
            Width    = 165,
        };
        PawTheme.StyleButton(_btnSave);
        PawTheme.StyleButton(_btnCancel, outlined: true);

        _btnSave.Click   += BtnSave_Click;
        _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.AddRange([_btnSave, _btnCancel]);
    }

    private void PopulateExisting(TaskItem t)
    {
        _txtTitle.Text           = t.Title;
        _txtDesc.Text            = t.Description;
        _cboPriority.SelectedIndex = (int)t.Priority;
        _dtpDue.Value            = t.DueDate;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtTitle.Text))
        {
            MessageBox.Show("Please enter a task title! 🌸", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Result = new TaskItem
        {
            Title       = _txtTitle.Text.Trim(),
            Description = _txtDesc.Text.Trim(),
            Priority    = (TaskPriority)_cboPriority.SelectedIndex,
            DueDate     = _dtpDue.Value.Date,
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private static Label MakeLabel(string text, Point loc) => new()
    {
        Text      = text,
        Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
        ForeColor = PawTheme.TextMuted,
        AutoSize  = true,
        Location  = loc,
        BackColor = Color.Transparent,
    };

    private static TextBox MakeTextBox(Point loc, int width) => new()
    {
        Location    = loc,
        Width       = width,
        Height      = 28,
        BackColor   = PawTheme.Surface,
        ForeColor   = PawTheme.TextDark,
        Font        = PawTheme.FontBody,
        BorderStyle = BorderStyle.FixedSingle,
    };
}
