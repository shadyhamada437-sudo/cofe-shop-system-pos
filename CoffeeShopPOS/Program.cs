using CoffeeShopPOS.Dialogs;
using CoffeeShopPOS.Forms;
using CoffeeShopPOS.Helpers;
using CoffeeShopPOS.Models;
using CoffeeShopPOS.UserControls;

namespace CoffeeShopPOS;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length > 0)
        {
            MockData.CurrentCashier ??= MockData.GetCashiers()[0];
            string target = Path.GetFileNameWithoutExtension(args[0].Trim()).ToLowerInvariant();
            switch (target)
            {
                case "login":
                case "loginform":
                    Application.Run(new LoginForm());
                    return;
                case "main":
                case "mainform":
                    Application.Run(new MainForm(MockData.GetCashiers()[0]));
                    return;
                case "dashboard":
                case "dashboardcontrol":
                    RunPage(new DashboardControl(), "Dashboard");
                    return;
                case "pos":
                case "poscontrol":
                    RunPage(new POSControl(), "POS / New Sale");
                    return;
                case "products":
                case "productscontrol":
                    RunPage(new ProductsControl(), "Products / Menu");
                    return;
                case "categories":
                case "categoriescontrol":
                    RunPage(new CategoriesControl(), "Categories");
                    return;
                case "sales":
                case "salescontrol":
                    RunPage(new SalesControl(), "Sales / Transactions");
                    return;
                case "receipt":
                case "receiptcontrol":
                    RunPage(new ReceiptControl(), "Billing / Receipt Preview");
                    return;
                case "analytics":
                case "analyticscontrol":
                    RunPage(new AnalyticsControl(), "Sales Analytics & Reports");
                    return;
                case "settings":
                case "settingscontrol":
                    RunPage(new SettingsControl(), "Settings");
                    return;
                case "productdialog":
                    Application.Run(new ProductDialog());
                    return;
                case "categorydialog":
                    Application.Run(new CategoryDialog(null, MockData.GetCategories()));
                    return;
                case "saledetailsdialog":
                    Application.Run(new SaleDetailsDialog(MockData.GetSales()[0]));
                    return;
            }
        }

        Application.Run(new LoginForm());
    }

    private static void RunPage(UserControl page, string title)
    {
        var host = new Form
        {
            Text = $"Brew Haven POS — {title}",
            Size = new Size(1280, 800),
            MinimumSize = new Size(1024, 680),
            StartPosition = FormStartPosition.CenterScreen,
            WindowState = FormWindowState.Maximized,
            BackColor = ThemeManager.MainBackground
        };
        page.Dock = DockStyle.Fill;
        host.Controls.Add(page);
        Application.Run(host);
    }
}
