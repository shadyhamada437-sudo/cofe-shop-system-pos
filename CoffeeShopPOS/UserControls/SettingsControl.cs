using System.Drawing.Drawing2D;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.UserControls;

/// <summary>
/// Application settings screen (shop info, financial, receipt, system, about).
/// Saved values are written to <see cref="MockData.Settings"/>.
/// </summary>
public partial class SettingsControl : UserControl
{
    private static readonly string[] CurrencyItems =
    {
        "USD ($)", "EUR (€)", "EGP (E£)", "SAR (﷼)", "AED (د.إ)"
    };

    private static AppSettings FactoryDefaults() => new()
    {
        ShopName = "Brew Haven",
        Address = "123 Coffee Street, Cityville",
        Phone = "(555) 123-4567",
        Currency = "$",
        TaxRate = 14m,
        DefaultDiscount = 0m,
        ReceiptFooter = "Thank you for visiting Brew Haven!",
        LogoPath = string.Empty
    };

    private const int LabelX = 20;
    private const int InputX = 190;

    // ───────────── Controls ─────────────
    private readonly TextBox _shopName = new();
    private readonly TextBox _address = new();
    private readonly TextBox _phone = new();
    private readonly ComboBox _currency = new();
    private readonly TextBox _taxRate = new();
    private readonly TextBox _discount = new();
    private readonly TextBox _footer = new();
    private readonly ComboBox _theme = new();
    private readonly Label _currentUserLabel = new();
    private readonly Label _errorLabel = new();

    /// <summary>Text shown for the logged-in user in the System card (can be set by the main form).</summary>
    public string CurrentUser
    {
        get => _currentUserLabel.Text;
        set => _currentUserLabel.Text = value;
    }

    public SettingsControl()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;
        AutoScroll = true;
        AutoScrollMinSize = new Size(760, 0);
        Padding = new Padding(30);

