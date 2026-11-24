using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.Dialogs;

/// <summary>
/// Modal dialog showing full details of a sale transaction.
/// </summary>
public partial class SaleDetailsDialog : Form
{
    private readonly Sale _sale;

    public SaleDetailsDialog(Sale sale)
    {
        _sale = sale;

        Text = $"Sale Details - {sale.TransactionId}";
        Size = new Size(500, 640);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;

        int x = 20, w = 440, y = 15;

        // ── Header ──
        var title = new Label
        {
            Text = sale.TransactionId, Font = ThemeManager.TitleFont, ForeColor = ThemeManager.Primary,
            AutoSize = true, Location = new Point(x, y)
        };
        Color statusColor = StatusColor(sale.Status);
        var status = new Label
        {
            Text = sale.Status.ToString().ToUpper(), Font = ThemeManager.SmallBoldFont,
            ForeColor = Color.White, BackColor = statusColor, AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter, Size = new Size(100, 26),
            Location = new Point(x + w - 100, y)
        };
        Controls.Add(title);
        Controls.Add(status);
        y += 38;

        y = AddInfoRow("Date:", sale.DateTime.ToString("yyyy-MM-dd HH:mm"), x, y);
        y = AddInfoRow("Cashier:", sale.CashierName, x, y);
        y = AddInfoRow("Order Type:", FormatOrderType(sale.OrderType), x, y);
        y += 8;

        // ── Items grid ──
        var grid = new DataGridView { Location = new Point(x, y), Size = new Size(w, 170) };
        ThemeManager.StyleDataGridView(grid);
        grid.RowTemplate.Height = 30;
        grid.ColumnHeadersHeight = 34;
        grid.Columns.Add("Product", "Product");
        grid.Columns.Add("Qty", "Qty");
        grid.Columns.Add("Unit", "Unit Price");
        grid.Columns.Add("Line", "Line Total");
        grid.Columns["Product"]!.FillWeight = 140;
        grid.Columns["Qty"]!.FillWeight = 40;
        foreach (var n in new[] { "Qty", "Unit", "Line" })
            grid.Columns[n]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        foreach (var item in sale.Items)
            grid.Rows.Add(item.ProductName, item.Quantity, item.UnitPrice.ToString("C2"), item.LineTotal.ToString("C2"));
        grid.ClearSelection();
        Controls.Add(grid);
        y += 180;

        // ── Summary ──
        y = AddSummaryRow("Subtotal:", sale.Subtotal.ToString("C2"), x, y, w, false);
        y = AddSummaryRow("Discount:", "-" + sale.Discount.ToString("C2"), x, y, w, false);
        y = AddSummaryRow($"Tax ({sale.TaxRate:0.##}%):", sale.Tax.ToString("C2"), x, y, w, false);
        y = AddSummaryRow("Total:", sale.Total.ToString("C2"), x, y, w, true);
        y += 6;

        // ── Payment info ──
        y = AddInfoRow("Payment Method:", sale.PaymentMethod.ToString(), x, y);
        y = AddInfoRow("Amount Received:", sale.AmountReceived.ToString("C2"), x, y);
        y = AddInfoRow("Change Due:", sale.ChangeDue.ToString("C2"), x, y);
        y += 10;

        // ── Buttons ──
        var btnPrint = new Button { Text = "🖨 Print Receipt", Size = new Size(150, 38), Location = new Point(x + w - 310, y) };
        ThemeManager.StyleButton(btnPrint, true);
        btnPrint.Click += (s, e) => MessageBox.Show(this,
            $"Receipt for {_sale.TransactionId} sent to printer.\n(Use Billing / Receipts for print preview.)",
            "Print Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);

        var btnClose = new Button { Text = "Close", Size = new Size(140, 38), Location = new Point(x + w - 140, y), DialogResult = DialogResult.Cancel };
        ThemeManager.StyleButton(btnClose);
        Controls.Add(btnPrint);
        Controls.Add(btnClose);
        CancelButton = btnClose;

        ClientSize = new Size(500, y + 38 + 20);
    }

    private int AddInfoRow(string label, string value, int x, int y)
    {
        Controls.Add(new Label
        {
            Text = label, Font = ThemeManager.SmallBoldFont, ForeColor = ThemeManager.SecondaryText,
            AutoSize = false, Size = new Size(140, 22), Location = new Point(x, y), TextAlign = ContentAlignment.MiddleLeft
        });
        Controls.Add(new Label
        {
            Text = value, Font = ThemeManager.BodyFont, ForeColor = ThemeManager.MainText,
            AutoSize = false, Size = new Size(290, 22), Location = new Point(x + 145, y), TextAlign = ContentAlignment.MiddleLeft
        });
        return y + 24;
    }

    private int AddSummaryRow(string label, string value, int x, int y, int w, bool bold)
    {
        var font = bold ? new Font("Segoe UI", 12f, FontStyle.Bold) : ThemeManager.BodyFont;
        Controls.Add(new Label
        {
            Text = label, Font = font, ForeColor = ThemeManager.MainText, AutoSize = false,
            Size = new Size(w / 2, 26), Location = new Point(x + w / 2 - 20, y), TextAlign = ContentAlignment.MiddleRight
        });
        Controls.Add(new Label
        {
            Text = value, Font = font, ForeColor = bold ? ThemeManager.Primary : ThemeManager.MainText, AutoSize = false,
            Size = new Size(w / 2 - 40, 26), Location = new Point(x + w - (w / 2 - 40), y), TextAlign = ContentAlignment.MiddleRight
        });
        return y + 26;
    }

    internal static Color StatusColor(SaleStatus status) => status switch
    {
        SaleStatus.Completed => ThemeManager.Success,
        SaleStatus.Cancelled => ThemeManager.Danger,
        SaleStatus.Refunded => ThemeManager.Warning,
        _ => ThemeManager.SecondaryText
    };

    internal static string FormatOrderType(OrderType t) => t == OrderType.DineIn ? "Dine In" : "Takeaway";
}

