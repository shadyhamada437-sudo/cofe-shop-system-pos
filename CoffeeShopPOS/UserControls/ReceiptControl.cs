using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using CoffeeShopPOS.Dialogs;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.UserControls;

/// <summary>
/// Billing / Receipts screen: pick a transaction, view a receipt-style panel and print-preview it.
/// The receipt is drawn by a single renderer shared by the on-screen panel and the print document.
/// </summary>
public partial class ReceiptControl : UserControl
{
    private const int ReceiptWidth = 320;
    private const int ReceiptPad = 16;

    private enum Align { Left, Center, Right, Split, Separator }

    private sealed record Line(string Left, string Right, Font Font, Align Align, float SpaceAfter = 0);

    private readonly List<Sale> _sales = MockData.GetSales();
    private readonly ComboBox _selector;
    private readonly Panel _scrollHost;
    private readonly Panel _receiptPanel;
    private Sale? _current;

    private readonly Font _shopFont = new("Segoe UI", 14f, FontStyle.Bold);
    private readonly Font _smallFont = new("Consolas", 8f);
    private readonly Font _bodyFont = new("Consolas", 9f);
    private readonly Font _bodyBold = new("Consolas", 9f, FontStyle.Bold);
    private readonly Font _totalFont = new("Consolas", 12f, FontStyle.Bold);
    private readonly Font _thanksFont = new("Segoe UI", 9f, FontStyle.Bold);

    public ReceiptControl() : this(null)
    {
    }

    public ReceiptControl(Sale? initialSale)
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;
        Padding = new Padding(20);

