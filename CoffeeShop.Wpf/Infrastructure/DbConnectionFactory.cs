namespace CoffeeShop.Wpf.Infrastructure;

public static class DbConnectionFactory
{
    public static string ConnectionString { get; private set; }
        = "Server=LAPTOP-VHGPK0SP;Database=CoffeeShopDb;Trusted_Connection=True;TrustServerCertificate=True";

    public static void Configure(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string không được để trống.", nameof(connectionString));
        }

        ConnectionString = connectionString;
    }
}
