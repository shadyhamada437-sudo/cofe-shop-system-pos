namespace CoffeeShopPOS.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;

    public override string ToString() => Name;
}