        // ── Top bar ──
        var topBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = ThemeManager.MainBackground };
        var title = new Label
        {
            Text = "🧾 Billing / Receipts", Font = ThemeManager.SubHeaderFont, ForeColor = ThemeManager.MainText,
            AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft
        };
        var right = new FlowLayoutPanel
        {
            Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent,
            Padding = new Padding(0, 8, 0, 0)
        };
        _selector = new ComboBox { Width = 280, Margin = new Padding(0, 4, 10, 0) };
        ThemeManager.StyleComboBox(_selector);
        var btnPreview = UIHelper.CreateIconButton("🖨 Print Preview", ThemeManager.Primary);
        btnPreview.MinimumSize = new Size(140, 40);
        btnPreview.Margin = new Padding(0);
        btnPreview.Click += (s, e) => ShowPrintPreview();
        right.Controls.AddRange(new Control[] { _selector, btnPreview });
        topBar.Controls.Add(right);
        topBar.Controls.Add(title);

        // ── Scrollable host with centered receipt ──
        _scrollHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ThemeManager.MainBackground };
        _receiptPanel = new Panel { Width = ReceiptWidth, Height = 400, BackColor = Color.White };
        UIHelper.SetDoubleBuffered(_receiptPanel);
        _receiptPanel.Paint += OnReceiptPaint;
        _scrollHost.Controls.Add(_receiptPanel);
        _scrollHost.Resize += (s, e) => CenterReceipt();

        Controls.Add(_scrollHost);
        Controls.Add(topBar);

        // ── Data ──
        int targetIndex = 0;
        int idx = 0;
        foreach (var s in _sales.OrderByDescending(x => x.DateTime))
        {
            _selector.Items.Add(new SaleItemRef(s));
            if (initialSale != null && s.Id == initialSale.Id)
                targetIndex = idx;
            idx++;
        }
        _selector.SelectedIndexChanged += (s, e) =>
        {
            _current = (_selector.SelectedItem as SaleItemRef)?.Sale;
            LoadReceipt();
        };
        if (_selector.Items.Count > 0) _selector.SelectedIndex = targetIndex;
        else LoadReceipt();
    }

    private sealed class SaleItemRef
    {
        public Sale Sale { get; }
        public SaleItemRef(Sale sale) => Sale = sale;
        public override string ToString() => $"{Sale.TransactionId}  -  {Sale.DateTime:yyyy-MM-dd HH:mm}";
    }

    private void LoadReceipt()
    {
        float h = RenderReceipt(null, ReceiptWidth);
        _receiptPanel.Height = (int)Math.Ceiling(h);
        CenterReceipt();
        _receiptPanel.Invalidate();
    }

    private void CenterReceipt()
    {
        int x = Math.Max(10, (_scrollHost.ClientSize.Width - _receiptPanel.Width) / 2);
        _receiptPanel.Location = new Point(x + _scrollHost.AutoScrollPosition.X, 10 + _scrollHost.AutoScrollPosition.Y);
        _scrollHost.AutoScrollMinSize = new Size(0, _receiptPanel.Height + 30);
    }

    private void OnReceiptPaint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        RenderReceipt(e.Graphics, ReceiptWidth);
        using var pen = new Pen(Color.FromArgb(200, 190, 175));
        e.Graphics.DrawRectangle(pen, 0, 0, _receiptPanel.Width - 1, _receiptPanel.Height - 1);
    }

    // ───────────── Receipt content ─────────────

    private List<Line> BuildLines()
    {
        var cfg = MockData.Settings;
        var lines = new List<Line>
        {
            new($"☕ {cfg.ShopName}", "", _shopFont, Align.Center, 2),
            new(cfg.Address, "", _smallFont, Align.Center),
            new($"Tel: {cfg.Phone}", "", _smallFont, Align.Center, 4),
            new("", "", _bodyFont, Align.Separator)
        };

        if (_current == null)
        {
            lines.Add(new("No transaction selected", "", _bodyFont, Align.Center, 20));
            return lines;
        }

        var s = _current;
        lines.Add(new($"Receipt #: {s.TransactionId}", "", _bodyFont, Align.Left));
        lines.Add(new($"Date/Time: {s.DateTime:yyyy-MM-dd HH:mm}", "", _bodyFont, Align.Left));
        lines.Add(new($"Cashier: {s.CashierName}", "", _bodyFont, Align.Left));
        lines.Add(new($"Order Type: {SaleDetailsDialog.FormatOrderType(s.OrderType)}", "", _bodyFont, Align.Left));
        if (s.Status != SaleStatus.Completed)
            lines.Add(new($"Status: {s.Status.ToString().ToUpper()}", "", _bodyBold, Align.Left));
        lines.Add(new("", "", _bodyFont, Align.Separator));

        foreach (var item in s.Items)
            lines.Add(new(item.ProductName, $"x{item.Quantity}  {item.LineTotal:C2}", _bodyFont, Align.Split));
        lines.Add(new("", "", _bodyFont, Align.Separator));

        lines.Add(new("Subtotal:", s.Subtotal.ToString("C2"), _bodyFont, Align.Split));
        lines.Add(new("Discount:", "-" + s.Discount.ToString("C2"), _bodyFont, Align.Split));
        lines.Add(new($"Tax ({s.TaxRate:0.##}%):", s.Tax.ToString("C2"), _bodyFont, Align.Split, 4));
        lines.Add(new("TOTAL:", s.Total.ToString("C2"), _totalFont, Align.Split));
        lines.Add(new("", "", _bodyFont, Align.Separator));

        lines.Add(new("Payment:", s.PaymentMethod.ToString(), _bodyFont, Align.Split));
        lines.Add(new("Amount Received:", s.AmountReceived.ToString("C2"), _bodyFont, Align.Split));
        lines.Add(new("Change Due:", s.ChangeDue.ToString("C2"), _bodyFont, Align.Split));
        lines.Add(new("", "", _bodyFont, Align.Separator));

        lines.Add(new("Thank you for visiting Brew Haven! ☕", "", _thanksFont, Align.Center, 2));
        lines.Add(new("Have a wonderful day!", "", _smallFont, Align.Center, 6));
        return lines;
    }

    /// <summary>
    /// Renders (or only measures, when g is null) the receipt into the given width. Returns total height.
    /// </summary>
    private float RenderReceipt(Graphics? g, float width)
    {
        Graphics? measure = null;
        if (g == null) { measure = Graphics.FromHwnd(IntPtr.Zero); g = measure; }
        bool draw = measure == null;

        float y = ReceiptPad;
        float innerW = width - 2 * ReceiptPad;
        using var brush = new SolidBrush(ThemeManager.MainText);
        using var sepPen = new Pen(ThemeManager.SecondaryText) { DashStyle = DashStyle.Dash };

        foreach (var line in BuildLines())
        {
            float lineH = TextRenderer.MeasureText(g, "Ag", line.Font, Size.Empty, TextFormatFlags.NoPadding).Height + 3;

            switch (line.Align)
            {
                case Align.Separator:
                    if (draw) g.DrawLine(sepPen, ReceiptPad, y + 6, width - ReceiptPad, y + 6);
                    lineH = 12;
                    break;
                case Align.Split:
                {
                    var rightSize = g.MeasureString(line.Right, line.Font);
                    if (draw)
                    {
                        var nameRect = new RectangleF(ReceiptPad, y, innerW - rightSize.Width, lineH);
                        using var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                        g.DrawString(line.Left, line.Font, brush, nameRect, fmt);
                        using var rf = new StringFormat { Alignment = StringAlignment.Far, FormatFlags = StringFormatFlags.NoWrap };
                        g.DrawString(line.Right, line.Font, brush, new RectangleF(ReceiptPad, y, innerW, lineH), rf);
                    }
                    break;
                }
                default:
                {
                    var measured = g.MeasureString(line.Left, line.Font, (int)innerW);
                    lineH = Math.Max(lineH, measured.Height);
                    if (draw)
                    {
                        using var fmt = new StringFormat
                        {
                            Alignment = line.Align == Align.Center ? StringAlignment.Center
                                      : line.Align == Align.Right ? StringAlignment.Far : StringAlignment.Near
                        };
                        g.DrawString(line.Left, line.Font, brush, new RectangleF(ReceiptPad, y, innerW, lineH), fmt);
                    }
                    break;
                }
            }
            y += lineH + line.SpaceAfter;
        }

        measure?.Dispose();
        return y + ReceiptPad;
    }

    // ───────────── Printing ─────────────

    private void ShowPrintPreview()
    {
        if (_current == null)
        {
            MessageBox.Show("Please select a transaction first.", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            using var doc = new PrintDocument { DocumentName = $"Receipt {_current.TransactionId}" };
            doc.PrintPage += (s, e) =>
            {
                var g = e.Graphics!;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var state = g.Save();
                g.TranslateTransform(e.MarginBounds.Left, e.MarginBounds.Top);
                g.ScaleTransform(100f / 96f, 100f / 96f);
                RenderReceipt(g, ReceiptWidth);
                g.Restore(state);
                e.HasMorePages = false;
            };

            using var preview = new PrintPreviewDialog
            {
                Document = doc,
                Width = 700,
                Height = 800,
                StartPosition = FormStartPosition.CenterParent
            };
            preview.ShowDialog(FindForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to show print preview: {ex.Message}", "Print Preview", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _shopFont.Dispose(); _smallFont.Dispose(); _bodyFont.Dispose();
            _bodyBold.Dispose(); _totalFont.Dispose(); _thanksFont.Dispose();
        }
        base.Dispose(disposing);
    }
}

