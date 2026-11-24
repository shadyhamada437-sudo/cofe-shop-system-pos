using System.Globalization;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.Dialogs;

/// <summary>
/// Modal dialog to add or edit a product, with inline validation.
/// </summary>
public partial class ProductDialog : Form
{
    private readonly Product? _existing;

    private readonly TextBox _nameBox;
    private readonly ComboBox _categoryBox;
    private readonly TextBox _descBox;
    private readonly TextBox _priceBox;
    private readonly TextBox _costBox;
    private readonly TextBox _stockBox;
    private readonly Panel _imagePanel;
    private readonly CheckBox _availableCheck;

    private readonly Label _nameError;
    private readonly Label _categoryError;
    private readonly Label _priceError;
    private readonly Label _costError;
    private readonly Label _stockError;

    /// <summary>The resulting product (valid after DialogResult.OK).</summary>
    public Product Result { get; private set; }

    public ProductDialog(Product? existing = null)
    {
        _existing = existing;
        Result = existing != null
            ? new Product
            {
                Id = existing.Id, Name = existing.Name, CategoryId = existing.CategoryId,
                CategoryName = existing.CategoryName, Description = existing.Description,
                SellingPrice = existing.SellingPrice, CostPrice = existing.CostPrice,
                StockQuantity = existing.StockQuantity, ImagePath = existing.ImagePath,
                IsAvailable = existing.IsAvailable
            }
            : new Product();

        Text = existing == null ? "Add Product" : "Edit Product";
        Size = new Size(450, 640);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;

        int x = 25, w = 385, y = 12;

        Label MakeLabel(string text, int top) => new()
        {
            Text = text, AutoSize = true, Font = ThemeManager.SmallBoldFont,
            ForeColor = ThemeManager.MainText, Location = new Point(x, top)
        };
        Label MakeError(int top) => new()
        {
            AutoSize = false, Size = new Size(w, 16), Location = new Point(x, top), Visible = false
        };

        // Name
        Controls.Add(MakeLabel("Product Name *", y));
        _nameBox = new TextBox { Location = new Point(x, y + 20), Width = w, Text = Result.Name };
        ThemeManager.StyleTextBox(_nameBox);
        _nameError = MakeError(y + 48);
        Controls.Add(_nameBox); Controls.Add(_nameError);
        y += 66;

        // Category
        Controls.Add(MakeLabel("Category *", y));
        _categoryBox = new ComboBox { Location = new Point(x, y + 20), Width = w };
        ThemeManager.StyleComboBox(_categoryBox);
        var categories = MockData.GetCategories();
        _categoryBox.DataSource = categories;
        _categoryBox.DisplayMember = "Name";
        _categoryBox.ValueMember = "Id";
        _categoryBox.SelectedIndex = -1;
        if (existing != null)
        {
            var idx = categories.FindIndex(c => c.Id == existing.CategoryId);
            if (idx < 0) idx = categories.FindIndex(c => c.Name == existing.CategoryName);
            _categoryBox.SelectedIndex = idx;
        }
        _categoryError = MakeError(y + 48);
        Controls.Add(_categoryBox); Controls.Add(_categoryError);
        y += 66;

        // Description
        Controls.Add(MakeLabel("Description", y));
        _descBox = new TextBox
        {
            Location = new Point(x, y + 20), Size = new Size(w, 60), Multiline = true,
            ScrollBars = ScrollBars.Vertical, Text = Result.Description
        };
        ThemeManager.StyleTextBox(_descBox);
        Controls.Add(_descBox);
        y += 90;

        // Selling price
        Controls.Add(MakeLabel("Selling Price *", y));
        _priceBox = new TextBox
        {
            Location = new Point(x, y + 20), Width = w,
            Text = existing != null ? existing.SellingPrice.ToString("0.00", CultureInfo.CurrentCulture) : ""
        };
        ThemeManager.StyleTextBox(_priceBox);
        _priceError = MakeError(y + 48);
        Controls.Add(_priceBox); Controls.Add(_priceError);
        y += 66;

        // Cost price
        Controls.Add(MakeLabel("Cost Price *", y));
        _costBox = new TextBox
        {
            Location = new Point(x, y + 20), Width = w,
            Text = existing != null ? existing.CostPrice.ToString("0.00", CultureInfo.CurrentCulture) : ""
        };
        ThemeManager.StyleTextBox(_costBox);
        _costError = MakeError(y + 48);
        Controls.Add(_costBox); Controls.Add(_costError);
        y += 66;

        // Stock
        Controls.Add(MakeLabel("Stock Qty *", y));
        _stockBox = new TextBox
        {
            Location = new Point(x, y + 20), Width = w,
            Text = existing != null ? existing.StockQuantity.ToString() : "0"
        };
        ThemeManager.StyleTextBox(_stockBox);
        _stockError = MakeError(y + 48);
        Controls.Add(_stockBox); Controls.Add(_stockError);
        y += 66;

        // Image placeholder
        Controls.Add(MakeLabel("Image", y));
        _imagePanel = new Panel
        {
            Location = new Point(x, y + 20), Size = new Size(w, 44),
            BackColor = Color.White, Cursor = Cursors.Hand
        };
        var imgLabel = new Label
        {
            Text = string.IsNullOrEmpty(Result.ImagePath) ? "Click to select image" : Result.ImagePath,
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = ThemeManager.SecondaryText, Font = ThemeManager.SmallFont, Cursor = Cursors.Hand
        };
        _imagePanel.Controls.Add(imgLabel);
        _imagePanel.Paint += (s, e) =>
        {
            using var pen = new Pen(ThemeManager.Secondary) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            e.Graphics.DrawRectangle(pen, 0, 0, _imagePanel.Width - 1, _imagePanel.Height - 1);
        };
        EventHandler pick = (s, e) => MessageBox.Show(this, "Image selection is a visual placeholder in this preview.", "Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
        _imagePanel.Click += pick;
        imgLabel.Click += pick;
        Controls.Add(_imagePanel);
        y += 74;

        // Available
        _availableCheck = new CheckBox
        {
            Text = "Available", Checked = existing?.IsAvailable ?? true, AutoSize = true,
            Location = new Point(x, y), Font = ThemeManager.BodyFont, ForeColor = ThemeManager.MainText
        };
        Controls.Add(_availableCheck);
        y += 38;

        // Buttons
        var btnSave = new Button { Text = "Save", Size = new Size(100, 36), Location = new Point(x + w - 210, y) };
        ThemeManager.StyleButton(btnSave, true);
        btnSave.Click += OnSave;
        var btnCancel = new Button { Text = "Cancel", Size = new Size(100, 36), Location = new Point(x + w - 100, y), DialogResult = DialogResult.Cancel };
        ThemeManager.StyleButton(btnCancel);
        Controls.Add(btnSave); Controls.Add(btnCancel);
        AcceptButton = btnSave;
        CancelButton = btnCancel;

        ClientSize = new Size(450, y + 36 + 20);
    }

    private void OnSave(object? sender, EventArgs e)
    {
        foreach (var l in new[] { _nameError, _categoryError, _priceError, _costError, _stockError })
            ValidationHelper.ClearValidationError(l);

        bool ok = true;

        if (!ValidationHelper.IsRequired(_nameBox.Text))
        {
            ValidationHelper.ShowValidationError(_nameError, "Product name is required.");
            ok = false;
        }
        if (_categoryBox.SelectedItem is not Category cat)
        {
            ValidationHelper.ShowValidationError(_categoryError, "Please select a category.");
            ok = false;
            cat = null!;
        }
        if (!ValidationHelper.IsValidPrice(_priceBox.Text))
        {
            ValidationHelper.ShowValidationError(_priceError, "Enter a valid price greater than 0.");
            ok = false;
        }
        if (!ValidationHelper.IsValidPrice(_costBox.Text))
        {
            ValidationHelper.ShowValidationError(_costError, "Enter a valid cost greater than 0.");
            ok = false;
        }
        if (!ValidationHelper.IsValidQuantity(_stockBox.Text))
        {
            ValidationHelper.ShowValidationError(_stockError, "Enter a valid whole number (0 or more).");
            ok = false;
        }

        if (!ok) return;

        Result.Name = _nameBox.Text.Trim();
        Result.CategoryId = cat.Id;
        Result.CategoryName = cat.Name;
        Result.Description = _descBox.Text.Trim();
        Result.SellingPrice = decimal.Parse(_priceBox.Text);
        Result.CostPrice = decimal.Parse(_costBox.Text);
        Result.StockQuantity = int.Parse(_stockBox.Text);
        Result.IsAvailable = _availableCheck.Checked;

        DialogResult = DialogResult.OK;
        Close();
    }
}

