using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace CoffeeShopPOS.UserControls;

/// <summary>
/// Dashboard overview: KPI cards, GDI+ charts and summary grids (in-memory mock data).
/// </summary>
public partial class DashboardControl : UserControl
{
    // ───────────── Mock chart data ─────────────
    private static readonly (string Label, decimal Value)[] WeeklySales =
    {
        ("Mon", 820m), ("Tue", 940m), ("Wed", 760m), ("Thu", 1100m),
        ("Fri", 1340m), ("Sat", 1680m), ("Sun", 1247.50m)
    };

    private static readonly (string Label, decimal Value)[] CategorySales =
    {
        ("Espresso", 1840m), ("Hot Coffee", 3120m), ("Iced Coffee", 2480m), ("Tea", 1260m),
        ("Frappes", 1920m), ("Desserts", 980m), ("Bakery", 1150m)
    };

    private static readonly Color[] CategoryColors =
    {
        ThemeManager.Primary,
        ThemeManager.Secondary,
        ThemeManager.Accent,
        ControlPaint.Dark(ThemeManager.Primary, 0.25f),
        ControlPaint.Light(ThemeManager.Primary, 0.35f),
        ControlPaint.Dark(ThemeManager.Secondary, 0.2f),
        ControlPaint.Light(ThemeManager.Accent, 0.3f)
    };

    private readonly DataGridView _recentGrid = new();
    private readonly DataGridView _bestGrid = new();

    public DashboardControl()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.MainBackground;
        AutoScroll = true;
        Padding = new Padding(20);
        DoubleBuffered = true;

        // Dock=Top: last added control is placed at the very top.
        Controls.Add(BuildBottomRow());
        Controls.Add(BuildChartsRow());
        Controls.Add(BuildKpiRow());

