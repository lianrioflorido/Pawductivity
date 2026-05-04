using Pawductivity.Managers;
using Pawductivity.Models;

namespace Pawductivity.Forms;

public class ShopForm : Form
{
    private readonly GameManager _gm;     // Handles game logic (coins, items, buying)
    private readonly Action _onBuy;       // Callback after successful purchase
    private Label _lblCoins = null!;      // Label showing player's coins

    public ShopForm(GameManager gm, Action onBuy)
    {
        _gm = gm;          // store GameManager reference
        _onBuy = onBuy;    // store callback
        InitializeComponent(); // build UI
    }

    private void InitializeComponent()
    {
        // =========================
        // FORM SETTINGS (WINDOW)
        // =========================
        Text = "🛍️ Pawductivity Shop";
        Size = new Size(540, 580);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = PawTheme.Background;

        // =========================
        // TITLE LABEL (HEADER)
        // =========================
        var title = new Label
        {
            Text = "🛍️ Shop",
            Font = PawTheme.FontTitle,
            ForeColor = PawTheme.Primary,
            AutoSize = true,
            Location = new Point(20, 10),
            BackColor = Color.Transparent,
        };

        // =========================
        // COINS LABEL (TOP DISPLAY)
        // =========================
        _lblCoins = new Label
        {
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = PawTheme.Primary,
            AutoSize = true,
            Location = new Point(20, 60),
            BackColor = Color.Transparent,
        };

        // Add title + coins to form
        Controls.AddRange([title, _lblCoins]);

        // Set initial coin text
        UpdateCoinsLabel();

        // =========================
        // SHOP ITEM LIST GENERATION
        // =========================
        int y = 90; // starting Y position for item cards

        foreach (var item in _gm.ShopItems)
        {
            // create UI card for each item
            var card = BuildItemCard(item, y);
            Controls.Add(card);

            y += 72; // move next card down
        }
    }

    // =========================
    // CREATE SINGLE ITEM CARD
    // =========================
    private Panel BuildItemCard(ShopItem item, int y)
    {
        // =========================
        // CARD CONTAINER PANEL
        // =========================
        var card = new Panel
        {
            Location = new Point(20, y),
            Size = new Size(480, 62),
            BackColor = PawTheme.Surface,
        };

        // Draw border around card
        card.Paint += (s, e) =>
        {
            using var pen = new Pen(PawTheme.CardBorder, 1.5f);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        // =========================
        // ITEM EMOJI LABEL
        // =========================
        var lblEmoji = new Label
        {
            Text = item.Emoji,
            Font = new Font("Segoe UI Emoji", 22f),
            AutoSize = true,
            Location = new Point(3, 5),
            BackColor = Color.Transparent,
        };

        // =========================
        // ITEM NAME LABEL
        // =========================
        var lblName = new Label
        {
            Text = item.Name,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = PawTheme.TextDark,
            AutoSize = true,
            Location = new Point(75, 8),
            BackColor = Color.Transparent,
        };

        // =========================
        // ITEM DESCRIPTION LABEL
        // =========================
        var lblDesc = new Label
        {
            Text = $"{item.Description}   ❤️+{item.HealthBoost}  😸+{item.MoodBoost}",
            Font = PawTheme.FontSmall,
            ForeColor = PawTheme.TextMuted,
            AutoSize = true,
            Location = new Point(75, 30),
            BackColor = Color.Transparent,
        };

        // =========================
        // BUY BUTTON
        // =========================
        var btnBuy = new Button
        {
            Text = $"🪙 {item.Cost}",
            Location = new Point(380, 14),
            Width = 85,
        };

        PawTheme.StyleButton(btnBuy);

        // =========================
        // BUY BUTTON CLICK LOGIC
        // =========================
        btnBuy.Click += (s, e) =>
        {
            // try buying item
            if (_gm.BuyItem(item))
            {
                MessageBox.Show(
                    $"You bought {item.Emoji} {item.Name}!\nYour pet loves it! 💕",
                    "Yay!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                _onBuy();              // trigger callback
                UpdateCoinsLabel();    // refresh coins
            }
            else
            {
                MessageBox.Show(
                    "Not enough coins! 🪙 Complete more tasks to earn coins.",
                    "Oops!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        };

        // =========================
        // ADD ELEMENTS TO CARD
        // =========================
        card.Controls.AddRange([lblEmoji, lblName, lblDesc, btnBuy]);

        return card;
    }

    // =========================
    // UPDATE COINS DISPLAY
    // =========================
    private void UpdateCoinsLabel() =>
        _lblCoins.Text = $"🪙 Your coins: {_gm.Pet.Coins}";
}