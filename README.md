# ☕ Brew Haven — Coffee Shop Point of Sale (POS) System

**Course:** CS312 — Modern Development Tools  
**Deliverable:** Phase 1 — Graphical User Interface (GUI) Prototype  
**Platform:** C# 12 / .NET 8 Windows Forms (`net8.0-windows`)

---

## 📖 Project Overview

**Brew Haven POS** is a modern, touch-friendly desktop Point of Sale (POS) application built with **C# and .NET 8 Windows Forms** for the **CS312 (Modern Development Tools)** course project.

This **Phase 1 deliverable** provides the complete Graphical User Interface (GUI), navigation architecture, interactive workflows, custom GDI+ visualizations, and in-memory state management across all 9 required screens and modal dialogs. All controls are styled consistently around a warm espresso-and-cream coffee shop palette managed centrally by `ThemeManager`.

---

## 🚀 Setup & Run Instructions

### Prerequisites
- **Operating System:** Windows 10 / 11
- **SDK:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (8.0.100 or later)
- **IDE (Optional):** Visual Studio 2022 (v17.8+) with the **.NET Desktop Development** workload installed

### Option 1: Run from Visual Studio 2022
1. Open `CoffeeShopPOS.sln` in **Visual Studio 2022**.
2. Set `CoffeeShopPOS` as the Startup Project (configured by default).
3. Press **F5** (Start Debugging) or **Ctrl+F5** (Start Without Debugging).

### Option 2: Run from the Command Line (.NET CLI)
```powershell
cd "CoffeeShopPOS"
dotnet build ../CoffeeShopPOS.sln
dotnet run --project CoffeeShopPOS.csproj
```

---

## 🔐 Default Mock Login Credentials

The login screen validates against seeded in-memory accounts and also provides one-click **Quick Demo Login** buttons:

| Role | Username | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin` | `admin123` | Full access to POS, Sales, Products, Categories, Receipts, Analytics, Settings |
| **Cashier** | `cashier` | `pass123` | Cashier session with role badge displayed in the top navigation bar |

---

## 📂 Project Structure

```text
CoffeeShopPOS.sln
README.md
└── CoffeeShopPOS/
    ├── CoffeeShopPOS.csproj          # .NET 8 Windows Forms project file
    ├── Program.cs                    # Application entry point (starts LoginForm)
    ├── Models/                       # Domain models & enums
    │   ├── Product.cs                # Menu item (price, cost, stock, category, availability)
    │   ├── Category.cs               # Product category (Id, Name, Description)
    │   ├── Sale.cs                   # Sale transaction + OrderType & SaleStatus enums
    │   ├── SaleItem.cs               # Individual line item within a sale
    │   ├── Payment.cs                # PaymentMethod enum (Cash, Card, Other)
    │   ├── Cashier.cs                # User account + CashierRole enum (Admin, Cashier)
    │   └── AppSettings.cs            # Shop configuration (name, tax rate, currency, footer)
    ├── Helpers/                      # Shared styling, UI builders, validation, & mock data
    │   ├── ThemeManager.cs           # Palette constants, fonts, & control styling helpers
    │   ├── UIHelper.cs               # Factory methods for KPI cards, icon buttons, search boxes
    │   ├── ValidationHelper.cs       # Price, quantity, percentage, & required-field validators
    │   └── MockData.cs               # Shared in-memory store (7 categories, 15 products, 18 sales)
    ├── Forms/                        # Top-level Windows Forms
    │   ├── LoginForm.cs              # Split-screen login form with quick demo fill buttons
    │   └── MainForm.cs               # Main shell with left sidebar, top bar, live clock, & page host
    ├── UserControls/                 # Swappable screen views hosted inside MainForm
    │   ├── DashboardControl.cs       # KPI summary cards, 7-day GDI+ bar chart, recent sales
    │   ├── POSControl.cs             # Interactive point-of-sale terminal & live cart calculator
    │   ├── SalesControl.cs           # Transaction history with multi-criteria filtering & actions
    │   ├── ProductsControl.cs        # Menu/product CRUD table with category & availability filters
    │   ├── CategoriesControl.cs      # Category management grid with live product counts
    │   ├── ReceiptControl.cs         # Custom GDI+ thermal receipt renderer & PrintPreviewDialog
    │   ├── AnalyticsControl.cs       # Revenue/profit KPIs, date range filter, & 3 GDI+ charts
    │   └── SettingsControl.cs        # Shop info, tax/currency config, password form, & system info
    └── Dialogs/                      # Modal dialog forms
        ├── ProductDialog.cs          # Add / Edit product modal with inline validation
        ├── CategoryDialog.cs         # Add / Edit category modal
        ├── SaleDetailsDialog.cs      # Itemized transaction breakdown modal
        └── ConfirmationDialog.cs     # Reusable confirmation prompt
