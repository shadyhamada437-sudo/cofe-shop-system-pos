using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;
using System.Globalization;
using System.Text;

namespace CoffeeShopPOS.UserControls;

/// <summary>
/// Point-of-sale screen: product selection on the left, cart and payment on the right.
/// Works purely with in-memory data (MockData).
/// </summary>
public partial class POSControl : UserControl
{
    // ───────────── Constants / styling data ─────────────
    private static decimal TaxRatePercent => MockData.Settings.TaxRate;
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly Color LightButton = Color.FromArgb(240, 233, 223);
    private static readonly Color LineColor = Color.FromArgb(232, 226, 218);

    private static readonly Dictionary<string, Color> CategoryColors = new()
    {
        ["Espresso"] = Color.FromArgb(93, 64, 55),
        ["Hot Coffee"] = Color.FromArgb(139, 94, 60),
        ["Iced Coffee"] = Color.FromArgb(74, 144, 217),
        ["Tea"] = Color.FromArgb(107, 158, 91),
        ["Frappes"] = Color.FromArgb(181, 115, 157),
        ["Desserts"] = Color.FromArgb(217, 119, 43),
        ["Bakery"] = Color.FromArgb(217, 164, 65)
    };

    private static readonly Dictionary<string, string> CategoryEmoji = new()
    {
        ["Espresso"] = "☕",
        ["Hot Coffee"] = "☕",
        ["Iced Coffee"] = "🧊",
        ["Tea"] = "🍵",
        ["Frappes"] = "🥤",
        ["Desserts"] = "🍰",
        ["Bakery"] = "🥐"
    };

    private sealed class CartLine
    {
        public Product Product = null!;
        public int Quantity;
        public Panel Row = null!;
        public Label QtyLabel = null!;
        public Label TotalLabel = null!;
    }

    // ───────────── State ─────────────
    private List<Product> _products = new();
    private readonly List<CartLine> _cart = new();
    private string _activeCategory = "All";
    private bool _isDineIn;
    private PaymentMethod _payment = PaymentMethod.Cash;
    private string _txnNumber = "";
    private decimal _subtotal, _discount, _tax, _total;
    private bool _discountValid = true;

    // ───────────── Left controls ─────────────
    private TextBox _search = null!;
    private FlowLayoutPanel _categoryFlow = null!;
    private FlowLayoutPanel _productFlow = null!;
    private readonly List<Button> _categoryButtons = new();

    // ───────────── Right controls ─────────────
    private Panel _right = null!;
    private Label _txnLabel = null!;
    private Button _dineInBtn = null!, _takeawayBtn = null!;
    private Panel _tablePanel = null!;
    private TextBox _tableBox = null!;
    private Panel _cartList = null!;
    private Label _emptyLabel = null!;
    private Label _subtotalValue = null!, _taxLabel = null!, _taxValue = null!, _totalValue = null!;
    private TextBox _discountBox = null!;
    private Button _cashBtn = null!, _cardBtn = null!, _otherBtn = null!;
    private Panel _cashPanel = null!;
    private TextBox _receivedBox = null!;
    private Label _changeValue = null!;

    public POSControl()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.MainBackground;
        DoubleBuffered = true;

        // Dock: Fill first, then edge-docked controls.
        Controls.Add(BuildLeftPanel());
        Controls.Add(BuildRightPanel());

        Resize += (_, _) => SizeRightPanel();
        SizeRightPanel();

        ReloadCatalog();
        SetOrderType(false);
        SetPayment(PaymentMethod.Cash);
        NewTransaction();
        UpdateCartState();

