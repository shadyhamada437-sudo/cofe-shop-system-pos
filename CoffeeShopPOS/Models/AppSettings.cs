namespace CoffeeShopPOS.Models;

public class AppSettings
{
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Currency { get; set; } = "$";
    public decimal TaxRate { get; set; }
    public decimal DefaultDiscount { get; set; }
    public string ReceiptFooter { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
}