```

---

## 🖥️ Summary of Completed Screens & GUI Interactions

### 1. Login Screen (`LoginForm`)
- Split-panel layout featuring dark espresso branding on the left and a clean login card on the right.
- Show/hide password toggle, inline validation error banner, **Enter** key submission, and **Admin Demo** / **Cashier Demo** quick-fill buttons.

### 2. Main Application Shell (`MainForm`)
- Persistent dark espresso sidebar (`#28211D`) with active indicator and hover effects across 8 modules plus Logout.
- Top header bar displaying the active screen title, live ticking clock (`Timer`), logged-in user badge (`Name (Role)`), and notification button.

### 3. Dashboard (`DashboardControl`)
- **4 KPI Stat Cards:** Today's Revenue, Today's Orders, Average Order Value, and Top Selling Item.
- **Weekly Sales Overview:** Custom GDI+ bar chart plotting revenue over the last 7 days.
- **Recent Transactions & Top Selling Products:** Live-bound tables reflecting any new sales completed in the POS screen.

### 4. POS / New Sale (`POSControl`)
- **Left Menu Browser:** Search bar, horizontal category filter pills (`All` + 7 categories), and clickable product cards with category color accents and prices.
- **Right Order Cart:** Auto-generated transaction ID, Dine-In / Takeaway toggle (with conditional Table # input), per-line `+`/`-`/`×` quantity controls, discount validation, dynamic tax calculation from `MockData.Settings.TaxRate`, payment method selector (`Cash`, `Card`, `Other`), live change-due calculation, and order completion that persists directly to `MockData.Sales`.

### 5. Sales / Transactions (`SalesControl` & `SaleDetailsDialog`)
- Combined filters for Search by Transaction ID, From/To Date range, Payment Method, Sale Status, and Order Type.
- Color-coded status badges (`Completed` green, `Cancelled` red, `Refunded` amber).
- Actions to **View** full itemized details (`SaleDetailsDialog`), **Print Receipt** (loads the transaction in `ReceiptControl`), or **Cancel Sale** (with confirmation dialog).

### 6. Products / Menu Management (`ProductsControl` & `ProductDialog`)
- Searchable and filterable `DataGridView` of all coffee shop products with stock and availability indicators.
- Full in-memory CRUD: **Add Product**, **Edit**, and **Delete** via `ProductDialog` and `ConfirmationDialog` with `ValidationHelper` input checks.

### 7. Categories Management (`CategoriesControl` & `CategoryDialog`)
- Displays all product categories along with dynamically computed product counts per category.
- Supports adding, editing, and deleting categories (with a safeguard warning when a category still has assigned products).

### 8. Billing / Receipts (`ReceiptControl`)
- Custom GDI+ thermal receipt renderer displaying shop header, transaction metadata, itemized quantities and line totals, subtotal/discount/tax/total breakdown, payment details, and receipt footer.
- Includes a transaction selector dropdown and **Print Preview** powered by WinForms `PrintDocument` and `PrintPreviewDialog`.

### 9. Sales Analytics (`AnalyticsControl`)
- Preset and custom date-range filters (`Today`, `Last 7 Days`, `Last 30 Days`, `Custom`).
- **4 Summary KPI Cards:** Total Revenue, Total Orders, Average Order Value, and Estimated Profit (Selling Price vs. Cost Price).
- **3 Custom GDI+ Charts:** Daily Revenue Bar Chart, Sales by Category Pie/Donut Chart with legend, and Payment Methods Horizontal Bar Chart, plus a Top 5 Best-Selling Products table with visual progress bars.

### 10. Settings (`SettingsControl`)
- Editable Shop Information (Shop Name, Address, Phone, Receipt Footer, Logo browse) and Financial Settings (Currency selector, Tax Rate %, Default Discount %).
- Saving updates `MockData.Settings` immediately so POS tax calculations, analytics currency formatting, and receipt headers reflect changes right away.
- Includes **Reset Defaults** (`Brew Haven`, `14%` tax) and User Account / Change Password validation.

---

## ⚠️ Phase 1 Limitations & Phase 2 Planned Features

### Phase 1 Limitations (Current GUI Prototype)
- **In-Memory Persistence Only:** All data (`MockData.Products`, `Categories`, `Sales`, `Settings`, `Cashiers`) is stored in memory for the duration of the application session and resets when the application restarts.
- **Simulated Hardware:** Receipt printing uses the Windows Forms `PrintPreviewDialog` / software renderer rather than ESC/POS thermal printer hardware commands, and card payments do not contact an external payment gateway.

### Phase 2 Planned Features
1. **SQL Server & ADO.NET Integration:** Replace `MockData` with a relational SQL Server database using `Microsoft.Data.SqlClient` (parameterized queries, transactions, and stored procedures).
2. **Role-Based Access Control & Password Hashing:** Enforce granular route/button permissions between `Admin` and `Cashier` roles and store salted PBKDF2/BCrypt password hashes in the database.
3. **Async Database & Network Operations:** Use `async`/`await` for non-blocking database queries, report generation, and daily backup exports (CSV/PDF).
4. **Hardware & Barcode Support:** Direct thermal receipt printing and barcode scanner input handling on the POS screen.
