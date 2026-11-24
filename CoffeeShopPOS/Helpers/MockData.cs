using CoffeeShopPOS.Models;

namespace CoffeeShopPOS.Helpers;

/// <summary>
/// Provides all mock/seed data for the Coffee Shop POS application.
/// </summary>
public static class MockData
{
    // ───────────── Shared in-memory stores (lazily seeded once, mutable) ─────────────

    private static List<Category>? _categories;
    private static List<Product>? _products;
    private static List<Cashier>? _cashiers;
    private static AppSettings? _settings;
    private static List<Sale>? _sales;

    /// <summary>Shared mutable category list.</summary>
    public static List<Category> Categories => _categories ??= BuildCategories();
    /// <summary>Shared mutable product list.</summary>
    public static List<Product> Products => _products ??= BuildProducts();
    /// <summary>Shared mutable cashier list.</summary>
    public static List<Cashier> Cashiers => _cashiers ??= BuildCashiers();
    /// <summary>Shared mutable application settings.</summary>
    public static AppSettings Settings { get => _settings ??= BuildSettings(); set => _settings = value; }
    /// <summary>Shared mutable sales list. Add new sales here (e.g. MockData.Sales.Add(sale)).</summary>
    public static List<Sale> Sales => _sales ??= BuildSales();

    /// <summary>The currently logged-in cashier (set by LoginForm).</summary>
    public static Cashier? CurrentCashier { get; set; }

    public static List<Category> GetCategories() => Categories;
    public static List<Product> GetProducts() => Products;
    public static List<Cashier> GetCashiers() => Cashiers;
    public static AppSettings GetDefaultSettings() => Settings;
    public static List<Sale> GetSales() => Sales;

    /// <summary>Next unused sale Id.</summary>
    public static int NextSaleId() => Sales.Count == 0 ? 1 : Sales.Max(s => s.Id) + 1;
    /// <summary>Next transaction id string, e.g. TXN-019.</summary>
    public static string NextTransactionId() => $"TXN-{NextSaleId():000}";
    /// <summary>Next unused product Id.</summary>
    public static int NextProductId() => Products.Count == 0 ? 1 : Products.Max(p => p.Id) + 1;
    /// <summary>Next unused category Id.</summary>
    public static int NextCategoryId() => Categories.Count == 0 ? 1 : Categories.Max(c => c.Id) + 1;
    // ───────────── Categories ─────────────

    private static List<Category> BuildCategories() => new()
    {
        new Category { Id = 1, Name = "Espresso",    Description = "Concentrated coffee shots and espresso-based drinks" },
        new Category { Id = 2, Name = "Hot Coffee",   Description = "Classic brewed hot coffee beverages" },
        new Category { Id = 3, Name = "Iced Coffee",  Description = "Chilled and iced coffee drinks" },
        new Category { Id = 4, Name = "Tea",          Description = "Hot and iced tea selections" },
        new Category { Id = 5, Name = "Frappes",      Description = "Blended frozen coffee and cream beverages" },
        new Category { Id = 6, Name = "Desserts",     Description = "Sweet treats and pastry desserts" },
        new Category { Id = 7, Name = "Bakery",       Description = "Freshly baked bread, muffins, and pastries" }
    };

    // ───────────── Products ─────────────

