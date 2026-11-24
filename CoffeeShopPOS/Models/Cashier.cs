namespace CoffeeShopPOS.Models;

public enum CashierRole
{
    Admin,
    Cashier
}

public class Cashier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public CashierRole Role { get; set; }

    public override string ToString() => Name;
}
