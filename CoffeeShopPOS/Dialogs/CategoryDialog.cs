using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.Dialogs;

/// <summary>
/// Modal dialog to add or edit a category. Name must be required and unique.
/// </summary>
public partial class CategoryDialog : Form
{
    private readonly Category? _existing;
    private readonly IEnumerable<Category> _allCategories;

    private readonly TextBox _nameBox;
    private readonly TextBox _descBox;
    private readonly Label _nameError;

    /// <summary>The resulting category (valid after DialogResult.OK).</summary>
    public Category Result { get; private set; }

    public CategoryDialog(Category? existing, IEnumerable<Category> allCategories)
    {
        _existing = existing;
        _allCategories = allCategories;
        Result = existing != null
            ? new Category { Id = existing.Id, Name = existing.Name, Description = existing.Description }
            : new Category();

        Text = existing == null ? "Add Category" : "Edit Category";
        Size = new Size(400, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;

        int x = 25, w = 335;

        var lblName = new Label { Text = "Category Name *", AutoSize = true, Font = ThemeManager.SmallBoldFont, ForeColor = ThemeManager.MainText, Location = new Point(x, 18) };
        _nameBox = new TextBox { Location = new Point(x, 40), Width = w, Text = Result.Name };
        ThemeManager.StyleTextBox(_nameBox);
        _nameError = new Label { AutoSize = false, Size = new Size(w, 18), Location = new Point(x, 70), Visible = false };

        var lblDesc = new Label { Text = "Description", AutoSize = true, Font = ThemeManager.SmallBoldFont, ForeColor = ThemeManager.MainText, Location = new Point(x, 94) };
        _descBox = new TextBox { Location = new Point(x, 116), Size = new Size(w, 60), Multiline = true, Text = Result.Description, ScrollBars = ScrollBars.Vertical };
        ThemeManager.StyleTextBox(_descBox);

        var btnSave = new Button { Text = "Save", Size = new Size(100, 36), Location = new Point(x + w - 210, 196) };
        ThemeManager.StyleButton(btnSave, true);
        btnSave.Click += OnSave;

        var btnCancel = new Button { Text = "Cancel", Size = new Size(100, 36), Location = new Point(x + w - 100, 196), DialogResult = DialogResult.Cancel };
        ThemeManager.StyleButton(btnCancel);

        AcceptButton = btnSave;
        CancelButton = btnCancel;

        Controls.AddRange(new Control[] { lblName, _nameBox, _nameError, lblDesc, _descBox, btnSave, btnCancel });
    }

    private void OnSave(object? sender, EventArgs e)
    {
        ValidationHelper.ClearValidationError(_nameError);
        string name = _nameBox.Text.Trim();

        if (!ValidationHelper.IsRequired(name))
        {
            ValidationHelper.ShowValidationError(_nameError, "Category name is required.");
            return;
        }

        bool duplicate = _allCategories.Any(c =>
            c.Id != (_existing?.Id ?? -1) &&
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        if (duplicate)
        {
            ValidationHelper.ShowValidationError(_nameError, "A category with this name already exists.");
            return;
        }

        Result.Name = name;
        Result.Description = _descBox.Text.Trim();
        DialogResult = DialogResult.OK;
        Close();
    }
}