    private static List<Product> BuildProducts() => new()
    {
        // Espresso (1)
        new Product { Id = 1,  Name = "Classic Espresso",       CategoryId = 1, CategoryName = "Espresso",    Description = "Rich, full-bodied single shot of espresso",       SellingPrice = 2.50m, CostPrice = 0.80m, StockQuantity = 200, IsAvailable = true },
        new Product { Id = 2,  Name = "Double Shot Espresso",   CategoryId = 1, CategoryName = "Espresso",    Description = "Intense double shot for the bold coffee lover",   SellingPrice = 3.50m, CostPrice = 1.20m, StockQuantity = 180, IsAvailable = true },

        // Hot Coffee (2)
        new Product { Id = 3,  Name = "Caramel Latte",          CategoryId = 2, CategoryName = "Hot Coffee",   Description = "Smooth latte with rich caramel drizzle",          SellingPrice = 4.75m, CostPrice = 1.50m, StockQuantity = 150, IsAvailable = true },
        new Product { Id = 4,  Name = "Cappuccino",             CategoryId = 2, CategoryName = "Hot Coffee",   Description = "Equal parts espresso, steamed milk, and foam",    SellingPrice = 4.25m, CostPrice = 1.30m, StockQuantity = 160, IsAvailable = true },

        // Iced Coffee (3)
        new Product { Id = 5,  Name = "Iced Americano",         CategoryId = 3, CategoryName = "Iced Coffee",  Description = "Chilled espresso over ice with cold water",       SellingPrice = 3.75m, CostPrice = 1.00m, StockQuantity = 170, IsAvailable = true },
        new Product { Id = 6,  Name = "Iced Vanilla Latte",     CategoryId = 3, CategoryName = "Iced Coffee",  Description = "Creamy iced latte infused with vanilla syrup",    SellingPrice = 5.00m, CostPrice = 1.60m, StockQuantity = 140, IsAvailable = true },

        // Tea (4)
        new Product { Id = 7,  Name = "English Breakfast Tea",  CategoryId = 4, CategoryName = "Tea",          Description = "Full-bodied black tea blend",                     SellingPrice = 3.00m, CostPrice = 0.60m, StockQuantity = 200, IsAvailable = true },
        new Product { Id = 8,  Name = "Matcha Green Tea Latte", CategoryId = 4, CategoryName = "Tea",          Description = "Premium matcha whisked with steamed milk",        SellingPrice = 4.50m, CostPrice = 1.80m, StockQuantity = 120, IsAvailable = true },

        // Frappes (5)
        new Product { Id = 9,  Name = "Mocha Frappe",           CategoryId = 5, CategoryName = "Frappes",      Description = "Blended chocolate and coffee with whipped cream", SellingPrice = 5.50m, CostPrice = 1.90m, StockQuantity = 130, IsAvailable = true },
        new Product { Id = 10, Name = "Caramel Frappe",         CategoryId = 5, CategoryName = "Frappes",      Description = "Frozen caramel coffee blend topped with cream",   SellingPrice = 5.50m, CostPrice = 1.85m, StockQuantity = 130, IsAvailable = true },

        // Desserts (6)
        new Product { Id = 11, Name = "Tiramisu Slice",         CategoryId = 6, CategoryName = "Desserts",     Description = "Classic Italian coffee-flavored layered dessert", SellingPrice = 6.00m, CostPrice = 2.50m, StockQuantity = 40,  IsAvailable = true },
        new Product { Id = 12, Name = "Chocolate Brownie",      CategoryId = 6, CategoryName = "Desserts",     Description = "Warm fudgy brownie with cocoa chunks",            SellingPrice = 4.00m, CostPrice = 1.40m, StockQuantity = 60,  IsAvailable = true },

        // Bakery (7)
        new Product { Id = 13, Name = "Butter Croissant",       CategoryId = 7, CategoryName = "Bakery",       Description = "Flaky, golden French-style butter croissant",     SellingPrice = 3.25m, CostPrice = 1.00m, StockQuantity = 80,  IsAvailable = true },
        new Product { Id = 14, Name = "Blueberry Muffin",       CategoryId = 7, CategoryName = "Bakery",       Description = "Soft muffin loaded with fresh blueberries",       SellingPrice = 3.50m, CostPrice = 1.10m, StockQuantity = 70,  IsAvailable = true },
        new Product { Id = 15, Name = "Cinnamon Roll",          CategoryId = 7, CategoryName = "Bakery",       Description = "Warm cinnamon roll with cream cheese icing",      SellingPrice = 3.75m, CostPrice = 1.20m, StockQuantity = 65,  IsAvailable = true }
    };

    // ───────────── Cashiers ─────────────

