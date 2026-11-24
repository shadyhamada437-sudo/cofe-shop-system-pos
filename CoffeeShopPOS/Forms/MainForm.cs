using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;
using CoffeeShopPOS.UserControls;

namespace CoffeeShopPOS.Forms;

/// <summary>
/// Main application shell: sidebar navigation, top bar and a swappable content area.
/// </summary>
public partial class MainForm : Form
{
    private sealed record NavItem(string Text, string Title, string PageName, string TypeName);

    private static readonly Color SidebarHover = Color.FromArgb(50, 45, 40);

    private static readonly NavItem[] NavItems =
    {
        new("📊  Dashboard",              "Dashboard",           "Dashboard",           "DashboardControl"),
        new("🛒  POS / New Sale",         "POS / New Sale",      "POS",                 "POSControl"),
        new("📋  Sales / Transactions",   "Sales / Transactions","Sales",               "SalesControl"),
        new("☕  Products / Menu",        "Products / Menu",     "Products",            "ProductsControl"),
        new("📁  Categories",             "Categories",          "Categories",          "CategoriesControl"),
        new("🧾  Billing / Receipts",     "Billing / Receipts",  "Receipts",            "ReceiptControl"),
        new("📈  Sales Analytics",        "Sales Analytics",     "Sales Analytics",     "AnalyticsControl"),
        new("⚙️  Settings",               "Settings",            "Settings",            "SettingsControl"),
    };

    private Panel _sidebar = null!;
    private Panel _brandPanel = null!;
    private Panel _navPanel = null!;
    private Panel _topBar = null!;
    private Panel _contentPanel = null!;
    private Label _titleLabel = null!;
    private Label _dateTimeLabel = null!;
    private Label _userLabel = null!;
    private Label _notificationLabel = null!;
    private Button _logoutButton = null!;
    private System.Windows.Forms.Timer _clockTimer = null!;
    private readonly List<Button> _navButtons = new();
    private Button? _activeButton;

    /// <summary>The currently logged-in cashier.</summary>
    public Cashier CurrentCashier { get; }

    public MainForm(Cashier cashier)
    {
        CurrentCashier = cashier;
        MockData.CurrentCashier = cashier;
        InitializeUI();
        NavigateTo(0);
    }

    private void InitializeUI()
    {
        SuspendLayout();

        Text = "Brew Haven POS";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1200, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;

        BuildContentPanel();
        BuildTopBar();
        BuildSidebar();

        // Dock order (reverse of z-order): Fill added first, then Top, then Left sidebar on top.
        Controls.Add(_contentPanel);
        Controls.Add(_topBar);
        Controls.Add(_sidebar);

        _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _clockTimer.Tick += (s, e) => UpdateClock();
        _clockTimer.Start();
        UpdateClock();

        FormClosing += (s, e) => _clockTimer.Stop();

        ResumeLayout(false);
    }

    // ───────────── Sidebar ─────────────
    private void BuildSidebar()
    {
        _sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 240,
            BackColor = ThemeManager.Sidebar
        };

