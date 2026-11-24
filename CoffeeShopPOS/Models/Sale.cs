namespace CoffeeShopPOS.Models;

public enum OrderType
{
    DineIn,
    Takeaway
}

public enum SaleStatus
{
    Completed,
    Cancelled,
    Refunded
}

public class Sale
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public int CashierId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public OrderType OrderType { get; set; }
    public List<SaleItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountReceived { get; set; }
    public decimal ChangeDue { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
}