    private static List<Cashier> BuildCashiers() => new()
    {
        new Cashier { Id = 1, Name = "Admin User",   Username = "admin",   Password = "admin123", Role = CashierRole.Admin },
        new Cashier { Id = 2, Name = "Jane Cashier",  Username = "cashier", Password = "pass123",  Role = CashierRole.Cashier }
    };

    // ───────────── App Settings ─────────────

    private static AppSettings BuildSettings() => new()
    {
        ShopName       = "Brew Haven",
        Address        = "123 Coffee Street, Cityville",
        Phone          = "(555) 123-4567",
        Currency       = "$",
        TaxRate        = 14m,
        DefaultDiscount = 0m,
        ReceiptFooter  = "Thank you for visiting Brew Haven!",
        LogoPath       = ""
    };

    // ───────────── Sales ─────────────

    private static List<Sale> BuildSales()
    {
        var now = DateTime.Now;
        var products = Products;

        Product P(int id) => products.First(p => p.Id == id);

        SaleItem Item(int productId, int qty)
        {
            var p = P(productId);
            return new SaleItem
            {
                ProductId   = p.Id,
                ProductName = p.Name,
                UnitPrice   = p.SellingPrice,
                Quantity    = qty
            };
        }

        decimal taxRate = 14m;

        Sale MakeSale(int id, string txn, DateTime dt, int cashierId, string cashierName,
                      OrderType orderType, List<SaleItem> items, decimal discount,
                      PaymentMethod payment, decimal amountReceived,
                      SaleStatus status = SaleStatus.Completed)
        {
            decimal subtotal = items.Sum(i => i.LineTotal);
            decimal discountAmt = discount;
            decimal taxable = subtotal - discountAmt;
            decimal tax = Math.Round(taxable * taxRate / 100m, 2);
            decimal total = taxable + tax;
            decimal change = amountReceived - total;
            if (change < 0) change = 0;

            return new Sale
            {
                Id             = id,
                TransactionId  = txn,
                DateTime       = dt,
                CashierId      = cashierId,
                CashierName    = cashierName,
                OrderType      = orderType,
                Items          = items,
                Subtotal       = subtotal,
                Discount       = discountAmt,
                TaxRate        = taxRate,
                Tax            = tax,
                Total          = total,
                PaymentMethod  = payment,
                AmountReceived = amountReceived,
                ChangeDue      = change,
                Status         = status
            };
        }

        return new List<Sale>
        {
            MakeSale(1,  "TXN-001", now.AddDays(-28).Date.AddHours(8).AddMinutes(15),
                1, "Admin User", OrderType.DineIn,
                new List<SaleItem> { Item(3, 2), Item(13, 1) },
                0m, PaymentMethod.Cash, 20.00m),

            MakeSale(2,  "TXN-002", now.AddDays(-27).Date.AddHours(9).AddMinutes(30),
                2, "Jane Cashier", OrderType.Takeaway,
                new List<SaleItem> { Item(1, 1), Item(14, 2) },
                0m, PaymentMethod.Card, 0m),

            MakeSale(3,  "TXN-003", now.AddDays(-25).Date.AddHours(10).AddMinutes(0),
                1, "Admin User", OrderType.DineIn,
                new List<SaleItem> { Item(4, 1), Item(11, 1), Item(7, 1) },
                1.00m, PaymentMethod.Cash, 20.00m),

            MakeSale(4,  "TXN-004", now.AddDays(-24).Date.AddHours(11).AddMinutes(45),
                2, "Jane Cashier", OrderType.Takeaway,
                new List<SaleItem> { Item(5, 2), Item(12, 1) },
                0m, PaymentMethod.Card, 0m),

            MakeSale(5,  "TXN-005", now.AddDays(-22).Date.AddHours(14).AddMinutes(20),
                1, "Admin User", OrderType.DineIn,
                new List<SaleItem> { Item(9, 1), Item(10, 1), Item(15, 2) },
                0m, PaymentMethod.Cash, 25.00m),

            MakeSale(6,  "TXN-006", now.AddDays(-20).Date.AddHours(8).AddMinutes(50),
                2, "Jane Cashier", OrderType.Takeaway,
                new List<SaleItem> { Item(6, 1) },
                0m, PaymentMethod.Cash, 10.00m),

            MakeSale(7,  "TXN-007", now.AddDays(-18).Date.AddHours(13).AddMinutes(10),
                1, "Admin User", OrderType.DineIn,
                new List<SaleItem> { Item(8, 2), Item(3, 1) },
                0.50m, PaymentMethod.Card, 0m),

            MakeSale(8,  "TXN-008", now.AddDays(-16).Date.AddHours(15).AddMinutes(30),
                2, "Jane Cashier", OrderType.Takeaway,
                new List<SaleItem> { Item(2, 1), Item(13, 2), Item(7, 1) },
                0m, PaymentMethod.Cash, 20.00m),

            MakeSale(9,  "TXN-009", now.AddDays(-14).Date.AddHours(9).AddMinutes(5),
                1, "Admin User", OrderType.DineIn,
                new List<SaleItem> { Item(1, 3), Item(14, 1) },
                0m, PaymentMethod.Other, 15.00m),

            MakeSale(10, "TXN-010", now.AddDays(-12).Date.AddHours(16).AddMinutes(0),
                2, "Jane Cashier", OrderType.DineIn,
                new List<SaleItem> { Item(4, 2), Item(11, 1), Item(12, 1) },
                2.00m, PaymentMethod.Card, 0m),

            MakeSale(11, "TXN-011", now.AddDays(-10).Date.AddHours(10).AddMinutes(40),
                1, "Admin User", OrderType.Takeaway,
                new List<SaleItem> { Item(5, 1), Item(9, 1) },
                0m, PaymentMethod.Cash, 15.00m),

            MakeSale(12, "TXN-012", now.AddDays(-8).Date.AddHours(12).AddMinutes(15),
                2, "Jane Cashier", OrderType.DineIn,
                new List<SaleItem> { Item(6, 2), Item(15, 1) },
                0m, PaymentMethod.Card, 0m),

            MakeSale(13, "TXN-013", now.AddDays(-6).Date.AddHours(14).AddMinutes(55),
                1, "Admin User", OrderType.Takeaway,
                new List<SaleItem> { Item(3, 1), Item(8, 1), Item(13, 2) },
                1.50m, PaymentMethod.Cash, 25.00m),

            MakeSale(14, "TXN-014", now.AddDays(-4).Date.AddHours(11).AddMinutes(20),
                2, "Jane Cashier", OrderType.DineIn,
                new List<SaleItem> { Item(10, 2), Item(14, 1) },
                0m, PaymentMethod.Other, 20.00m),

            MakeSale(15, "TXN-015", now.AddDays(-3).Date.AddHours(9).AddMinutes(0),
                1, "Admin User", OrderType.Takeaway,
                new List<SaleItem> { Item(2, 2), Item(4, 1) },
                0m, PaymentMethod.Cash, 15.00m),

            MakeSale(16, "TXN-016", now.AddDays(-2).Date.AddHours(17).AddMinutes(30),
                2, "Jane Cashier", OrderType.DineIn,
                new List<SaleItem> { Item(1, 1), Item(11, 1), Item(15, 1) },
                0m, PaymentMethod.Card, 0m,
                SaleStatus.Cancelled),

            MakeSale(17, "TXN-017", now.AddDays(-1).Date.AddHours(8).AddMinutes(45),
                1, "Admin User", OrderType.Takeaway,
                new List<SaleItem> { Item(6, 1), Item(12, 2), Item(7, 1) },
                0.75m, PaymentMethod.Cash, 25.00m),

            MakeSale(18, "TXN-018", now.Date.AddHours(10).AddMinutes(10),
                2, "Jane Cashier", OrderType.DineIn,
                new List<SaleItem> { Item(3, 2), Item(9, 1), Item(13, 1) },
                0m, PaymentMethod.Card, 0m,
                SaleStatus.Refunded)
        };
    }
}
