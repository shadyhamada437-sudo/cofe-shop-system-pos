using System.Reflection;

namespace CoffeeShopPOS.Helpers;

/// <summary>
/// Helper class for creating pre-styled UI controls used across the application.
/// </summary>
public static class UIHelper
{
    /// <summary>
    /// Creates a flat styled button with an icon-style appearance.
    /// </summary>
    public static Button CreateIconButton(string text, Color bgColor)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = bgColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = ThemeManager.ButtonFont,
            Cursor = Cursors.Hand,
            Padding = new Padding(12, 8, 12, 8),
            AutoSize = false,
            Height = 40,
            MinimumSize = new Size(100, 40),
            TextAlign = ContentAlignment.MiddleCenter
        };

        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bgColor, 0.15f);
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(bgColor, 0.1f);

        return btn;
    }

    /// <summary>
    /// Creates a KPI stat card panel with a colored left border, a large value, and a small title.
    /// </summary>
    public static Panel CreateStatCard(string title, string value, Color accentColor)
    {
        var card = new Panel
        {
            Width = 220,
            Height = 100,
            BackColor = ThemeManager.CardBackground,
            Padding = new Padding(0),
            Margin = new Padding(8)
        };

        // Colored left border
        var leftBorder = new Panel
        {
            Width = 5,
            Dock = DockStyle.Left,
            BackColor = accentColor
        };

        // Value label (large)
        var valueLabel = new Label
        {
            Text = value,
            Font = ThemeManager.LargeValueFont,
            ForeColor = ThemeManager.MainText,
            AutoSize = false,
            TextAlign = ContentAlignment.BottomLeft,
            Dock = DockStyle.None,
            Location = new Point(20, 15),
            Size = new Size(185, 40)
        };

        // Title label (small)
        var titleLabel = new Label
        {
            Text = title,
            Font = ThemeManager.SmallFont,
            ForeColor = ThemeManager.SecondaryText,
            AutoSize = false,
            TextAlign = ContentAlignment.TopLeft,
            Dock = DockStyle.None,
            Location = new Point(20, 60),
            Size = new Size(185, 25)
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        card.Controls.Add(leftBorder);

        // Paint subtle border
        card.Paint += (s, e) =>
        {
            var rect = card.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            using var pen = new Pen(Color.FromArgb(40, 0, 0, 0), 1);
            e.Graphics.DrawRectangle(pen, rect);
        };

        return card;
    }

    /// <summary>
    /// Creates a search text box with placeholder text behavior.
    /// Gray placeholder text disappears on focus and reappears when empty on blur.
    /// </summary>
    public static TextBox CreateSearchBox(string placeholder)
    {
        var txt = new TextBox
        {
            Font = ThemeManager.BodyFont,
            ForeColor = ThemeManager.SecondaryText,
            BackColor = ThemeManager.CardBackground,
            BorderStyle = BorderStyle.FixedSingle,
            Text = placeholder,
            Tag = placeholder,
            Height = 32,
            Width = 250
        };

        txt.GotFocus += (s, e) =>
        {
            if (txt.Text == (string)txt.Tag)
            {
                txt.Text = string.Empty;
                txt.ForeColor = ThemeManager.MainText;
            }
        };

        txt.LostFocus += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.Text = (string)txt.Tag;
                txt.ForeColor = ThemeManager.SecondaryText;
            }
        };

        return txt;
    }

    /// <summary>
    /// Enables double-buffering on any control via reflection to reduce flicker.
    /// </summary>
    public static void SetDoubleBuffered(Control ctrl)
    {
        typeof(Control)
            .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?
            .SetValue(ctrl, true, null);
    }
}