        VisibleChanged += (_, _) =>
        {
            if (Visible)
            {
                ReloadCatalog();
                UpdateTotals();
            }
        };
    }

    private void SizeRightPanel() => _right.Width = Math.Max(390, (int)(Width * 0.35));

    // ═════════════════════════ LEFT: products ═════════════════════════

    private Control BuildLeftPanel()
    {
        var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = ThemeManager.MainBackground };

        _productFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = ThemeManager.MainBackground
        };
        UIHelper.SetDoubleBuffered(_productFlow);

        _categoryFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Padding = new Padding(0, 0, 0, 4),
            BackColor = ThemeManager.MainBackground
        };

        var searchHost = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(0, 0, 0, 10) };
        _search = UIHelper.CreateSearchBox("🔍 Search products...");
        _search.Font = new Font("Segoe UI", 12F);
        _search.Dock = DockStyle.Top;
        _search.TextChanged += (_, _) => ApplyFilter();
        searchHost.Controls.Add(_search);

        left.Controls.Add(_productFlow);
        left.Controls.Add(_categoryFlow);
        left.Controls.Add(searchHost);
        return left;
    }

    private string SearchText => _search.Text == (string?)_search.Tag ? "" : _search.Text.Trim();

    /// <summary>Re-reads products/categories from the shared store and refreshes the left side.</summary>
    private void ReloadCatalog()
    {
        _products = MockData.GetProducts().ToList();
        var names = MockData.GetCategories().Select(c => c.Name).ToList();
        if (_activeCategory != "All" && !names.Contains(_activeCategory)) _activeCategory = "All";

        _categoryFlow.SuspendLayout();
        foreach (var b in _categoryButtons) { _categoryFlow.Controls.Remove(b); b.Dispose(); }
        _categoryButtons.Clear();

        foreach (var name in new[] { "All" }.Concat(names))
        {
            var btn = new Button
            {
                Text = name,
                Tag = name,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Padding = new Padding(10, 3, 10, 3),
                Margin = new Padding(0, 0, 8, 6),
                MinimumSize = new Size(60, 34)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (_, _) =>
            {
                _activeCategory = name;
                RefreshCategoryStyles();
                ApplyFilter();
            };
            _categoryButtons.Add(btn);
            _categoryFlow.Controls.Add(btn);
        }
        _categoryFlow.ResumeLayout();
        RefreshCategoryStyles();
        ApplyFilter();
    }

    private void RefreshCategoryStyles()
    {
        foreach (var b in _categoryButtons)
            SetToggleStyle(b, (string)b.Tag! == _activeCategory);
    }

    private static void SetToggleStyle(Button b, bool active)
    {
        b.BackColor = active ? ThemeManager.Primary : LightButton;
        b.ForeColor = active ? Color.White : ThemeManager.MainText;
        b.FlatAppearance.MouseOverBackColor = active ? ControlPaint.Dark(ThemeManager.Primary, 0.08f) : Color.FromArgb(230, 220, 205);
        b.FlatAppearance.MouseDownBackColor = active ? ControlPaint.Dark(ThemeManager.Primary, 0.15f) : Color.FromArgb(220, 208, 190);
    }

    private void ApplyFilter()
    {
        string q = SearchText;
        var list = _products.Where(p =>
            (_activeCategory == "All" || p.CategoryName == _activeCategory) &&
            (q.Length == 0 ||
             p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
             p.CategoryName.Contains(q, StringComparison.OrdinalIgnoreCase))).ToList();

        _productFlow.SuspendLayout();
        var old = _productFlow.Controls.Cast<Control>().ToList();
        _productFlow.Controls.Clear();
        foreach (var c in old) c.Dispose();

        if (list.Count == 0)
        {
            _productFlow.Controls.Add(new Label
            {
                Text = "No products found",
                AutoSize = false,
                Size = new Size(300, 60),
                Font = ThemeManager.BodyFont,
                ForeColor = ThemeManager.SecondaryText,
                TextAlign = ContentAlignment.MiddleLeft
            });
        }
        else
        {
            foreach (var p in list) _productFlow.Controls.Add(CreateProductCard(p));
        }
        _productFlow.ResumeLayout();
    }

    private Panel CreateProductCard(Product p)
    {
        bool avail = p.IsAvailable;
        var baseColor = CategoryColors.TryGetValue(p.CategoryName, out var cc) ? cc : ThemeManager.Secondary;
        var topColor = avail ? baseColor : Color.FromArgb(185, 180, 175);
        string emoji = CategoryEmoji.TryGetValue(p.CategoryName, out var em) ? em : (p.Name.Length > 0 ? p.Name[..1] : "?");

        var card = new Panel
        {
            Size = new Size(155, 190),
            Margin = new Padding(6),
            BackColor = Color.White,
            Cursor = avail ? Cursors.Hand : Cursors.Default
        };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(222, 216, 208));
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        var colorBlock = new Panel { Location = new Point(1, 1), Size = new Size(153, 70), BackColor = topColor };
        var icon = new Label
        {
            Dock = DockStyle.Fill,
            Text = emoji,
            Font = new Font("Segoe UI Emoji", 26F),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter
        };
        colorBlock.Controls.Add(icon);

        var name = new Label
        {
            Text = p.Name,
            Location = new Point(8, 76),
            Size = new Size(139, 38),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = avail ? ThemeManager.MainText : ThemeManager.SecondaryText,
            AutoEllipsis = true
        };
        var price = new Label
        {
            Text = "$" + p.SellingPrice.ToString("N2", Inv),
            Location = new Point(8, 114),
            Size = new Size(139, 22),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = avail ? ThemeManager.Primary : ThemeManager.SecondaryText
        };
        var dot = new Label
        {
            Text = "●",
            Location = new Point(6, 138),
            Size = new Size(18, 18),
            Font = new Font("Segoe UI", 9F),
            ForeColor = avail ? ThemeManager.Success : ThemeManager.Danger,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var status = new Label
        {
            Text = avail ? "Available" : "Out of Stock",
            Location = new Point(24, 138),
            Size = new Size(125, 18),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = ThemeManager.SecondaryText,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var add = new Button
        {
            Text = "Add +",
            Location = new Point(8, 159),
            Size = new Size(139, 26),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = avail ? ThemeManager.Primary : Color.FromArgb(170, 165, 160),
            Cursor = avail ? Cursors.Hand : Cursors.Default
        };
        add.FlatAppearance.BorderSize = 0;
        if (avail)
        {
            add.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(ThemeManager.Primary, 0.1f);
            add.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(ThemeManager.Primary, 0.2f);
        }
        else
        {
            add.FlatAppearance.MouseOverBackColor = add.BackColor;
            add.FlatAppearance.MouseDownBackColor = add.BackColor;
        }

        void Add(object? s, EventArgs e) { if (avail) AddToCart(p); }
        card.Click += Add;
        colorBlock.Click += Add;
        icon.Click += Add;
        name.Click += Add;
        price.Click += Add;
        dot.Click += Add;
        status.Click += Add;
        add.Click += Add;

        card.Controls.AddRange(new Control[] { colorBlock, name, price, dot, status, add });
        return card;
    }

    // ═════════════════════════ RIGHT: cart & payment ═════════════════════════

    private Control BuildRightPanel()
    {
        _right = new Panel
        {
            Dock = DockStyle.Right,
            Width = 400,
            BackColor = Color.White,
            Padding = new Padding(16, 10, 16, 12),
            AutoScroll = true
        };
        _right.AutoScrollMinSize = new Size(0, 640);
        _right.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(222, 216, 208));
            e.Graphics.DrawLine(pen, 0, 0, 0, _right.Height);
        };

        // Fill: cart
        var cartHost = BuildCartHost();

        // Top stack (add innermost first, topmost last)
        var tablePanel = BuildTablePanel();
        var orderPanel = BuildOrderTypePanel();
        var header = BuildHeader();

        // Bottom stack (add innermost first, bottom-most last)
        var billing = BuildBillingPanel();
        var payment = BuildPaymentPanel();
        var cash = BuildCashPanel();
        var actions = BuildActionPanel();

        _right.Controls.Add(cartHost);
        _right.Controls.Add(tablePanel);
        _right.Controls.Add(orderPanel);
        _right.Controls.Add(header);
        _right.Controls.Add(billing);
        _right.Controls.Add(payment);
        _right.Controls.Add(cash);
        _right.Controls.Add(actions);
        return _right;
    }

    private Control BuildHeader()
    {
        var p = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.White };
        var title = new Label
        {
            Text = "🛒 Current Sale",
            Dock = DockStyle.Fill,
            Font = ThemeManager.TitleFont,
            ForeColor = ThemeManager.MainText,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txnLabel = new Label
        {
            Dock = DockStyle.Right,
            Width = 120,
            Font = ThemeManager.BodyBoldFont,
            ForeColor = ThemeManager.SecondaryText,
            TextAlign = ContentAlignment.MiddleRight
        };
        p.Controls.Add(title);
        p.Controls.Add(_txnLabel);
        return p;
    }

    private Control BuildOrderTypePanel()
    {
        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White
        };
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _dineInBtn = CreateToggleButton("🍽️ Dine In");
        _takeawayBtn = CreateToggleButton("☕ Takeaway");
        _dineInBtn.Margin = new Padding(0, 4, 4, 6);
        _takeawayBtn.Margin = new Padding(4, 4, 0, 6);
        _dineInBtn.Click += (_, _) => SetOrderType(true);
        _takeawayBtn.Click += (_, _) => SetOrderType(false);
        tlp.Controls.Add(_dineInBtn, 0, 0);
        tlp.Controls.Add(_takeawayBtn, 1, 0);
        return tlp;
    }

    private Control BuildTablePanel()
    {
        _tablePanel = new Panel { Dock = DockStyle.Top, Height = 38, Visible = false, BackColor = Color.White };
        var lbl = new Label
        {
            Text = "Table Number:",
            Location = new Point(0, 6),
            Size = new Size(110, 24),
            Font = ThemeManager.BodyFont,
            ForeColor = ThemeManager.MainText,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _tableBox = new TextBox { Location = new Point(114, 5), Width = 90 };
        ThemeManager.StyleTextBox(_tableBox);
        _tablePanel.Controls.Add(lbl);
        _tablePanel.Controls.Add(_tableBox);
        return _tablePanel;
    }

    private Control BuildCartHost()
    {
        var host = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
        host.Paint += (_, e) =>
        {
            using var pen = new Pen(LineColor);
            e.Graphics.DrawRectangle(pen, 0, 0, host.Width - 1, host.Height - 1);
        };

        _cartList = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
        UIHelper.SetDoubleBuffered(_cartList);

        _emptyLabel = new Label
        {
            Text = "Cart is empty",
            Dock = DockStyle.Fill,
            Font = ThemeManager.BodyFont,
            ForeColor = ThemeManager.SecondaryText,
            TextAlign = ContentAlignment.MiddleCenter
        };

        host.Controls.Add(_cartList);
        host.Controls.Add(_emptyLabel);
        return host;
    }

    private Control BuildBillingPanel()
    {
        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 144,
            Padding = new Padding(0, 6, 0, 0),
            ColumnCount = 2,
            RowCount = 5,
            BackColor = Color.White
        };
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        _subtotalValue = CellLabel("$0.00", ThemeManager.BodyBoldFont, ThemeManager.MainText, ContentAlignment.MiddleRight);
        _taxValue = CellLabel("$0.00", ThemeManager.BodyBoldFont, ThemeManager.MainText, ContentAlignment.MiddleRight);
        _totalValue = CellLabel("$0.00", new Font("Segoe UI", 16F, FontStyle.Bold), ThemeManager.Primary, ContentAlignment.MiddleRight);

        _discountBox = new TextBox
        {
            Text = "0.00",
            Width = 90,
            TextAlign = HorizontalAlignment.Right,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0)
        };
        ThemeManager.StyleTextBox(_discountBox);
        _discountBox.KeyPress += NumericKeyPress;
        _discountBox.TextChanged += (_, _) => UpdateTotals();
        _discountBox.Leave += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_discountBox.Text)) _discountBox.Text = "0.00";
        };
        _discountBox.Enter += (_, _) => _discountBox.SelectAll();

        var separator = new Panel { Height = 1, BackColor = LineColor, Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(0) };

        tlp.Controls.Add(CellLabel("Subtotal:", ThemeManager.BodyFont, ThemeManager.SecondaryText, ContentAlignment.MiddleLeft), 0, 0);
        tlp.Controls.Add(_subtotalValue, 1, 0);
        tlp.Controls.Add(CellLabel("Discount: $", ThemeManager.BodyFont, ThemeManager.SecondaryText, ContentAlignment.MiddleLeft), 0, 1);
        tlp.Controls.Add(_discountBox, 1, 1);
        _taxLabel = CellLabel($"Tax ({TaxRatePercent.ToString("0.##", Inv)}%):", ThemeManager.BodyFont, ThemeManager.SecondaryText, ContentAlignment.MiddleLeft);
        tlp.Controls.Add(_taxLabel, 0, 2);
        tlp.Controls.Add(_taxValue, 1, 2);
        tlp.Controls.Add(separator, 0, 3);
        tlp.SetColumnSpan(separator, 2);
        tlp.Controls.Add(CellLabel("TOTAL:", new Font("Segoe UI", 16F, FontStyle.Bold), ThemeManager.Primary, ContentAlignment.MiddleLeft), 0, 4);
        tlp.Controls.Add(_totalValue, 1, 4);
        return tlp;
    }

    private Control BuildPaymentPanel()
    {
        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(0, 4, 0, 0),
            BackColor = Color.White
        };
        for (int i = 0; i < 3; i++) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3f));
        tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _cashBtn = CreateToggleButton("💵 Cash");
        _cardBtn = CreateToggleButton("💳 Card");
        _otherBtn = CreateToggleButton("📱 Other");
        _cashBtn.Margin = new Padding(0, 0, 4, 6);
        _cardBtn.Margin = new Padding(4, 0, 4, 6);
        _otherBtn.Margin = new Padding(4, 0, 0, 6);
        _cashBtn.Click += (_, _) => SetPayment(PaymentMethod.Cash);
        _cardBtn.Click += (_, _) => SetPayment(PaymentMethod.Card);
        _otherBtn.Click += (_, _) => SetPayment(PaymentMethod.Other);
        tlp.Controls.Add(_cashBtn, 0, 0);
        tlp.Controls.Add(_cardBtn, 1, 0);
        tlp.Controls.Add(_otherBtn, 2, 0);
        return tlp;
    }

    private Control BuildCashPanel()
    {
        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.White
        };
        _cashPanel = tlp;
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        _receivedBox = new TextBox
        {
            Width = 120,
            TextAlign = HorizontalAlignment.Right,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0)
        };
        ThemeManager.StyleTextBox(_receivedBox);
        _receivedBox.KeyPress += NumericKeyPress;
        _receivedBox.TextChanged += (_, _) => UpdateChange();

        _changeValue = CellLabel("$0.00", ThemeManager.BodyBoldFont, ThemeManager.SecondaryText, ContentAlignment.MiddleRight);

        tlp.Controls.Add(CellLabel("Amount Received:", ThemeManager.BodyFont, ThemeManager.SecondaryText, ContentAlignment.MiddleLeft), 0, 0);
        tlp.Controls.Add(_receivedBox, 1, 0);
        tlp.Controls.Add(CellLabel("Change Due:", ThemeManager.BodyFont, ThemeManager.SecondaryText, ContentAlignment.MiddleLeft), 0, 1);
        tlp.Controls.Add(_changeValue, 1, 1);
        return tlp;
    }

    private Control BuildActionPanel()
    {
        var tlp = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 100,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 4, 0, 0),
            BackColor = Color.White
        };
        tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var complete = UIHelper.CreateIconButton("✅ Complete Sale", ThemeManager.Success);
        complete.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        complete.Dock = DockStyle.Fill;
        complete.Margin = new Padding(0, 0, 0, 6);
        complete.Click += (_, _) => CompleteSale();

        var clear = UIHelper.CreateIconButton("🗑️ Clear Cart", ThemeManager.Danger);
        clear.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        clear.Dock = DockStyle.Fill;
        clear.MinimumSize = new Size(0, 0);
        clear.Margin = new Padding(0, 0, 0, 4);
        clear.Click += (_, _) => ClearCartWithConfirm();

        tlp.Controls.Add(complete, 0, 0);
        tlp.Controls.Add(clear, 0, 1);
        return tlp;
    }

    private static Label CellLabel(string text, Font font, Color color, ContentAlignment align) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        AutoSize = false,
        Font = font,
        ForeColor = color,
        TextAlign = align,
        Margin = new Padding(0)
    };

    private static Button CreateToggleButton(string text)
    {
        var b = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    private static void NumericKeyPress(object? sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar)) return;
        if (e.KeyChar == '.' && sender is TextBox tb && !tb.Text.Contains('.')) return;
        e.Handled = true;
    }

    private static string Money(decimal v) => "$" + v.ToString("N2", Inv);

    // ═════════════════════════ Mode toggles ═════════════════════════

    private void SetOrderType(bool dineIn)
    {
        _isDineIn = dineIn;
        SetToggleStyle(_dineInBtn, dineIn);
        SetToggleStyle(_takeawayBtn, !dineIn);
        _tablePanel.Visible = dineIn;
        if (!dineIn) _tableBox.Clear();
    }

    private void SetPayment(PaymentMethod method)
    {
        _payment = method;
        SetToggleStyle(_cashBtn, method == PaymentMethod.Cash);
        SetToggleStyle(_cardBtn, method == PaymentMethod.Card);
        SetToggleStyle(_otherBtn, method == PaymentMethod.Other);
        _cashPanel.Visible = method == PaymentMethod.Cash;
        UpdateChange();
    }

    // ═════════════════════════ Cart ═════════════════════════

    private void NewTransaction() => _txnLabel.Text = _txnNumber = MockData.NextTransactionId();

    private void AddToCart(Product p)
    {
        if (!p.IsAvailable) return;

        var line = _cart.FirstOrDefault(l => l.Product.Id == p.Id);
        if (line != null)
        {
            line.Quantity++;
            RefreshLine(line);
        }
        else
        {
            line = new CartLine { Product = p, Quantity = 1 };
            BuildCartRow(line);
            _cart.Add(line);
            _cartList.Controls.Add(line.Row);
            line.Row.BringToFront(); // Dock=Top: front-most is placed last, i.e. new rows append at the bottom
            _cartList.ScrollControlIntoView(line.Row);
        }
        UpdateCartState();
    }

    private void ChangeQuantity(CartLine line, int delta)
    {
        line.Quantity += delta;
        if (line.Quantity <= 0) { RemoveLine(line); return; }
        RefreshLine(line);
        UpdateTotals();
    }

    private void RemoveLine(CartLine line)
    {
        _cart.Remove(line);
        _cartList.Controls.Remove(line.Row);
        line.Row.Dispose();
        UpdateCartState();
    }

    private void RefreshLine(CartLine line)
    {
        line.QtyLabel.Text = line.Quantity.ToString();
        line.TotalLabel.Text = Money(line.Product.SellingPrice * line.Quantity);
    }

    private void BuildCartRow(CartLine line)
    {
        var p = line.Product;
        var row = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.White };
        row.Paint += (_, e) =>
        {
            using var pen = new Pen(LineColor);
            e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1);
        };

        var name = new Label
        {
            Text = p.Name,
            AutoSize = false,
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = ThemeManager.MainText,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var remove = CreateRowButton("✕");
        remove.ForeColor = ThemeManager.Danger;
        remove.BackColor = Color.White;
        remove.FlatAppearance.MouseOverBackColor = Color.FromArgb(252, 235, 235);
        remove.Click += (_, _) => RemoveLine(line);

        var unit = new Label
        {
            Text = Money(p.SellingPrice),
            AutoSize = false,
            Font = ThemeManager.SmallFont,
            ForeColor = ThemeManager.SecondaryText,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var minus = CreateRowButton("-");
        var plus = CreateRowButton("+");
        minus.Click += (_, _) => ChangeQuantity(line, -1);
        plus.Click += (_, _) => ChangeQuantity(line, +1);

        var qty = new Label
        {
            AutoSize = false,
            Font = ThemeManager.BodyBoldFont,
            ForeColor = ThemeManager.MainText,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var total = new Label
        {
            AutoSize = false,
            Font = ThemeManager.BodyBoldFont,
            ForeColor = ThemeManager.MainText,
            TextAlign = ContentAlignment.MiddleRight
        };

        line.Row = row;
        line.QtyLabel = qty;
        line.TotalLabel = total;
        RefreshLine(line);

        void Place()
        {
            int w = row.ClientSize.Width;
            name.SetBounds(6, 5, Math.Max(40, w - 6 - 38), 24);
            remove.SetBounds(w - 32, 5, 28, 24);
            unit.SetBounds(6, 31, 80, 22);
            minus.SetBounds(96, 30, 26, 24);
            qty.SetBounds(124, 30, 32, 24);
            plus.SetBounds(158, 30, 26, 24);
            total.SetBounds(Math.Max(190, w - 112), 30, Math.Min(106, Math.Max(40, w - 194)), 24);
        }
        row.Resize += (_, _) => Place();
        row.Controls.AddRange(new Control[] { name, remove, unit, minus, qty, plus, total });
        Place();
    }

    private static Button CreateRowButton(string text)
    {
        var b = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = ThemeManager.MainText,
            BackColor = LightButton,
            Cursor = Cursors.Hand,
            Padding = new Padding(0),
            UseVisualStyleBackColor = false
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(228, 216, 198);
        return b;
    }

    private void UpdateCartState()
    {
        bool empty = _cart.Count == 0;
        _emptyLabel.Visible = empty;
        _cartList.Visible = !empty;
        UpdateTotals();
    }

    private void ClearCartInternal()
    {
        foreach (var l in _cart) { _cartList.Controls.Remove(l.Row); l.Row.Dispose(); }
        _cart.Clear();
        UpdateCartState();
    }

    private void ClearCartWithConfirm()
    {
        if (_cart.Count == 0) return;
        var r = MessageBox.Show(FindForm(), "Remove all items from the current sale?", "Clear Cart",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r == DialogResult.Yes) ClearCartInternal();
    }

    // ═════════════════════════ Totals ═════════════════════════

    private void UpdateTotals()
    {
        _subtotal = _cart.Sum(l => l.Product.SellingPrice * l.Quantity);

        string t = _discountBox.Text.Trim();
        decimal disc = 0m;
        bool ok = t.Length == 0 || (decimal.TryParse(t, NumberStyles.Number, Inv, out disc) && disc >= 0);
        if (!ok) disc = 0m;
        if (disc > _subtotal)
        {
            ok = _subtotal == 0 && disc == 0;
            disc = _subtotal;
        }
        _discountValid = ok;
        _discountBox.BackColor = ok ? ThemeManager.CardBackground : Color.FromArgb(253, 232, 232);
        _discount = disc;

        decimal taxable = _subtotal - _discount;
        _tax = Math.Round(taxable * TaxRatePercent / 100m, 2, MidpointRounding.AwayFromZero);
        _total = taxable + _tax;

        _subtotalValue.Text = Money(_subtotal);
        if (_taxLabel != null)
            _taxLabel.Text = $"Tax ({TaxRatePercent.ToString("0.##", Inv)}%):";
        _taxValue.Text = Money(_tax);
        _totalValue.Text = Money(_total);
        UpdateChange();
    }

    private bool TryGetReceived(out decimal received)
    {
        received = 0m;
        string t = _receivedBox.Text.Trim();
        return t.Length > 0 && decimal.TryParse(t, NumberStyles.Number, Inv, out received) && received >= 0;
    }

    private void UpdateChange()
    {
        if (_changeValue == null || _receivedBox == null) return;
        if (_receivedBox.Text.Trim().Length == 0)
        {
            _changeValue.Text = "$0.00";
            _changeValue.ForeColor = ThemeManager.SecondaryText;
            return;
        }
        if (!TryGetReceived(out var received))
        {
            _changeValue.Text = "Invalid";
            _changeValue.ForeColor = ThemeManager.Danger;
            return;
        }
        decimal change = received - _total;
        _changeValue.Text = (change < 0 ? "-" : "") + Money(Math.Abs(change));
        _changeValue.ForeColor = change >= 0 ? ThemeManager.Success : ThemeManager.Danger;
    }

    // ═════════════════════════ Complete sale ═════════════════════════

    private void CompleteSale()
    {
        var owner = FindForm();

        if (_cart.Count == 0)
        {
            MessageBox.Show(owner, "The cart is empty. Add at least one product.", "Cannot Complete Sale",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!_discountValid)
        {
            MessageBox.Show(owner, "Please enter a valid discount (not negative and not more than the subtotal).",
                "Invalid Discount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _discountBox.Focus();
            return;
        }

        decimal received = 0m;
        if (_payment == PaymentMethod.Cash)
        {
            if (!TryGetReceived(out received))
            {
                MessageBox.Show(owner, "Please enter the amount received from the customer.", "Cash Payment",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _receivedBox.Focus();
                return;
            }
            if (received < _total)
            {
                MessageBox.Show(owner,
                    $"Amount received ({Money(received)}) is less than the total ({Money(_total)}).",
                    "Insufficient Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _receivedBox.Focus();
                return;
            }
        }
        decimal change = _payment == PaymentMethod.Cash ? received - _total : 0m;

        var sb = new StringBuilder();
        sb.AppendLine($"Transaction: {_txnNumber}");
        sb.AppendLine($"Order type: {(_isDineIn ? "Dine In" : "Takeaway")}" +
                      (_isDineIn && _tableBox.Text.Trim().Length > 0 ? $" (Table {_tableBox.Text.Trim()})" : ""));
        sb.AppendLine();
        foreach (var l in _cart)
            sb.AppendLine($"  {l.Quantity} x {l.Product.Name}  —  {Money(l.Product.SellingPrice * l.Quantity)}");
        sb.AppendLine();
        sb.AppendLine($"Subtotal: {Money(_subtotal)}");
        sb.AppendLine($"Discount: {Money(_discount)}");
        sb.AppendLine($"Tax ({TaxRatePercent:0.##}%): {Money(_tax)}");
        sb.AppendLine($"TOTAL: {Money(_total)}");
        sb.AppendLine($"Payment: {_payment}");
        if (_payment == PaymentMethod.Cash)
        {
            sb.AppendLine($"Received: {Money(received)}");
            sb.AppendLine($"Change: {Money(change)}");
        }
        sb.AppendLine();
        sb.Append("Complete this sale?");

        if (MessageBox.Show(owner, sb.ToString(), "Confirm Sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var cashier = MockData.CurrentCashier ?? MockData.Cashiers.FirstOrDefault();
        var sale = new Sale
        {
            Id = MockData.NextSaleId(),
            TransactionId = _txnNumber,
            DateTime = DateTime.Now,
            CashierId = cashier?.Id ?? 0,
            CashierName = cashier?.Name ?? "Unknown",
            OrderType = _isDineIn ? OrderType.DineIn : OrderType.Takeaway,
            Items = _cart.Select(l => new SaleItem
            {
                ProductId = l.Product.Id,
                ProductName = l.Product.Name,
                UnitPrice = l.Product.SellingPrice,
                Quantity = l.Quantity
            }).ToList(),
            Subtotal = _subtotal,
            Discount = _discount,
            TaxRate = TaxRatePercent,
            Tax = _tax,
            Total = _total,
            PaymentMethod = _payment,
            AmountReceived = _payment == PaymentMethod.Cash ? received : _total,
            ChangeDue = change,
            Status = SaleStatus.Completed
        };
        MockData.Sales.Add(sale);

        ResetForm();
        MessageBox.Show(owner, "Sale completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ResetForm()
    {
        ClearCartInternal();
        _discountBox.Text = "0.00";
        _receivedBox.Clear();
        _tableBox.Clear();
        SetOrderType(false);
        SetPayment(PaymentMethod.Cash);
        NewTransaction();
        UpdateTotals();
    }
}

