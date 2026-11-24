using CoffeeShopPOS.Dialogs;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.UserControls;

/// <summary>
/// Sales / Transactions screen with combined filters, status colouring, details and cancel.
/// </summary>
public partial class SalesControl : UserControl
{
    private readonly List<Sale> _sales = MockData.GetSales();

    private readonly TextBox _searchBox;
    private readonly DateTimePicker _from;
    private readonly DateTimePicker _to;
    private readonly ComboBox _payment;
    private readonly ComboBox _status;
    private readonly ComboBox _orderType;
    private readonly DataGridView _grid;

    public SalesControl()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;
        Padding = new Padding(20);

        // ── Top bar ──
        var topBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = ThemeManager.MainBackground };
        var title = new Label
        {
            Text = "📋 Sales / Transactions", Font = ThemeManager.SubHeaderFont, ForeColor = ThemeManager.MainText,
            AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft
        };
        var right = new FlowLayoutPanel
        {
            Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent,
            Padding = new Padding(0, 8, 0, 0)
        };
        _searchBox = UIHelper.CreateSearchBox("🔍 Search transaction ID...");
        _searchBox.Width = 220;
        _searchBox.Margin = new Padding(0, 4, 10, 0);
        var btnRefresh = UIHelper.CreateIconButton("🔄 Refresh", ThemeManager.Primary);
        btnRefresh.Margin = new Padding(0);
        btnRefresh.Click += (s, e) => ResetFilters();
        right.Controls.AddRange(new Control[] { _searchBox, btnRefresh });
        topBar.Controls.Add(right);
        topBar.Controls.Add(title);

        // ── Filter row ──
        var filterRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 44, BackColor = ThemeManager.MainBackground,
            FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 4, 0, 0)
        };

        Label Lbl(string t) => new()
        {
            Text = t, AutoSize = true, Font = ThemeManager.SmallBoldFont, ForeColor = ThemeManager.SecondaryText,
            Margin = new Padding(0, 8, 4, 0)
        };
        ComboBox Combo(params string[] items)
        {
            var c = new ComboBox { Width = 110, Margin = new Padding(0, 3, 14, 0) };
            ThemeManager.StyleComboBox(c);
            c.Items.AddRange(items);
            c.SelectedIndex = 0;
            return c;
        }

        _from = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 105, Margin = new Padding(0, 3, 14, 0) };
        _to = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 105, Margin = new Padding(0, 3, 14, 0) };
        _payment = Combo("All", "Cash", "Card", "Other");
        _status = Combo("All", "Completed", "Cancelled", "Refunded");
        _orderType = Combo("All", "Dine In", "Takeaway");
        ResetDates();

        filterRow.Controls.AddRange(new Control[]
        {
            Lbl("From"), _from, Lbl("To"), _to, Lbl("Payment"), _payment, Lbl("Status"), _status, Lbl("Order"), _orderType
        });

        // ── Action bar ──
        var actionBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 48, BackColor = ThemeManager.MainBackground,
            FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 4, 0, 0)
        };
        var btnView = UIHelper.CreateIconButton("👁 View", ThemeManager.Primary);
        btnView.Click += (s, e) => ViewSelected();
        var btnPrint = UIHelper.CreateIconButton("🖨 Print Receipt", ThemeManager.Secondary);
        btnPrint.MinimumSize = new Size(140, 40);
        btnPrint.Click += (s, e) => PrintSelected();
        var btnCancel = UIHelper.CreateIconButton("✖ Cancel Sale", ThemeManager.Danger);
        btnCancel.MinimumSize = new Size(130, 40);
        btnCancel.Click += (s, e) => CancelSelected();
        actionBar.Controls.AddRange(new Control[] { btnView, btnPrint, btnCancel });

        // ── Grid ──
        _grid = new DataGridView { Dock = DockStyle.Fill };
        ThemeManager.StyleDataGridView(_grid);
        UIHelper.SetDoubleBuffered(_grid);
        _grid.MultiSelect = false;
        _grid.Columns.Add("Txn", "Transaction ID");
        _grid.Columns.Add("Date", "Date/Time");
        _grid.Columns.Add("Cashier", "Cashier");
        _grid.Columns.Add("Order", "Order Type");
        _grid.Columns.Add("Payment", "Payment");
        _grid.Columns.Add("Subtotal", "Subtotal ($)");
        _grid.Columns.Add("Discount", "Discount ($)");
        _grid.Columns.Add("Tax", "Tax ($)");
        _grid.Columns.Add("Total", "Total ($)");
        _grid.Columns.Add("Status", "Status");
        _grid.Columns["Date"]!.FillWeight = 120;
        foreach (var n in new[] { "Subtotal", "Discount", "Tax", "Total" })
            _grid.Columns[n]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _grid.Columns["Status"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ViewSelected(); };

        Controls.Add(_grid);
        Controls.Add(actionBar);
        Controls.Add(filterRow);
        Controls.Add(topBar);

        _searchBox.TextChanged += (s, e) => RefreshGrid();
        _from.ValueChanged += (s, e) => RefreshGrid();
        _to.ValueChanged += (s, e) => RefreshGrid();
        _payment.SelectedIndexChanged += (s, e) => RefreshGrid();
        _status.SelectedIndexChanged += (s, e) => RefreshGrid();
        _orderType.SelectedIndexChanged += (s, e) => RefreshGrid();

        RefreshGrid();
    }

    private string SearchText => _searchBox.Text == (string?)_searchBox.Tag ? "" : _searchBox.Text.Trim();

    private void ResetDates()
    {
        _from.Value = DateTime.Today.AddDays(-35);
        _to.Value = DateTime.Today;
    }

    private void ResetFilters()
    {
        _searchBox.Text = (string)_searchBox.Tag!;
        _searchBox.ForeColor = ThemeManager.SecondaryText;
        ResetDates();
        _payment.SelectedIndex = 0;
        _status.SelectedIndex = 0;
        _orderType.SelectedIndex = 0;
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        string? selectedTxn = (_grid.CurrentRow?.Tag as Sale)?.TransactionId;
        string search = SearchText;
        DateTime from = _from.Value.Date, to = _to.Value.Date;

        var filtered = _sales.Where(s =>
            (search.Length == 0 || s.TransactionId.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
            s.DateTime.Date >= from && s.DateTime.Date <= to &&
            (_payment.SelectedIndex <= 0 || s.PaymentMethod.ToString() == _payment.SelectedItem!.ToString()) &&
            (_status.SelectedIndex <= 0 || s.Status.ToString() == _status.SelectedItem!.ToString()) &&
            (_orderType.SelectedIndex <= 0 || SaleDetailsDialog.FormatOrderType(s.OrderType) == _orderType.SelectedItem!.ToString()))
            .OrderByDescending(s => s.DateTime);

        _grid.Rows.Clear();
        foreach (var s in filtered)
        {
            int idx = _grid.Rows.Add(s.TransactionId, s.DateTime.ToString("yyyy-MM-dd HH:mm"), s.CashierName,
                SaleDetailsDialog.FormatOrderType(s.OrderType), s.PaymentMethod.ToString(),
                s.Subtotal.ToString("N2"), s.Discount.ToString("N2"), s.Tax.ToString("N2"), s.Total.ToString("N2"),
                s.Status.ToString());
            var row = _grid.Rows[idx];
            row.Tag = s;

            var cell = row.Cells["Status"];
            cell.Style.BackColor = SaleDetailsDialog.StatusColor(s.Status);
            cell.Style.SelectionBackColor = SaleDetailsDialog.StatusColor(s.Status);
            cell.Style.ForeColor = Color.White;
            cell.Style.SelectionForeColor = Color.White;
            cell.Style.Font = ThemeManager.BodyBoldFont;

            if (selectedTxn == s.TransactionId) row.Selected = true;
        }
    }

    private Sale? SelectedSale =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].Tag as Sale : _grid.CurrentRow?.Tag as Sale;

    private void ViewSelected()
    {
        var sale = SelectedSale;
        if (sale == null) { MessageBox.Show("Please select a transaction first.", "View Sale", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        using var dlg = new SaleDetailsDialog(sale);
        dlg.ShowDialog(FindForm());
    }

    private void PrintSelected()
    {
        var sale = SelectedSale;
        if (sale == null) { MessageBox.Show("Please select a transaction first.", "Print Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        if (FindForm() is Forms.MainForm mainForm)
        {
            mainForm.LoadPage(new ReceiptControl(sale));
        }
    }

    private void CancelSelected()
    {
        var sale = SelectedSale;
        if (sale == null) { MessageBox.Show("Please select a transaction first.", "Cancel Sale", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        if (sale.Status != SaleStatus.Completed)
        {
            MessageBox.Show($"Only completed sales can be cancelled. This sale is already {sale.Status}.", "Cancel Sale",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (ConfirmationDialog.Show($"Cancel transaction {sale.TransactionId}?", "Cancel Sale") != DialogResult.OK)
            return;
        sale.Status = SaleStatus.Cancelled;
        RefreshGrid();
    }
}

