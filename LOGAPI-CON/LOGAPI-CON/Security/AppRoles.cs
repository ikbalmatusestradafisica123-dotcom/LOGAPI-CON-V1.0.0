namespace LOGAPI_CON.Security;

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string Owner = "Owner";
    public const string IT = "IT";
    public const string Logistics = "Logistics";
    public const string Supervisor = "Supervisor";
    public const string Sales = "Sales";
    public const string Inventory = "Inventory";
    public const string Purchasing = "Purchasing";
    public const string Warehouse = "Warehouse";
    public const string Driver = "Driver";

    public static readonly IReadOnlyCollection<string> All =
    [
    Administrator,
    Owner,
    IT,
    Logistics,
    Supervisor,
    Sales,
    Inventory,
    Purchasing,
    Warehouse,
    Driver
    ];
};