        RefreshData();
        VisibleChanged += (_, _) => { if (Visible) RefreshData(); };
    }

    // ───────────── KPI row ─────────────

    private Control BuildKpiRow()
    {
        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 126,
            Padding = new Padding(0, 0, 0, 16),
            ColumnCount = 6,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        for (int i = 0; i < 6; i++)
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6f));
        tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var cards = new (string Title, string Value, Color Accent)[]
        {
            ("Today's Sales", "$1,247.50", ThemeManager.Success),
            ("Transactions", "34", Color.FromArgb(74, 144, 217)),
            ("Avg Order", "$36.69", ThemeManager.Primary),
            ("Products", MockData.GetProducts().Count.ToString(), ThemeManager.Secondary),
            ("Best Seller", "Cappuccino", ThemeManager.Warning),
            ("Discounts", "$45.00", ThemeManager.Danger)
        };

        for (int i = 0; i < cards.Length; i++)
        {
            var card = CreateKpiCard(cards[i].Title, cards[i].Value, cards[i].Accent);
            card.Margin = new Padding(i == 0 ? 0 : 6, 0, i == cards.Length - 1 ? 0 : 6, 0);
            tlp.Controls.Add(card, i, 0);
        }
        return tlp;
    }

    private static Panel CreateKpiCard(string title, string value, Color accent)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.CardBackground, MinimumSize = new Size(120, 90) };

        var valueLabel = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            AutoSize = false,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = ThemeManager.MainText,
            TextAlign = ContentAlignment.BottomLeft,
            Padding = new Padding(14, 0, 4, 0)
        };
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Bottom,
            Height = 34,
            AutoSize = false,
            Font = new Font("Segoe UI", 9F),
            ForeColor = ThemeManager.SecondaryText,
            TextAlign = ContentAlignment.TopLeft,
            Padding = new Padding(14, 3, 4, 0)
        };
        var strip = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };

        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(strip);

        // Shrink the value font if the text does not fit the card width.
        void Fit()
        {
            float size = 18F;
            int avail = valueLabel.Width - valueLabel.Padding.Horizontal;
            if (avail <= 0) return;
            while (size > 10F)
            {
                using var probe = new Font("Segoe UI", size, FontStyle.Bold);
                if (TextRenderer.MeasureText(valueLabel.Text, probe).Width <= avail) break;
                size -= 1F;
            }
            if (Math.Abs(valueLabel.Font.Size - size) > 0.1f)
            {
                var old = valueLabel.Font;
                valueLabel.Font = new Font("Segoe UI", size, FontStyle.Bold);
                old.Dispose();
            }
        }
        valueLabel.SizeChanged += (_, _) => Fit();

        card.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(228, 222, 214));
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };
        return card;
    }

    // ───────────── Charts row ─────────────

    private Control BuildChartsRow()
    {
        var tlp = CreateTwoColumnRow(290);

        var weekly = new ChartCanvas();
        weekly.Paint += PaintWeeklyChart;
        tlp.Controls.Add(CreateSection("Weekly Sales", weekly, new Padding(0, 0, 8, 0)), 0, 0);

        var category = new ChartCanvas();
        category.Paint += PaintCategoryChart;
        tlp.Controls.Add(CreateSection("Sales by Category", category, new Padding(8, 0, 0, 0)), 1, 0);

        return tlp;
    }

    private Control BuildBottomRow()
    {
        var tlp = CreateTwoColumnRow(330);

        ConfigureGrid(_recentGrid,
            ("ID", 70), ("Time", 140), ("Items", 60), ("Total", 90), ("Status", 100));
        tlp.Controls.Add(CreateSection("Recent Transactions", _recentGrid, new Padding(0, 0, 8, 0)), 0, 0);

        ConfigureGrid(_bestGrid,
            ("Rank", 50), ("Product", 170), ("Category", 100), ("Qty Sold", 70), ("Revenue", 90));
        tlp.Controls.Add(CreateSection("Best Selling Products", _bestGrid, new Padding(8, 0, 0, 0)), 1, 0);

        return tlp;
    }

    private static TableLayoutPanel CreateTwoColumnRow(int height)
    {
        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = height,
            Padding = new Padding(0, 0, 0, 16),
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        return tlp;
    }

    /// <summary>White bordered card with a title and a fill content control.</summary>
    private static Panel CreateSection(string title, Control content, Padding margin)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = margin,
            BackColor = ThemeManager.CardBackground,
            Padding = new Padding(14, 10, 14, 12)
        };
        content.Dock = DockStyle.Fill;
        var lbl = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 30,
            AutoSize = false,
            Font = ThemeManager.BodyBoldFont,
            ForeColor = ThemeManager.MainText,
            TextAlign = ContentAlignment.MiddleLeft
        };
        panel.Controls.Add(content);
        panel.Controls.Add(lbl);
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(228, 222, 214));
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };
        return panel;
    }

    private sealed class ChartCanvas : Panel
    {
        public ChartCanvas()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = ThemeManager.CardBackground;
        }
    }

    // ───────────── GDI+ charts ─────────────

    private static decimal NiceStep(decimal rawStep)
    {
        if (rawStep <= 0) return 1;
        double mag = Math.Pow(10, Math.Floor(Math.Log10((double)rawStep)));
        double norm = (double)rawStep / mag;
        double nice = norm <= 1 ? 1 : norm <= 2 ? 2 : norm <= 5 ? 5 : 10;
        return (decimal)(nice * mag);
    }

    private static string Money0(decimal v) => "$" + v.ToString("N0", CultureInfo.InvariantCulture);

    private void PaintWeeklyChart(object? sender, PaintEventArgs e)
    {
        var ctl = (Control)sender!;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        const int left = 52, right = 8, top = 20, bottom = 26;
        var plot = new Rectangle(left, top, ctl.ClientSize.Width - left - right, ctl.ClientSize.Height - top - bottom);
        if (plot.Width < 60 || plot.Height < 50) return;

        decimal max = WeeklySales.Max(w => w.Value);
        decimal step = NiceStep(max / 4m);
        decimal axisMax = Math.Ceiling(max / step) * step;
        int lines = (int)(axisMax / step);

        using var gridPen = new Pen(Color.FromArgb(232, 232, 232));
        using var axisBrush = new SolidBrush(ThemeManager.SecondaryText);
        using var smallFont = new Font("Segoe UI", 8F);
        using var valueFont = new Font("Segoe UI", 8F, FontStyle.Bold);
        using var rightFmt = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        using var centerFmt = new StringFormat { Alignment = StringAlignment.Center };

        for (int i = 0; i <= lines; i++)
        {
            float y = plot.Bottom - (float)(plot.Height * (i * step) / axisMax);
            g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            g.DrawString(Money0(i * step), smallFont, axisBrush,
                new RectangleF(0, y - 9, left - 6, 18), rightFmt);
        }

        float slot = plot.Width / (float)WeeklySales.Length;
        float barW = slot * 0.55f;
        using var barBrush = new SolidBrush(ThemeManager.Primary);
        using var textBrush = new SolidBrush(ThemeManager.MainText);

        for (int i = 0; i < WeeklySales.Length; i++)
        {
            float h = (float)(plot.Height * WeeklySales[i].Value / axisMax);
            float x = plot.Left + i * slot + (slot - barW) / 2f;
            var bar = new RectangleF(x, plot.Bottom - h, barW, h);
            g.FillRectangle(barBrush, bar);

            g.DrawString(Money0(WeeklySales[i].Value), valueFont, textBrush,
                new RectangleF(x - 14, bar.Top - 16, barW + 28, 16), centerFmt);
            g.DrawString(WeeklySales[i].Label, smallFont, axisBrush,
                new RectangleF(x - 14, plot.Bottom + 5, barW + 28, 16), centerFmt);
        }

        using var basePen = new Pen(Color.FromArgb(200, 200, 200));
        g.DrawLine(basePen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
    }

    private void PaintCategoryChart(object? sender, PaintEventArgs e)
    {
        var ctl = (Control)sender!;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using var labelFont = new Font("Segoe UI", 9F);
        using var valueFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        using var labelBrush = new SolidBrush(ThemeManager.MainText);
        using var leftFmt = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

        int labelW = 0;
        foreach (var c in CategorySales)
            labelW = Math.Max(labelW, TextRenderer.MeasureText(c.Label, labelFont).Width);
        labelW += 10;
        const int valueW = 62;

        var area = new Rectangle(0, 2, ctl.ClientSize.Width, ctl.ClientSize.Height - 4);
        int barAreaW = area.Width - labelW - valueW;
        if (barAreaW < 40 || area.Height < 40) return;

        decimal max = CategorySales.Max(c => c.Value);
        float rowH = area.Height / (float)CategorySales.Length;
        float barH = Math.Min(rowH * 0.62f, 26f);

        for (int i = 0; i < CategorySales.Length; i++)
        {
            float y = area.Top + i * rowH;
            float cy = y + rowH / 2f;
            g.DrawString(CategorySales[i].Label, labelFont, labelBrush,
                new RectangleF(0, y, labelW, rowH), leftFmt);

            float len = Math.Max(2f, (float)(barAreaW * CategorySales[i].Value / max));
            var rect = new RectangleF(labelW, cy - barH / 2f, len, barH);
            using var brush = new SolidBrush(CategoryColors[i % CategoryColors.Length]);
            g.FillRectangle(brush, rect);

            g.DrawString(Money0(CategorySales[i].Value), valueFont, labelBrush,
                new RectangleF(rect.Right + 6, y, valueW + 20, rowH), leftFmt);
        }
    }

    // ───────────── Grids ─────────────

    private static void ConfigureGrid(DataGridView dgv, params (string Name, int Weight)[] columns)
    {
        ThemeManager.StyleDataGridView(dgv);
        dgv.Dock = DockStyle.Fill;
        dgv.RowTemplate.Height = 34;
        dgv.ColumnHeadersHeight = 36;
        dgv.AllowUserToResizeRows = false;
        dgv.AllowUserToResizeColumns = false;
        dgv.MultiSelect = false;
        dgv.Font = ThemeManager.SmallFont;
        dgv.ColumnHeadersDefaultCellStyle.Font = ThemeManager.SmallBoldFont;
        dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        dgv.TabStop = false;
        UIHelper.SetDoubleBuffered(dgv);

        foreach (var (name, weight) in columns)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = name,
                Name = name,
                FillWeight = weight,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }
    }

    /// <summary>Reloads the grids from the shared mock data (also called when the page becomes visible).</summary>
    public void RefreshData()
    {
        // Recent transactions
        _recentGrid.Rows.Clear();
        var recent = MockData.GetSales().OrderByDescending(s => s.DateTime).Take(7);
        foreach (var s in recent)
        {
            int idx = _recentGrid.Rows.Add(
                s.TransactionId,
                s.DateTime.ToString("MMM dd, hh:mm tt", CultureInfo.InvariantCulture),
                s.Items.Sum(i => i.Quantity),
                "$" + s.Total.ToString("N2", CultureInfo.InvariantCulture),
                s.Status.ToString());

            var cell = _recentGrid.Rows[idx].Cells["Status"];
            var color = s.Status switch
            {
                SaleStatus.Completed => ThemeManager.Success,
                SaleStatus.Refunded => ThemeManager.Warning,
                _ => ThemeManager.Danger
            };
            cell.Style.ForeColor = color;
            cell.Style.SelectionForeColor = color;
            cell.Style.Font = ThemeManager.SmallBoldFont;
            _recentGrid.Rows[idx].Cells["Total"].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        _recentGrid.ClearSelection();

        // Best selling products
        _bestGrid.Rows.Clear();
        var products = MockData.GetProducts();
        var top = MockData.GetSales()
            .Where(s => s.Status == SaleStatus.Completed)
            .SelectMany(s => s.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                Name = g.First().ProductName,
                Category = products.FirstOrDefault(p => p.Id == g.Key)?.CategoryName ?? "-",
                Qty = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal)
            })
            .OrderByDescending(x => x.Qty).ThenByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        for (int i = 0; i < top.Count; i++)
        {
            int idx = _bestGrid.Rows.Add(
                "#" + (i + 1),
                top[i].Name,
                top[i].Category,
                top[i].Qty,
                "$" + top[i].Revenue.ToString("N2", CultureInfo.InvariantCulture));
            _bestGrid.Rows[idx].Cells["Revenue"].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        _bestGrid.ClearSelection();
    }
}

