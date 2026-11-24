using System.Drawing.Drawing2D;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.UserControls;

/// <summary>
/// Sales analytics screen: date range + report type filters, KPI cards,
/// a GDI+ chart and a report grid, all driven by <see cref="MockData.GetSales"/>.
/// Only completed sales are counted (cancelled / refunded are excluded).
/// </summary>
public partial class AnalyticsControl : UserControl
{
    // ───────────── Chart model ─────────────
    private enum ChartKind { VerticalBars, HorizontalBars }

    private sealed record ChartPoint(string Label, decimal Value, Color? Color = null);

    private sealed class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }

    private static readonly string[] ReportTypes =
    {
        "Daily Sales", "Weekly Sales", "Monthly Sales", "Sales by Product",
        "Sales by Category", "Sales by Payment Method", "Best-Selling Products", "Cashier Summary"
    };

    private static readonly Color BlueAccent = Color.FromArgb(52, 120, 200);
    private static readonly Font KpiValueFont = new("Segoe UI", 18F, FontStyle.Bold);
    private static readonly Font ChartLabelFont = new("Segoe UI", 8F, FontStyle.Regular);
    private static readonly Font ChartValueFont = new("Segoe UI", 8F, FontStyle.Bold);

    // ───────────── Controls ─────────────
    private readonly DateTimePicker _fromPicker = new();
    private readonly DateTimePicker _toPicker = new();
    private readonly ComboBox _reportCombo = new();
    private readonly Button _generateButton = new() { Text = "📊 Generate Report" };
    private readonly Button _exportButton = new() { Text = "📥 Export" };
    private readonly BufferedPanel _chartPanel = new();
    private readonly DataGridView _grid = new();
    private Label _revenueValue = null!;
    private Label _transactionsValue = null!;
    private Label _avgValue = null!;
    private Label _bestSellerValue = null!;

    // ───────────── Chart state ─────────────
    private List<ChartPoint> _points = new();
    private ChartKind _chartKind = ChartKind.VerticalBars;
    private string _chartTitle = string.Empty;
    private string _xAxisTitle = string.Empty;
    private string _yAxisTitle = string.Empty;
    private bool _valueIsMoney = true;
    private bool _showLegend;

    private string Currency =>
        string.IsNullOrWhiteSpace(MockData.Settings.Currency) ? "$" : MockData.Settings.Currency;

    public AnalyticsControl()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;
        AutoScroll = true;
        AutoScrollMinSize = new Size(900, 820);
        Padding = new Padding(24);

        BuildLayout();

        _fromPicker.Value = DateTime.Today.AddDays(-30);
        _toPicker.Value = DateTime.Today;
        _reportCombo.SelectedIndex = 0;

        GenerateReport();
    }

    // ───────────── Layout ─────────────
    private void BuildLayout()
    {
        // Report grid (fills remaining space)
        ThemeManager.StyleDataGridView(_grid);
        _grid.Dock = DockStyle.Fill;
        UIHelper.SetDoubleBuffered(_grid);

        // Chart
        _chartPanel.Dock = DockStyle.Fill;
        _chartPanel.BackColor = ThemeManager.CardBackground;
        _chartPanel.Paint += ChartPanel_Paint;
        var chartWrap = new Panel { Dock = DockStyle.Top, Height = 312, Padding = new Padding(0, 0, 0, 12) };
        chartWrap.Controls.Add(_chartPanel);

        // KPI row
        var kpiTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1
        };
        for (int i = 0; i < 4; i++) kpiTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        kpiTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        kpiTable.Controls.Add(CreateKpiCard("Total Revenue", ThemeManager.Success, out _revenueValue, false), 0, 0);
        kpiTable.Controls.Add(CreateKpiCard("Transactions", BlueAccent, out _transactionsValue, false), 1, 0);
        kpiTable.Controls.Add(CreateKpiCard("Avg Value", ThemeManager.Primary, out _avgValue, false), 2, 0);
        kpiTable.Controls.Add(CreateKpiCard("Best Seller", ThemeManager.Warning, out _bestSellerValue, true), 3, 0);
        var kpiWrap = new Panel { Dock = DockStyle.Top, Height = 114, Padding = new Padding(0, 0, 0, 14) };
        kpiWrap.Controls.Add(kpiTable);

        // Top bar
        var title = new Label
        {
            Text = "📈 Sales Analytics",
            Font = ThemeManager.SubHeaderFont,
            ForeColor = ThemeManager.MainText,
            Dock = DockStyle.Top,
            Height = 46,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _fromPicker.Format = DateTimePickerFormat.Short;
        _toPicker.Format = DateTimePickerFormat.Short;
        foreach (var dtp in new[] { _fromPicker, _toPicker })
        {
            dtp.Font = ThemeManager.BodyFont;
            dtp.Width = 125;
            dtp.Margin = new Padding(0, 4, 16, 0);
        }

        _reportCombo.Items.AddRange(ReportTypes);
        ThemeManager.StyleComboBox(_reportCombo);
        _reportCombo.Width = 230;
        _reportCombo.Margin = new Padding(0, 4, 16, 0);

        ThemeManager.StyleButton(_generateButton, true);
        _generateButton.Size = new Size(170, 36);
        _generateButton.Margin = new Padding(0, 0, 12, 0);
        _generateButton.Click += (_, _) => GenerateReport();

        ThemeManager.StyleButton(_exportButton, false);
        _exportButton.Size = new Size(110, 36);
        _exportButton.Margin = new Padding(0);
        _exportButton.Click += (_, _) =>
            MessageBox.Show("Export feature coming in next version", "Export",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

        var filters = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };
        filters.Controls.Add(CreateFilterLabel("From:"));
        filters.Controls.Add(_fromPicker);
        filters.Controls.Add(CreateFilterLabel("To:"));
        filters.Controls.Add(_toPicker);
        filters.Controls.Add(CreateFilterLabel("Report:"));
        filters.Controls.Add(_reportCombo);
        filters.Controls.Add(_generateButton);
        filters.Controls.Add(_exportButton);

        var topBar = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0, 0, 0, 10) };
        topBar.Controls.Add(filters);
        topBar.Controls.Add(title);

        // Fill control first, then top-docked controls (last added is docked first / topmost).
        Controls.Add(_grid);
        Controls.Add(chartWrap);
        Controls.Add(kpiWrap);
        Controls.Add(topBar);
    }

    private static Label CreateFilterLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = ThemeManager.BodyBoldFont,
        ForeColor = ThemeManager.SecondaryText,
        Margin = new Padding(0, 9, 6, 0)
    };

    private static Panel CreateKpiCard(string title, Color accent, out Label valueLabel, bool lastCard)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeManager.CardBackground,
            Margin = new Padding(0, 0, lastCard ? 0 : 12, 0)
        };

        var strip = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent };
        var titleLabel = new Label
        {
            Text = title,
            Font = ThemeManager.SmallBoldFont,
            ForeColor = ThemeManager.SecondaryText,
            Dock = DockStyle.Top,
            Height = 34,
            Padding = new Padding(16, 14, 0, 0),
            AutoSize = false
        };
        valueLabel = new Label
        {
            Text = "—",
            Font = KpiValueFont,
            ForeColor = ThemeManager.MainText,
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 0, 8, 10),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            AutoSize = false
        };

        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(strip);
        return card;
    }

    // ───────────── Report generation ─────────────
    private void GenerateReport()
    {
        var from = _fromPicker.Value.Date;
        var to = _toPicker.Value.Date;
        if (from > to)
        {
            MessageBox.Show("The 'From' date must be on or before the 'To' date.", "Invalid Date Range",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var end = to.AddDays(1);
        var sales = MockData.GetSales()
            .Where(s => s.Status == SaleStatus.Completed && s.DateTime >= from && s.DateTime < end)
            .ToList();

        UpdateKpis(sales);

        _grid.SuspendLayout();
        _grid.Rows.Clear();
        _grid.Columns.Clear();

        switch (_reportCombo.SelectedIndex)
        {
            case 1: BuildPeriodReport(sales, "Week", "Weekly Sales", "Week Starting",
                d => d.AddDays(-(((int)d.DayOfWeek + 6) % 7)),
                d => $"{d:MMM dd} – {d.AddDays(6):MMM dd}", d => $"{d:MMM dd}"); break;
            case 2: BuildPeriodReport(sales, "Month", "Monthly Sales", "Month",
                d => new DateTime(d.Year, d.Month, 1),
                d => $"{d:MMMM yyyy}", d => $"{d:MMM yy}"); break;
            case 3: BuildByProduct(sales); break;
            case 4: BuildByCategory(sales); break;
            case 5: BuildByPayment(sales); break;
            case 6: BuildBestSelling(sales); break;
            case 7: BuildCashierSummary(sales); break;
            default: BuildPeriodReport(sales, "Date", "Daily Sales", "Date",
                d => d.Date, d => $"{d:ddd, MMM dd yyyy}", d => $"{d:MM/dd}"); break;
        }

        _grid.ResumeLayout();
        _chartPanel.Invalidate();
    }

    private void UpdateKpis(List<Sale> sales)
    {
        decimal revenue = sales.Sum(s => s.Total);
        int count = sales.Count;
        decimal avg = count > 0 ? revenue / count : 0m;

        var best = sales.SelectMany(s => s.Items)
            .GroupBy(i => i.ProductName)
            .Select(g => new { Name = g.Key, Qty = g.Sum(i => i.Quantity) })
            .OrderByDescending(x => x.Qty)
            .FirstOrDefault();

        _revenueValue.Text = Money(revenue);
        _transactionsValue.Text = count.ToString();
        _avgValue.Text = Money(avg);
        _bestSellerValue.Text = best?.Name ?? "—";
    }

    private string Money(decimal value, string format = "N2") => Currency + value.ToString(format);

    // ───────────── Report builders ─────────────
    private void AddColumns(params (string Header, bool RightAlign)[] columns)
    {
        foreach (var (header, right) in columns)
        {
            var col = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            if (right)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            _grid.Columns.Add(col);
        }
    }

    private void SetChart(string title, string xTitle, string yTitle, ChartKind kind,
        List<ChartPoint> points, bool money, bool legend = false)
    {
        _chartTitle = title;
        _xAxisTitle = xTitle;
        _yAxisTitle = yTitle;
        _chartKind = kind;
        _points = points;
        _valueIsMoney = money;
        _showLegend = legend;
    }

    /// <summary>Daily / weekly / monthly revenue grouped by a period key.</summary>
    private void BuildPeriodReport(List<Sale> sales, string periodHeader, string title, string xTitle,
        Func<DateTime, DateTime> keyOf, Func<DateTime, string> gridLabel, Func<DateTime, string> chartLabel)
    {
        AddColumns((periodHeader, false), ("Transactions", true), ("Revenue", true));

        var groups = sales
            .GroupBy(s => keyOf(s.DateTime))
            .OrderBy(g => g.Key)
            .Select(g => new { Key = g.Key, Count = g.Count(), Revenue = g.Sum(s => s.Total) })
            .ToList();

        foreach (var g in groups)
            _grid.Rows.Add(gridLabel(g.Key), g.Count, Money(g.Revenue));

        SetChart($"{title} — Revenue", xTitle, $"Revenue ({Currency})", ChartKind.VerticalBars,
            groups.Select(g => new ChartPoint(chartLabel(g.Key), g.Revenue)).ToList(), true);
    }

    private sealed record ProductAgg(string Name, string Category, int Qty, decimal Revenue);

    private List<ProductAgg> AggregateProducts(List<Sale> sales)
    {
        var productMap = MockData.GetProducts().ToDictionary(p => p.Id);
        return sales.SelectMany(s => s.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new ProductAgg(
                g.First().ProductName,
                productMap.TryGetValue(g.Key, out var p) ? p.CategoryName : "Unknown",
                g.Sum(i => i.Quantity),
                g.Sum(i => i.LineTotal)))
            .ToList();
    }

    private void BuildByProduct(List<Sale> sales)
    {
        AddColumns(("Product", false), ("Category", false), ("Qty Sold", true), ("Revenue", true));
        var rows = AggregateProducts(sales).OrderByDescending(p => p.Revenue).ToList();
        foreach (var r in rows)
            _grid.Rows.Add(r.Name, r.Category, r.Qty, Money(r.Revenue));

        SetChart("Sales by Product — Revenue", $"Revenue ({Currency})", "Product", ChartKind.HorizontalBars,
            rows.Take(10).Select(r => new ChartPoint(r.Name, r.Revenue)).ToList(), true);
    }

    private void BuildByCategory(List<Sale> sales)
    {
        AddColumns(("Category", false), ("Products", true), ("Qty Sold", true), ("Revenue", true));
        var rows = AggregateProducts(sales)
            .GroupBy(p => p.Category)
            .Select(g => new { Category = g.Key, Products = g.Count(), Qty = g.Sum(p => p.Qty), Revenue = g.Sum(p => p.Revenue) })
            .OrderByDescending(r => r.Revenue)
            .ToList();
        foreach (var r in rows)
            _grid.Rows.Add(r.Category, r.Products, r.Qty, Money(r.Revenue));

        SetChart("Sales by Category — Revenue", $"Revenue ({Currency})", "Category", ChartKind.HorizontalBars,
            rows.Select(r => new ChartPoint(r.Category, r.Revenue)).ToList(), true);
    }

    private void BuildByPayment(List<Sale> sales)
    {
        AddColumns(("Method", false), ("Transactions", true), ("Revenue", true), ("% of Total", true));
        decimal total = sales.Sum(s => s.Total);
        var colors = new Dictionary<PaymentMethod, Color>
        {
            [PaymentMethod.Cash] = ThemeManager.Success,
            [PaymentMethod.Card] = BlueAccent,
            [PaymentMethod.Other] = ThemeManager.Warning
        };

        var points = new List<ChartPoint>();
        foreach (var method in new[] { PaymentMethod.Cash, PaymentMethod.Card, PaymentMethod.Other })
        {
            var subset = sales.Where(s => s.PaymentMethod == method).ToList();
            decimal revenue = subset.Sum(s => s.Total);
            decimal pct = total > 0 ? revenue / total * 100m : 0m;
            _grid.Rows.Add(method.ToString(), subset.Count, Money(revenue), $"{pct:0.0}%");
            points.Add(new ChartPoint(method.ToString(), revenue, colors[method]));
        }

        // An all-zero chart is meaningless; let the paint handler show the empty message.
        if (total <= 0) points.Clear();
        SetChart("Sales by Payment Method — Revenue", "Payment Method", $"Revenue ({Currency})",
            ChartKind.VerticalBars, points, true, legend: true);
    }

    private void BuildBestSelling(List<Sale> sales)
    {
        AddColumns(("Rank", false), ("Product", false), ("Category", false), ("Qty Sold", true));
        var rows = AggregateProducts(sales)
            .OrderByDescending(p => p.Qty).ThenByDescending(p => p.Revenue)
            .Take(10).ToList();

        int rank = 1;
        foreach (var r in rows)
            _grid.Rows.Add(rank++, r.Name, r.Category, r.Qty);

        SetChart("Best-Selling Products — Quantity Sold", "Quantity Sold", "Product", ChartKind.HorizontalBars,
            rows.Select(r => new ChartPoint(r.Name, r.Qty)).ToList(), false);
    }

    private void BuildCashierSummary(List<Sale> sales)
    {
        AddColumns(("Cashier", false), ("Transactions", true), ("Revenue", true), ("Avg Order", true));
        var rows = sales
            .GroupBy(s => s.CashierName)
            .Select(g => new { Cashier = g.Key, Count = g.Count(), Revenue = g.Sum(s => s.Total) })
            .OrderByDescending(r => r.Revenue)
            .ToList();
        foreach (var r in rows)
            _grid.Rows.Add(r.Cashier, r.Count, Money(r.Revenue), Money(r.Count > 0 ? r.Revenue / r.Count : 0m));

        SetChart("Cashier Summary — Revenue", "Cashier", $"Revenue ({Currency})", ChartKind.VerticalBars,
            rows.Select(r => new ChartPoint(r.Cashier, r.Revenue)).ToList(), true);
    }

    // ───────────── Chart painting ─────────────
    private void ChartPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = _chartPanel.ClientRectangle;

        using (var border = new Pen(Color.FromArgb(40, 0, 0, 0)))
            g.DrawRectangle(border, 0, 0, bounds.Width - 1, bounds.Height - 1);

        using (var titleBrush = new SolidBrush(ThemeManager.MainText))
            g.DrawString(_chartTitle, ThemeManager.TitleFont, titleBrush, 16, 10);

        if (_points.Count == 0)
        {
            using var brush = new SolidBrush(ThemeManager.SecondaryText);
            using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("No sales data for the selected period.", ThemeManager.BodyFont, brush,
                new RectangleF(0, 0, bounds.Width, bounds.Height), fmt);
            return;
        }

        if (_showLegend) DrawLegend(g, bounds);

        if (_chartKind == ChartKind.VerticalBars) DrawVerticalBars(g, bounds);
        else DrawHorizontalBars(g, bounds);
    }

    private void DrawLegend(Graphics g, Rectangle bounds)
    {
        using var textBrush = new SolidBrush(ThemeManager.MainText);
        float x = bounds.Width - 20;
        for (int i = _points.Count - 1; i >= 0; i--)
        {
            var p = _points[i];
            var size = g.MeasureString(p.Label, ChartLabelFont);
            x -= size.Width;
            g.DrawString(p.Label, ChartLabelFont, textBrush, x, 15);
            x -= 16;
            using var swatch = new SolidBrush(p.Color ?? ThemeManager.Primary);
            g.FillRectangle(swatch, x, 17, 12, 12);
            x -= 18;
        }
    }

    private decimal AxisMax()
    {
        decimal max = _points.Max(p => p.Value);
        if (!_valueIsMoney) max = Math.Max(max, 5m);
        if (max <= 0) return 1m;

        double d = (double)max;
        double mag = Math.Pow(10, Math.Floor(Math.Log10(d)));
        double norm = d / mag;
        double nice = norm <= 1 ? 1 : norm <= 2 ? 2 : norm <= 5 ? 5 : 10;
        return (decimal)(nice * mag);
    }

    private string FormatAxis(decimal v)
    {
        string text = v >= 1000 ? $"{v / 1000m:0.#}k" : v.ToString("0.##");
        return _valueIsMoney ? Currency + text : text;
    }

    private string FormatValue(decimal v, bool compact)
    {
        if (!_valueIsMoney) return v.ToString("0");
        return Currency + v.ToString(compact ? "N0" : "N2");
    }

    private void DrawVerticalBars(Graphics g, Rectangle bounds)
    {
        const int left = 72, right = 24, top = 46, bottom = 58;
        var plot = new Rectangle(left, top, bounds.Width - left - right, bounds.Height - top - bottom);
        if (plot.Width < 60 || plot.Height < 60) return;

        decimal max = AxisMax();
        using var gridPen = new Pen(Color.FromArgb(230, 225, 218));
        using var axisPen = new Pen(ThemeManager.SecondaryText);
        using var textBrush = new SolidBrush(ThemeManager.SecondaryText);
        using var mainBrush = new SolidBrush(ThemeManager.MainText);
        using var rightFmt = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        using var centerFmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
        using var valueFmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

        // Horizontal gridlines + Y labels
        const int steps = 5;
        for (int i = 0; i <= steps; i++)
        {
            float y = plot.Bottom - plot.Height * i / (float)steps;
            g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            g.DrawString(FormatAxis(max * i / steps), ChartLabelFont, textBrush,
                new RectangleF(28, y - 9, left - 34, 18), rightFmt);
        }
        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // Bars
        int n = _points.Count;
        float slot = plot.Width / (float)n;
        float barW = Math.Min(slot * 0.6f, 72f);
        int step = Math.Max(1, (int)Math.Ceiling(54f / slot));
        for (int i = 0; i < n; i++)
        {
            var p = _points[i];
            float h = (float)(p.Value / max) * plot.Height;
            float cx = plot.Left + slot * i + slot / 2f;
            var bar = new RectangleF(cx - barW / 2f, plot.Bottom - h, barW, h);
            using (var fill = new SolidBrush(p.Color ?? ThemeManager.Primary))
                g.FillRectangle(fill, bar);

            if (i % step == 0)
            {
                g.DrawString(FormatValue(p.Value, slot < 70), ChartValueFont, mainBrush,
                    new RectangleF(cx - 45, bar.Top - 20, 90, 18), valueFmt);
                g.DrawString(p.Label, ChartLabelFont, textBrush,
                    new RectangleF(cx - 45, plot.Bottom + 5, 90, 18), centerFmt);
            }
        }

        // Axis titles
        g.DrawString(_xAxisTitle, ChartLabelFont, mainBrush,
            new RectangleF(plot.Left, bounds.Height - 24, plot.Width, 18), centerFmt);
        DrawRotatedTitle(g, _yAxisTitle, 12, plot.Top + plot.Height / 2f, mainBrush);
    }

    private void DrawHorizontalBars(Graphics g, Rectangle bounds)
    {
        const int right = 80, top = 46, bottom = 58;
        float labelWidth = _points.Max(p => g.MeasureString(p.Label, ChartLabelFont).Width);
        int left = (int)Math.Min(labelWidth, 180f) + 44;
        var plot = new Rectangle(left, top, bounds.Width - left - right, bounds.Height - top - bottom);
        if (plot.Width < 60 || plot.Height < 40) return;

        decimal max = AxisMax();
        using var gridPen = new Pen(Color.FromArgb(230, 225, 218));
        using var axisPen = new Pen(ThemeManager.SecondaryText);
        using var textBrush = new SolidBrush(ThemeManager.SecondaryText);
        using var mainBrush = new SolidBrush(ThemeManager.MainText);
        using var rowFmt = new StringFormat
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };
        using var centerFmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
        using var valueFmt = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

        // Vertical gridlines + X labels
        const int steps = 5;
        for (int i = 0; i <= steps; i++)
        {
            float x = plot.Left + plot.Width * i / (float)steps;
            g.DrawLine(gridPen, x, plot.Top, x, plot.Bottom);
            g.DrawString(FormatAxis(max * i / steps), ChartLabelFont, textBrush,
                new RectangleF(x - 30, plot.Bottom + 5, 60, 18), centerFmt);
        }
        g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // Bars
        int n = _points.Count;
        float slot = plot.Height / (float)n;
        float barH = Math.Min(slot * 0.6f, 30f);
        for (int i = 0; i < n; i++)
        {
            var p = _points[i];
            float w = (float)(p.Value / max) * plot.Width;
            float cy = plot.Top + slot * i + slot / 2f;
            var bar = new RectangleF(plot.Left, cy - barH / 2f, w, barH);
            using (var fill = new SolidBrush(p.Color ?? ThemeManager.Primary))
                g.FillRectangle(fill, bar);

            g.DrawString(p.Label, ChartLabelFont, mainBrush,
                new RectangleF(30, cy - 10, left - 36, 20), rowFmt);
            g.DrawString(FormatValue(p.Value, false), ChartValueFont, mainBrush,
                new RectangleF(bar.Right + 6, cy - 10, 80, 20), valueFmt);
        }

        // Axis titles
        g.DrawString(_xAxisTitle, ChartLabelFont, mainBrush,
            new RectangleF(plot.Left, bounds.Height - 24, plot.Width, 18), centerFmt);
        DrawRotatedTitle(g, _yAxisTitle, 12, plot.Top + plot.Height / 2f, mainBrush);
    }

    private static void DrawRotatedTitle(Graphics g, string text, float x, float centerY, Brush brush)
    {
        using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        var state = g.Save();
        g.TranslateTransform(x, centerY);
        g.RotateTransform(-90);
        g.DrawString(text, ChartLabelFont, brush, 0, 0, fmt);
        g.Restore(state);
    }
}

