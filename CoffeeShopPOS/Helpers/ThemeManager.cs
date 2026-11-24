using System.Drawing.Drawing2D;

namespace CoffeeShopPOS.Helpers;

/// <summary>
/// Central theme manager that defines all colors, fonts, and control styling
/// methods used throughout the Coffee Shop POS application.
/// </summary>
public static class ThemeManager
{
    // ───────────── Colors ─────────────
    public static readonly Color MainBackground  = ColorTranslator.FromHtml("#F7F4EF");
    public static readonly Color Sidebar          = ColorTranslator.FromHtml("#28211D");
    public static readonly Color Primary          = ColorTranslator.FromHtml("#8B5E3C");
    public static readonly Color Secondary        = ColorTranslator.FromHtml("#C49A6C");
    public static readonly Color Accent           = ColorTranslator.FromHtml("#D9B382");
    public static readonly Color CardBackground   = ColorTranslator.FromHtml("#FFFFFF");
    public static readonly Color MainText         = ColorTranslator.FromHtml("#29231F");
    public static readonly Color SecondaryText    = ColorTranslator.FromHtml("#81766E");
    public static readonly Color Success          = ColorTranslator.FromHtml("#2E8B57");
    public static readonly Color Warning          = ColorTranslator.FromHtml("#D99A27");
    public static readonly Color Danger           = ColorTranslator.FromHtml("#C94C4C");

    // ───────────── Fonts ─────────────
    private const string FontFamily = "Segoe UI";

    public static readonly Font HeaderFont      = new(FontFamily, 20F, FontStyle.Bold);
    public static readonly Font SubHeaderFont   = new(FontFamily, 16F, FontStyle.Bold);
    public static readonly Font TitleFont        = new(FontFamily, 14F, FontStyle.Bold);
    public static readonly Font BodyFont         = new(FontFamily, 11F, FontStyle.Regular);
    public static readonly Font BodyBoldFont     = new(FontFamily, 11F, FontStyle.Bold);
    public static readonly Font SmallFont        = new(FontFamily, 9F, FontStyle.Regular);
    public static readonly Font SmallBoldFont    = new(FontFamily, 9F, FontStyle.Bold);
    public static readonly Font ButtonFont       = new(FontFamily, 10F, FontStyle.Bold);
    public static readonly Font LargeValueFont   = new(FontFamily, 24F, FontStyle.Bold);
    public static readonly Font SidebarFont      = new(FontFamily, 11F, FontStyle.Regular);
    public static readonly Font SidebarBoldFont  = new(FontFamily, 11F, FontStyle.Bold);

    // ───────────── Control Styling ─────────────

    /// <summary>Style a button with primary or default appearance.</summary>
    public static void StyleButton(Button btn, bool isPrimary = false)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor = Cursors.Hand;
        btn.Font = ButtonFont;
        btn.Padding = new Padding(12, 6, 12, 6);

        if (isPrimary)
        {
            btn.BackColor = Primary;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(Primary, 0.1f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(Primary, 0.2f);
        }
        else
        {
            btn.BackColor = CardBackground;
            btn.ForeColor = MainText;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Secondary;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 240, 233);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(235, 228, 218);
        }
    }

    /// <summary>Style a panel with the main background color.</summary>
    public static void StylePanel(Panel panel)
    {
        panel.BackColor = MainBackground;
        panel.ForeColor = MainText;
        panel.Font = BodyFont;
    }

    /// <summary>Style a DataGridView to match the application theme.</summary>
    public static void StyleDataGridView(DataGridView dgv)
    {
        dgv.BackgroundColor = CardBackground;
        dgv.BorderStyle = BorderStyle.None;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor = Color.FromArgb(230, 225, 218);
        dgv.Font = BodyFont;
        dgv.ForeColor = MainText;

        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = BodyBoldFont;
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primary;
        dgv.ColumnHeadersHeight = 40;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        dgv.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 218, 196);
        dgv.DefaultCellStyle.SelectionForeColor = MainText;

        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 247, 242);

        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.RowTemplate.Height = 38;
    }

    /// <summary>Style a text box to match the application theme.</summary>
    public static void StyleTextBox(TextBox txt)
    {
        txt.Font = BodyFont;
        txt.ForeColor = MainText;
        txt.BackColor = CardBackground;
        txt.BorderStyle = BorderStyle.FixedSingle;
    }

    /// <summary>Style a combo box to match the application theme.</summary>
    public static void StyleComboBox(ComboBox cmb)
    {
        cmb.Font = BodyFont;
        cmb.ForeColor = MainText;
        cmb.BackColor = CardBackground;
        cmb.FlatStyle = FlatStyle.Flat;
        cmb.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    /// <summary>Style a panel as a card with white background, padding, and subtle shadow feel.</summary>
    public static void StyleCard(Panel panel)
    {
        panel.BackColor = CardBackground;
        panel.ForeColor = MainText;
        panel.Font = BodyFont;
        panel.Padding = new Padding(16);
        panel.Margin = new Padding(8);

        // Remove existing paint handlers to avoid stacking
        panel.Paint += (s, e) =>
        {
            var rect = panel.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            using var pen = new Pen(Color.FromArgb(60, 0, 0, 0), 1);
            using var path = CreateRoundedRegion(rect, 8);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawPath(pen, path);
        };
    }

    /// <summary>Create a rounded rectangle GraphicsPath.</summary>
    public static GraphicsPath CreateRoundedRegion(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int diameter = radius * 2;

        var arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));

        // Top-left
        path.AddArc(arcRect, 180, 90);

        // Top-right
        arcRect.X = rect.Right - diameter;
        path.AddArc(arcRect, 270, 90);

        // Bottom-right
        arcRect.Y = rect.Bottom - diameter;
        path.AddArc(arcRect, 0, 90);

        // Bottom-left
        arcRect.X = rect.Left;
        path.AddArc(arcRect, 90, 90);

        path.CloseFigure();
        return path;
    }
}
