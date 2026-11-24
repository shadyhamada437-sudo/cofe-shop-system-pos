using CoffeeShopPOS.Dialogs;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.UserControls;

/// <summary>
/// Categories management screen with search and CRUD. Deleting a category that has products is blocked.
/// </summary>
public partial class CategoriesControl : UserControl
{
    private readonly List<Category> _categories = MockData.GetCategories();
    private readonly List<Product> _products = MockData.GetProducts();

    private readonly TextBox _searchBox;
    private readonly DataGridView _grid;

    public CategoriesControl()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;
        Padding = new Padding(20);

        // ── Top bar ──
        var topBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = ThemeManager.MainBackground };
        var title = new Label
        {
            Text = "📁 Categories", Font = ThemeManager.SubHeaderFont, ForeColor = ThemeManager.MainText,
            AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft
        };
        var right = new FlowLayoutPanel
        {
            Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent,
            Padding = new Padding(0, 8, 0, 0)
        };
        _searchBox = UIHelper.CreateSearchBox("🔍 Search categories...");
        _searchBox.Width = 220;
        _searchBox.Margin = new Padding(0, 4, 10, 0);
        var btnAdd = UIHelper.CreateIconButton("+ Add Category", ThemeManager.Success);
        btnAdd.Margin = new Padding(0);
        btnAdd.Click += (s, e) => AddCategory();
        right.Controls.AddRange(new Control[] { _searchBox, btnAdd });
        topBar.Controls.Add(right);
        topBar.Controls.Add(title);

        // ── Action bar ──
        var actionBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 48, BackColor = ThemeManager.MainBackground,
            FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 4, 0, 0)
        };
        var btnEdit = UIHelper.CreateIconButton("✏ Edit", ThemeManager.Primary);
        btnEdit.Click += (s, e) => EditSelected();
        var btnDelete = UIHelper.CreateIconButton("🗑 Delete", ThemeManager.Danger);
        btnDelete.Click += (s, e) => DeleteSelected();
        actionBar.Controls.AddRange(new Control[] { btnEdit, btnDelete });

        // ── Grid ──
        _grid = new DataGridView { Dock = DockStyle.Fill };
        ThemeManager.StyleDataGridView(_grid);
        UIHelper.SetDoubleBuffered(_grid);
        _grid.MultiSelect = false;
        _grid.Columns.Add("Id", "ID");
        _grid.Columns.Add("Name", "Name");
        _grid.Columns.Add("Description", "Description");
        _grid.Columns.Add("Count", "Product Count");
        _grid.Columns["Id"]!.FillWeight = 30;
        _grid.Columns["Name"]!.FillWeight = 80;
        _grid.Columns["Description"]!.FillWeight = 200;
        _grid.Columns["Count"]!.FillWeight = 60;
        _grid.Columns["Count"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditSelected(); };

        Controls.Add(_grid);
        Controls.Add(actionBar);
        Controls.Add(topBar);

        _searchBox.TextChanged += (s, e) => RefreshGrid();
        RefreshGrid();
    }

    private string SearchText => _searchBox.Text == (string?)_searchBox.Tag ? "" : _searchBox.Text.Trim();

    private int ProductCount(Category c) => _products.Count(p => p.CategoryId == c.Id || (p.CategoryId == 0 && p.CategoryName == c.Name));

    private void RefreshGrid()
    {
        int? selectedId = (_grid.CurrentRow?.Tag as Category)?.Id;
        string search = SearchText;

        _grid.Rows.Clear();
        foreach (var c in _categories.Where(c => search.Length == 0 || c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
        {
            int idx = _grid.Rows.Add(c.Id, c.Name, c.Description, ProductCount(c));
            _grid.Rows[idx].Tag = c;
            if (selectedId == c.Id) _grid.Rows[idx].Selected = true;
        }
    }

    private Category? SelectedCategory =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].Tag as Category : _grid.CurrentRow?.Tag as Category;

    private void AddCategory()
    {
        using var dlg = new CategoryDialog(null, _categories);
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
        dlg.Result.Id = _categories.Count == 0 ? 1 : _categories.Max(c => c.Id) + 1;
        _categories.Add(dlg.Result);
        RefreshGrid();
    }

    private void EditSelected()
    {
        var cat = SelectedCategory;
        if (cat == null) { MessageBox.Show("Please select a category first.", "Edit Category", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        string oldName = cat.Name;
        using var dlg = new CategoryDialog(cat, _categories);
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

        cat.Name = dlg.Result.Name;
        cat.Description = dlg.Result.Description;
        if (oldName != cat.Name)
            foreach (var p in _products.Where(p => p.CategoryId == cat.Id)) p.CategoryName = cat.Name;
        RefreshGrid();
    }

    private void DeleteSelected()
    {
        var cat = SelectedCategory;
        if (cat == null) { MessageBox.Show("Please select a category first.", "Delete Category", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        int count = ProductCount(cat);
        if (count > 0)
        {
            MessageBox.Show($"Cannot delete \"{cat.Name}\" because it contains {count} product(s).\nReassign or remove those products first.",
                "Cannot Delete Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (ConfirmationDialog.Show($"Delete category \"{cat.Name}\"?", "Delete Category") != DialogResult.OK)
            return;
        _categories.Remove(cat);
        RefreshGrid();
    }
}