        BuildLayout();
        LoadSettings(MockData.Settings);
        if (MockData.CurrentCashier != null)
            _currentUserLabel.Text = $"{MockData.CurrentCashier.Name} ({MockData.CurrentCashier.Role})";
    }

    // ───────────── Layout ─────────────
    private void BuildLayout()
    {
        var title = new Label
        {
            Text = "⚙️ Settings",
            Font = ThemeManager.SubHeaderFont,
            ForeColor = ThemeManager.MainText,
            Dock = DockStyle.Top,
            Height = 54,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var shopCard = BuildShopCard();
        var financeCard = BuildFinancialCard();
        var receiptCard = BuildReceiptCard();
        var systemCard = BuildSystemCard();
        var aboutCard = BuildAboutCard();

        _errorLabel.Dock = DockStyle.Top;
        _errorLabel.AutoSize = false;
        _errorLabel.Height = 0;
        _errorLabel.Visible = false;

        var buttons = BuildButtonBar();

        // Last added is docked first (topmost) → add in reverse visual order.
        Controls.Add(buttons);
        Controls.Add(_errorLabel);
        Controls.Add(aboutCard);
        Controls.Add(systemCard);
        Controls.Add(receiptCard);
        Controls.Add(financeCard);
        Controls.Add(shopCard);
        Controls.Add(title);
    }

    private static (Panel Wrapper, Panel Card) CreateCard(string title, int contentHeight)
    {
        var card = new Panel { Dock = DockStyle.Fill };
        ThemeManager.StyleCard(card);
        card.Controls.Add(new Label
        {
            Text = title,
            Font = ThemeManager.TitleFont,
            ForeColor = ThemeManager.Primary,
            AutoSize = true,
            Location = new Point(LabelX, 14)
        });

        var wrapper = new Panel
        {
            Dock = DockStyle.Top,
            Height = contentHeight + 16,
            Padding = new Padding(0, 0, 0, 16)
        };
        wrapper.Controls.Add(card);
        return (wrapper, card);
    }

    private static void AddLabel(Panel card, string text, int y) =>
        card.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            Font = ThemeManager.BodyFont,
            ForeColor = ThemeManager.MainText,
            Location = new Point(LabelX, y + 5)
        });

    private static void Place(Panel card, Control control, int y, int width, int x = InputX)
    {
        control.Location = new Point(x, y);
        control.Width = width;
        card.Controls.Add(control);
    }

    private static void AddSuffix(Panel card, string text, Control after, int y) =>
        card.Controls.Add(new Label
        {
            Text = text,
            AutoSize = true,
            Font = ThemeManager.BodyBoldFont,
            ForeColor = ThemeManager.SecondaryText,
            Location = new Point(after.Right + 6, y + 5)
        });

    private Panel BuildShopCard()
    {
        var (wrapper, card) = CreateCard("Shop Information", 205);

        AddLabel(card, "Shop Name:", 55);
        ThemeManager.StyleTextBox(_shopName);
        Place(card, _shopName, 55, 360);

        AddLabel(card, "Address:", 99);
        ThemeManager.StyleTextBox(_address);
        Place(card, _address, 99, 360);

        AddLabel(card, "Phone:", 143);
        ThemeManager.StyleTextBox(_phone);
        Place(card, _phone, 143, 360);

        // Logo placeholder
        var logo = new Panel
        {
            Size = new Size(100, 100),
            Location = new Point(InputX + 360 + 40, 55),
            BackColor = ThemeManager.MainBackground,
            Cursor = Cursors.Hand
        };
        var logoText = new Label
        {
            Text = "Click to change logo",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = ThemeManager.SmallFont,
            ForeColor = ThemeManager.SecondaryText,
            Cursor = Cursors.Hand,
            Padding = new Padding(6)
        };
        logo.Controls.Add(logoText);
        logo.Paint += (_, e) =>
        {
            using var pen = new Pen(ThemeManager.Secondary, 1) { DashStyle = DashStyle.Dash };
            e.Graphics.DrawRectangle(pen, 0, 0, logo.Width - 1, logo.Height - 1);
        };
        void OnLogoClick(object? s, EventArgs e) =>
            MessageBox.Show("Logo upload will be available in a future version.", "Logo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        logo.Click += OnLogoClick;
        logoText.Click += OnLogoClick;
        card.Controls.Add(logo);

        return wrapper;
    }

    private Panel BuildFinancialCard()
    {
        var (wrapper, card) = CreateCard("Financial Settings", 205);

        AddLabel(card, "Currency:", 55);
        _currency.Items.AddRange(CurrencyItems);
        ThemeManager.StyleComboBox(_currency);
        Place(card, _currency, 55, 200);

        AddLabel(card, "Tax Rate:", 99);
        ThemeManager.StyleTextBox(_taxRate);
        Place(card, _taxRate, 99, 100);
        AddSuffix(card, "%", _taxRate, 99);

        AddLabel(card, "Default Discount:", 143);
        ThemeManager.StyleTextBox(_discount);
        Place(card, _discount, 143, 100);
        AddSuffix(card, "%", _discount, 143);

        return wrapper;
    }

    private Panel BuildReceiptCard()
    {
        var (wrapper, card) = CreateCard("Receipt Settings", 150);

        AddLabel(card, "Receipt Footer:", 55);
        ThemeManager.StyleTextBox(_footer);
        _footer.Multiline = true;
        _footer.ScrollBars = ScrollBars.Vertical;
        _footer.AcceptsReturn = true;
        Place(card, _footer, 55, 360);
        _footer.Height = 60;

        return wrapper;
    }

    private Panel BuildSystemCard()
    {
        var (wrapper, card) = CreateCard("System", 165);

        AddLabel(card, "Theme:", 55);
        _theme.Items.Add("Light Theme");
        ThemeManager.StyleComboBox(_theme);
        Place(card, _theme, 55, 200);
        _theme.SelectedIndex = 0;
        card.Controls.Add(new Label
        {
            Text = "Dark theme coming soon",
            AutoSize = true,
            Font = new Font(ThemeManager.SmallFont, FontStyle.Italic),
            ForeColor = ThemeManager.SecondaryText,
            Location = new Point(_theme.Right + 12, 55 + 7)
        });

        AddLabel(card, "Current User:", 99);
        _currentUserLabel.Text = "Admin";
        _currentUserLabel.AutoSize = true;
        _currentUserLabel.Font = ThemeManager.BodyBoldFont;
        _currentUserLabel.ForeColor = ThemeManager.MainText;
        _currentUserLabel.Location = new Point(InputX, 99 + 5);
        card.Controls.Add(_currentUserLabel);

        return wrapper;
    }

    private static Panel BuildAboutCard()
    {
        var (wrapper, card) = CreateCard("About", 185);

        void AddLine(string text, int y, Font font, Color color) =>
            card.Controls.Add(new Label
            {
                Text = text,
                AutoSize = true,
                Font = font,
                ForeColor = color,
                Location = new Point(LabelX, y)
            });

        AddLine("Brew Haven POS System", 55, ThemeManager.BodyBoldFont, ThemeManager.MainText);
        AddLine("Version 1.0.0 (GUI Preview)", 86, ThemeManager.BodyFont, ThemeManager.SecondaryText);
        AddLine("CS312 — Modern Development Tools", 112, ThemeManager.BodyFont, ThemeManager.SecondaryText);
        AddLine("© 2024 Brew Haven Coffee", 138, ThemeManager.BodyFont, ThemeManager.SecondaryText);

        return wrapper;
    }

    private Panel BuildButtonBar()
    {
        var save = new Button { Text = "💾 Save Settings", Size = new Size(180, 42), Location = new Point(0, 8) };
        ThemeManager.StyleButton(save, true);
        save.Click += (_, _) => SaveSettings();

        var reset = new Button { Text = "🔄 Reset Defaults", Size = new Size(180, 42), Location = new Point(196, 8) };
        ThemeManager.StyleButton(reset, false);
        reset.Click += (_, _) => ResetDefaults();

        var bar = new Panel { Dock = DockStyle.Top, Height = 66 };
        bar.Controls.Add(save);
        bar.Controls.Add(reset);
        return bar;
    }

    // ───────────── Load / Save / Reset ─────────────
    private void LoadSettings(AppSettings s)
    {
        _shopName.Text = s.ShopName;
        _address.Text = s.Address;
        _phone.Text = s.Phone;
        _taxRate.Text = s.TaxRate.ToString("0.##");
        _discount.Text = s.DefaultDiscount.ToString("0.##");
        _footer.Text = s.ReceiptFooter.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

        int index = Array.FindIndex(CurrencyItems, c => CurrencySymbol(c) == s.Currency);
        _currency.SelectedIndex = index >= 0 ? index : 0;
    }

    private static string CurrencySymbol(string item)
    {
        int open = item.IndexOf('(');
        int close = item.LastIndexOf(')');
        return open >= 0 && close > open ? item.Substring(open + 1, close - open - 1) : item;
    }

    private void SaveSettings()
    {
        var errors = new List<string>();
        if (!ValidationHelper.IsRequired(_shopName.Text)) errors.Add("• Shop name is required.");
        if (!ValidationHelper.IsRequired(_phone.Text)) errors.Add("• Phone is required.");
        if (!ValidationHelper.IsValidPercentage(_taxRate.Text.Trim())) errors.Add("• Tax rate must be a number between 0 and 100.");
        if (!ValidationHelper.IsValidPercentage(_discount.Text.Trim())) errors.Add("• Default discount must be a number between 0 and 100.");

        if (errors.Count > 0)
        {
            _errorLabel.Height = errors.Count * 20 + 10;
            ValidationHelper.ShowValidationError(_errorLabel, string.Join(Environment.NewLine, errors));
            return;
        }

        ValidationHelper.ClearValidationError(_errorLabel);
        _errorLabel.Height = 0;

        MockData.Settings = new AppSettings
        {
            ShopName = _shopName.Text.Trim(),
            Address = _address.Text.Trim(),
            Phone = _phone.Text.Trim(),
            Currency = CurrencySymbol(_currency.SelectedItem as string ?? CurrencyItems[0]),
            TaxRate = decimal.Parse(_taxRate.Text.Trim()),
            DefaultDiscount = decimal.Parse(_discount.Text.Trim()),
            ReceiptFooter = _footer.Text.Replace("\r\n", "\n"),
            LogoPath = MockData.Settings.LogoPath
        };

        MessageBox.Show("Settings saved successfully!", "Settings",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ResetDefaults()
    {
        var result = MessageBox.Show(
            "Reset all fields to their default values?\nClick 'Save Settings' afterwards to apply them.",
            "Reset Defaults", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;

        LoadSettings(FactoryDefaults());
        ValidationHelper.ClearValidationError(_errorLabel);
        _errorLabel.Height = 0;
    }
}

