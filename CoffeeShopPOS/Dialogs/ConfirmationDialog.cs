using CoffeeShopPOS.Helpers;

namespace CoffeeShopPOS.Dialogs;

/// <summary>
/// A reusable confirmation dialog with ⚠️ icon, message, Confirm and Cancel buttons.
/// Use the static Show() method for convenience.
/// </summary>
public partial class ConfirmationDialog : Form
{
    private readonly Label _iconLabel;
    private readonly Label _messageLabel;
    private readonly Button _confirmButton;
    private readonly Button _cancelButton;

    public ConfirmationDialog(string message, string title = "Confirm", bool isDanger = true)
    {
        // ── Form settings ──
        Text = title;
        Size = new Size(380, 200);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;

        // ── Icon ──
        _iconLabel = new Label
        {
            Text = "⚠️",
            Font = new Font("Segoe UI", 28f),
            AutoSize = true,
            Location = new Point(30, 22)
        };

        // ── Message ──
        _messageLabel = new Label
        {
            Text = message,
            Font = ThemeManager.BodyFont,
            ForeColor = ThemeManager.MainText,
            AutoSize = false,
            Size = new Size(250, 60),
            Location = new Point(90, 28),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // ── Buttons ──
        _confirmButton = new Button
        {
            Text = "Confirm",
            Size = new Size(100, 38),
            Location = new Point(150, 110),
            DialogResult = DialogResult.OK
        };
        if (isDanger)
        {
            _confirmButton.BackColor = ThemeManager.Danger;
            _confirmButton.ForeColor = Color.White;
            _confirmButton.FlatStyle = FlatStyle.Flat;
            _confirmButton.FlatAppearance.BorderSize = 0;
            _confirmButton.Font = ThemeManager.ButtonFont;
            _confirmButton.Cursor = Cursors.Hand;
        }
        else
        {
            ThemeManager.StyleButton(_confirmButton, true);
        }

        _cancelButton = new Button
        {
            Text = "Cancel",
            Size = new Size(100, 38),
            Location = new Point(260, 110),
            DialogResult = DialogResult.Cancel
        };
        ThemeManager.StyleButton(_cancelButton);

        AcceptButton = _confirmButton;
        CancelButton = _cancelButton;

        Controls.Add(_iconLabel);
        Controls.Add(_messageLabel);
        Controls.Add(_confirmButton);
        Controls.Add(_cancelButton);
    }

    /// <summary>
    /// Convenience static method to show a confirmation dialog and return the result.
    /// </summary>
    public static DialogResult Show(string message, string title = "Confirm")
    {
        using var dlg = new ConfirmationDialog(message, title);
        return dlg.ShowDialog();
    }
}

