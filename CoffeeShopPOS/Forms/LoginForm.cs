using System.Drawing.Drawing2D;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.Forms;

/// <summary>
/// Coffee-themed login screen with mock authentication against MockData cashiers.
/// </summary>
public partial class LoginForm : Form
{
    private Panel _topPanel = null!;
    private Label _closeButton = null!;
    private Label _logoLabel = null!;
    private Label _welcomeLabel = null!;
    private Label _hintLabel = null!;
    private Label _userLabel = null!;
    private Label _passLabel = null!;
    private TextBox _usernameBox = null!;
    private TextBox _passwordBox = null!;
    private Button _toggleButton = null!;
    private CheckBox _rememberCheck = null!;
    private Button _loginButton = null!;
    private Label _errorLabel = null!;
    private Label _footerLabel = null!;

    private bool _dragging;
    private Point _dragStart;
    private string _rememberedUser = string.Empty;

    public LoginForm()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        SuspendLayout();

        Text = "Brew Haven POS - Login";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(500, 600);
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;
        DoubleBuffered = true;
        KeyPreview = true;

        const int margin = 60;
        int fieldWidth = ClientSize.Width - margin * 2;

        // ── Top drag panel ──
        _topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = ThemeManager.MainBackground
        };
        _topPanel.MouseDown += DragMouseDown;
        _topPanel.MouseMove += DragMouseMove;
        _topPanel.MouseUp += (s, e) => _dragging = false;

        _closeButton = new Label
        {
            Text = "✕",
            Font = new Font("Segoe UI", 12F, FontStyle.Regular),
            ForeColor = ThemeManager.SecondaryText,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(46, 40),
            Dock = DockStyle.Right,
            Cursor = Cursors.Hand
        };
        _closeButton.Click += (s, e) => Application.Exit();
        _closeButton.MouseEnter += (s, e) => { _closeButton.BackColor = ThemeManager.Danger; _closeButton.ForeColor = Color.White; };
        _closeButton.MouseLeave += (s, e) => { _closeButton.BackColor = Color.Transparent; _closeButton.ForeColor = ThemeManager.SecondaryText; };
        _topPanel.Controls.Add(_closeButton);

        // ── Logo ──
        _logoLabel = new Label
        {
            Text = "☕ Brew Haven",
            Font = new Font("Segoe UI", 30F, FontStyle.Bold),
            ForeColor = ThemeManager.Primary,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(ClientSize.Width, 70),
            Location = new Point(0, 70)
        };

        _welcomeLabel = new Label
        {
            Text = "Welcome Back",
            Font = ThemeManager.SubHeaderFont,
            ForeColor = ThemeManager.MainText,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(ClientSize.Width, 36),
            Location = new Point(0, 145)
        };

        _hintLabel = new Label
        {
            Text = "Sign in to continue to your POS dashboard",
            Font = ThemeManager.SmallFont,
            ForeColor = ThemeManager.SecondaryText,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(ClientSize.Width, 24),
            Location = new Point(0, 183)
        };

        // ── Username ──
        _userLabel = new Label
        {
            Text = "Username",
            Font = ThemeManager.SmallBoldFont,
            ForeColor = ThemeManager.MainText,
            AutoSize = true,
            Location = new Point(margin, 235)
        };

        _usernameBox = new TextBox
        {
            Location = new Point(margin, 258),
            Width = fieldWidth,
            PlaceholderText = "Enter your username"
        };
        ThemeManager.StyleTextBox(_usernameBox);
        _usernameBox.Font = new Font("Segoe UI", 12F);

        // ── Password ──
        _passLabel = new Label
        {
            Text = "Password",
            Font = ThemeManager.SmallBoldFont,
            ForeColor = ThemeManager.MainText,
            AutoSize = true,
            Location = new Point(margin, 308)
        };

        _passwordBox = new TextBox
        {
            Location = new Point(margin, 331),
            Width = fieldWidth - 50,
            UseSystemPasswordChar = true,
            PlaceholderText = "Enter your password"
        };
        ThemeManager.StyleTextBox(_passwordBox);
        _passwordBox.Font = new Font("Segoe UI", 12F);

        _toggleButton = new Button
        {
            Text = "👁",
            Size = new Size(44, _passwordBox.Height),
            Location = new Point(margin + fieldWidth - 44, 331),
            Font = new Font("Segoe UI Emoji", 11F),
            TabStop = false
        };
        ThemeManager.StyleButton(_toggleButton, false);
        _toggleButton.Font = new Font("Segoe UI Emoji", 11F);
        _toggleButton.Padding = Padding.Empty;
        _toggleButton.Click += (s, e) =>
        {
            _passwordBox.UseSystemPasswordChar = !_passwordBox.UseSystemPasswordChar;
            _toggleButton.Text = _passwordBox.UseSystemPasswordChar ? "👁" : "🙈";
            _passwordBox.Focus();
        };

        // ── Remember me ──
        _rememberCheck = new CheckBox
        {
            Text = "Remember Me",
            Font = ThemeManager.SmallFont,
            ForeColor = ThemeManager.MainText,
            AutoSize = true,
            Location = new Point(margin, 385),
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat
        };

        // ── Error label ──
        _errorLabel = new Label
        {
            Text = "Invalid username or password",
            Font = ThemeManager.SmallBoldFont,
            ForeColor = ThemeManager.Danger,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(fieldWidth, 24),
            Location = new Point(margin, 420),
            Visible = false
        };

        // ── Login button ──
        _loginButton = new Button
        {
            Text = "LOGIN",
            Size = new Size(fieldWidth, 50),
            Location = new Point(margin, 455)
        };
        ThemeManager.StyleButton(_loginButton, true);
        _loginButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _loginButton.Click += (s, e) => AttemptLogin();
        AcceptButton = _loginButton;

        // ── Footer ──
        _footerLabel = new Label
        {
            Text = "Demo: admin / admin123   •   cashier / pass123",
            Font = ThemeManager.SmallFont,
            ForeColor = ThemeManager.SecondaryText,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(ClientSize.Width, 24),
            Location = new Point(0, 540)
        };

        _usernameBox.TextChanged += (s, e) => _errorLabel.Visible = false;
        _passwordBox.TextChanged += (s, e) => _errorLabel.Visible = false;

        Controls.AddRange(new Control[]
        {
            _topPanel, _logoLabel, _welcomeLabel, _hintLabel,
            _userLabel, _usernameBox, _passLabel, _passwordBox, _toggleButton,
            _rememberCheck, _errorLabel, _loginButton, _footerLabel
        });

        ResumeLayout(false);
    }

    // ───────────── Dragging ─────────────
    private void DragMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        _dragging = true;
        _dragStart = e.Location;
    }

    private void DragMouseMove(object? sender, MouseEventArgs e)
    {
        if (!_dragging) return;
        Point screen = PointToScreen(e.Location);
        // e.Location is relative to the top panel, which sits at the form's origin.
        Location = new Point(screen.X - _dragStart.X, screen.Y - _dragStart.Y);
    }

    // ───────────── Authentication ─────────────
    private void AttemptLogin()
    {
        string username = _usernameBox.Text.Trim();
        string password = _passwordBox.Text;

        var cashier = MockData.GetCashiers().FirstOrDefault(c =>
            string.Equals(c.Username, username, StringComparison.OrdinalIgnoreCase) &&
            c.Password == password);

        if (cashier == null)
        {
            _errorLabel.Text = "Invalid username or password";
            _errorLabel.Visible = true;
            _passwordBox.SelectAll();
            _passwordBox.Focus();
            return;
        }

        _errorLabel.Visible = false;
        _rememberedUser = _rememberCheck.Checked ? username : string.Empty;

        MockData.CurrentCashier = cashier;
        Hide();
        var main = new MainForm(cashier);
        main.FormClosed += (s, e) =>
        {
            MockData.CurrentCashier = null;
            _passwordBox.Clear();
            _passwordBox.UseSystemPasswordChar = true;
            _toggleButton.Text = "👁";
            _usernameBox.Text = _rememberedUser;
            _rememberCheck.Checked = !string.IsNullOrEmpty(_rememberedUser);
            _errorLabel.Visible = false;
            Show();
            (string.IsNullOrEmpty(_rememberedUser) ? _usernameBox : _passwordBox).Focus();
        };
        main.Show();
    }

    // ───────────── Border ─────────────
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(ThemeManager.Primary, 2);
        e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
    }
}