        // Brand section
        _brandPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 90,
            BackColor = ThemeManager.Sidebar
        };
        var brandLabel = new Label
        {
            Text = "☕ Brew Haven",
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = ThemeManager.Accent,
            AutoSize = false,
            TextAlign = ContentAlignment.BottomLeft,
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(20, 0, 0, 0)
        };
        var subLabel = new Label
        {
            Text = "POS System",
            Font = ThemeManager.SmallFont,
            ForeColor = ThemeManager.SecondaryText,
            AutoSize = false,
            TextAlign = ContentAlignment.TopLeft,
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 2, 0, 0)
        };
        _brandPanel.Controls.Add(subLabel);
        _brandPanel.Controls.Add(brandLabel);

        // Logout (bottom)
        _logoutButton = CreateNavButton("🚪  Logout");
        _logoutButton.Dock = DockStyle.Bottom;
        _logoutButton.Click += (s, e) => Logout();

        // Nav buttons container
        _navPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeManager.Sidebar
        };

        _navButtons.Clear();
        for (int i = 0; i < NavItems.Length; i++)
        {
            int index = i;
            var btn = CreateNavButton(NavItems[i].Text);
            btn.Dock = DockStyle.Top;
            btn.Click += (s, e) => NavigateTo(index);
            _navButtons.Add(btn);
        }
        // Dock=Top stacks in reverse z-order: add last first so the first item is at the top.
        for (int i = _navButtons.Count - 1; i >= 0; i--)
            _navPanel.Controls.Add(_navButtons[i]);

        // Order matters: Fill first, then Bottom, then Top
        _sidebar.Controls.Add(_navPanel);
        _sidebar.Controls.Add(_logoutButton);
        _sidebar.Controls.Add(_brandPanel);
    }

    private Button CreateNavButton(string text)
    {
        var btn = new Button
        {
            Text = text,
            Height = 45,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = ThemeManager.Sidebar,
            Font = ThemeManager.SidebarFont,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(20, 0, 0, 0),
            Cursor = Cursors.Hand,
            TabStop = false,
            UseVisualStyleBackColor = false
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = SidebarHover;
        btn.FlatAppearance.MouseDownBackColor = ThemeManager.Primary;

        btn.MouseEnter += (s, e) =>
        {
            if (btn != _activeButton) btn.BackColor = SidebarHover;
        };
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = btn == _activeButton ? ThemeManager.Primary : ThemeManager.Sidebar;
        };
        return btn;
    }

    // ───────────── Top bar ─────────────
    private void BuildTopBar()
    {
        _topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = ThemeManager.CardBackground
        };
        _topBar.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(225, 225, 225), 1);
            e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
        };

        _titleLabel = new Label
        {
            Text = "Dashboard",
            Font = ThemeManager.SubHeaderFont,
            ForeColor = ThemeManager.MainText,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Left,
            Width = 420,
            Padding = new Padding(24, 0, 0, 0)
        };

        _notificationLabel = new Label
        {
            Text = "🔔",
            Font = new Font("Segoe UI Emoji", 14F),
            ForeColor = ThemeManager.Primary,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Right,
            Width = 56,
            Cursor = Cursors.Hand
        };
        _notificationLabel.Click += (s, e) =>
            MessageBox.Show("You have no new notifications.", "Notifications",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

        _userLabel = new Label
        {
            Text = $"👤 {CurrentCashier.Name} ({CurrentCashier.Role})",
            Font = ThemeManager.BodyBoldFont,
            ForeColor = ThemeManager.MainText,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Right,
            Width = 260
        };

        _dateTimeLabel = new Label
        {
            Font = ThemeManager.BodyFont,
            ForeColor = ThemeManager.SecondaryText,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Right,
            Width = 270
        };

        // Right-docked controls: last added is outermost-left; add so order is [time | user | bell]
        _topBar.Controls.Add(_titleLabel);
        _topBar.Controls.Add(_dateTimeLabel);
        _topBar.Controls.Add(_userLabel);
        _topBar.Controls.Add(_notificationLabel);
    }

    private void UpdateClock()
    {
        _dateTimeLabel.Text = DateTime.Now.ToString("ddd, dd MMM yyyy  hh:mm:ss tt");
    }

    // ───────────── Content ─────────────
    private void BuildContentPanel()
    {
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeManager.MainBackground
        };
    }

    /// <summary>Replace the content area with the given control.</summary>
    public void LoadPage(UserControl control)
    {
        _contentPanel.SuspendLayout();
        var old = _contentPanel.Controls.Cast<Control>().ToList();
        _contentPanel.Controls.Clear();
        foreach (var c in old) c.Dispose();
        control.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(control);
        _contentPanel.ResumeLayout(true);
    }

    // ───────────── Navigation ─────────────
    private void NavigateTo(int index)
    {
        var item = NavItems[index];
        SetActiveButton(_navButtons[index]);
        _titleLabel.Text = item.Title;
        LoadPage(CreatePage(item));
    }

    private void SetActiveButton(Button active)
    {
        foreach (var b in _navButtons)
        {
            b.BackColor = ThemeManager.Sidebar;
            b.Font = ThemeManager.SidebarFont;
        }
        _activeButton = active;
        active.BackColor = ThemeManager.Primary;
        active.Font = ThemeManager.SidebarBoldFont;
    }

    private UserControl CreatePage(NavItem item)
    {
        try
        {
            var type = typeof(MainForm).Assembly.GetType($"CoffeeShopPOS.UserControls.{item.TypeName}");
            if (type != null && typeof(UserControl).IsAssignableFrom(type))
            {
                if (Activator.CreateInstance(type) is UserControl uc)
                    return uc;
            }
        }
        catch (Exception ex)
        {
            return CreatePlaceholder($"{item.PageName} (error: {ex.InnerException?.Message ?? ex.Message})");
        }
        return CreatePlaceholder(item.PageName);
    }

    private static UserControl CreatePlaceholder(string pageName)
    {
        var uc = new UserControl { BackColor = ThemeManager.MainBackground };
        var label = new Label
        {
            Text = $"Loading {pageName}...",
            Font = ThemeManager.SubHeaderFont,
            ForeColor = ThemeManager.SecondaryText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        uc.Controls.Add(label);
        return uc;
    }

    // ───────────── Logout ─────────────
    private void Logout()
    {
        var result = MessageBox.Show("Are you sure you want to log out?", "Logout",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
            Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _clockTimer?.Dispose();
        base.Dispose(disposing);
    }
}

