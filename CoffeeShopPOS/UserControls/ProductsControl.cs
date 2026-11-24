using CoffeeShopPOS.Dialogs;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.UserControls;

/// <summary>
/// Products / Menu management screen with search, category filter and CRUD.
/// </summary>
public partial class ProductsControl : UserControl
{
    private readonly List<Product> _products = MockData.GetProducts();
    private readonly List<Category> _categories = MockData.GetCategories();

    private readonly TextBox _searchBox;
    private readonly ComboBox _categoryFilter;
    private readonly DataGridView _grid;

    public ProductsControl()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemeManager.MainBackground;
        Font = ThemeManager.BodyFont;
        Padding = new Padding(20);

        // ── Top bar ──
        var topBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = ThemeManager.MainBackground };

        var title = new Label
        {
            Text = "☕ Products / Menu", Font = ThemeManager.SubHeaderFont, ForeColor = ThemeManager.MainText,
            AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft
        };

        var right = new FlowLayoutPanel
        {
            Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent,
            Padding = new Padding(0, 8, 0, 0)
        };

        _searchBox = UIHelper.CreateSearchBox("🔍 Search products...");
        _searchBox.Width = 220;
        _searchBox.Margin = new Padding(0, 4, 10, 0);

        _categoryFilter = new ComboBox { Width = 160, Margin = new Padding(0, 4, 10, 0) };
        ThemeManager.StyleComboBox(_categoryFilter);

        var btnAdd = UIHelper.CreateIconButton("+ Add Product", ThemeManager.Success);
        btnAdd.Margin = new Padding(0);
        btnAdd.Click += (s, e) => AddProduct();

        right.Controls.AddRange(new Control[] { _searchBox, _categoryFilter, btnAdd });
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
        _grid.Columns.Add("Category", "Category");
        _grid.Columns.Add("Price", "Selling Price");
        _grid.Columns.Add("Cost", "Cost Price");
        _grid.Columns.Add("Stock", "Stock");
        _grid.Columns.Add("Available", "Available");
        _grid.Columns["Id"]!.FillWeight = 30;
        _grid.Columns["Name"]!.FillWeight = 150;
        _grid.Columns["Stock"]!.FillWeight = 50;
        _grid.Columns["Available"]!.FillWeight = 60;
        _grid.Columns["Price"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _grid.Columns["Cost"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _grid.Columns["Stock"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditSelected(); };
        _grid.CellMouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                _grid.ClearSelection();
                _grid.Rows[e.RowIndex].Selected = true;
                _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
            }
        };

        // Context menu
        var menu = new ContextMenuStrip { Font = ThemeManager.BodyFont };
        menu.Items.Add("Edit", null, (s, e) => EditSelected());
        menu.Items.Add("Delete", null, (s, e) => DeleteSelected());
        menu.Items.Add("Toggle Availability", null, (s, e) => ToggleAvailability());
        _grid.ContextMenuStrip = menu;

        // Dock order: Fill first, then Top controls (last added is outermost)
        Controls.Add(_grid);
        Controls.Add(actionBar);
        Controls.Add(topBar);

        // Filters
        _categoryFilter.Items.Add("All Categories");
        foreach (var c in _categories) _categoryFilter.Items.Add(c.Name);
        _categoryFilter.SelectedIndex = 0;
        _categoryFilter.SelectedIndexChanged += (s, e) => RefreshGrid();
        _searchBox.TextChanged += (s, e) => RefreshGrid();

        RefreshGrid();
    }

    private string SearchText => _searchBox.Text == (string?)_searchBox.Tag ? "" : _searchBox.Text.Trim();

    private void RefreshGrid()
    {
        int? selectedId = (_grid.CurrentRow?.Tag as Product)?.Id;
        string search = SearchText;
        string? category = _categoryFilter.SelectedIndex > 0 ? _categoryFilter.SelectedItem?.ToString() : null;

        var filtered = _products.Where(p =>
            (search.Length == 0 || p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
            (category == null || p.CategoryName == category));

        _grid.Rows.Clear();
        foreach (var p in filtered)
        {
            int idx = _grid.Rows.Add(p.Id, p.Name, p.CategoryName, p.SellingPrice.ToString("C2"),
                p.CostPrice.ToString("C2"), p.StockQuantity, p.IsAvailable ? "Yes" : "No");
            var row = _grid.Rows[idx];
            row.Tag = p;
            var cell = row.Cells["Available"];
            cell.Style.ForeColor = p.IsAvailable ? ThemeManager.Success : ThemeManager.Danger;
            cell.Style.Font = ThemeManager.BodyBoldFont;
            if (selectedId == p.Id) row.Selected = true;
        }
    }

    private Product? SelectedProduct =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].Tag as Product : _grid.CurrentRow?.Tag as Product;

    private void AddProduct()
    {
        using var dlg = new ProductDialog();
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
        dlg.Result.Id = _products.Count == 0 ? 1 : _products.Max(p => p.Id) + 1;
        _products.Add(dlg.Result);
        RefreshGrid();
    }

    private void EditSelected()
    {
        var product = SelectedProduct;
        if (product == null) { MessageBox.Show("Please select a product first.", "Edit Product", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        using var dlg = new ProductDialog(product);
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

        var r = dlg.Result;
        product.Name = r.Name;
        product.CategoryId = r.CategoryId;
        product.CategoryName = r.CategoryName;
        product.Description = r.Description;
        product.SellingPrice = r.SellingPrice;
        product.CostPrice = r.CostPrice;
        product.StockQuantity = r.StockQuantity;
        product.ImagePath = r.ImagePath;
        product.IsAvailable = r.IsAvailable;
        RefreshGrid();
    }

    private void DeleteSelected()
    {
        var product = SelectedProduct;
        if (product == null) { MessageBox.Show("Please select a product first.", "Delete Product", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

        if (ConfirmationDialog.Show($"Delete product \"{product.Name}\"?\nThis cannot be undone.", "Delete Product") != DialogResult.OK)
            return;
        _products.Remove(product);
        RefreshGrid();
    }

    private void ToggleAvailability()
    {
        var product = SelectedProduct;
        if (product == null) return;
        product.IsAvailable = !product.IsAvailable;
        RefreshGrid();
    }
}

