using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
bool demo = builder.Configuration.GetValue<bool>("Demo");
builder.WebHost.UseUrls("http://127.0.0.1:5112");
builder.Services.AddControllers();
if (demo)
{
    builder.Services.AddDbContext<NorthwindContext>(options =>
        options.UseSqlite("Data Source=northwind-demo.db"));
}
else
{
    var connection = new SqlConnectionStringBuilder
    {
        DataSource = "127.0.0.1,14333",
        InitialCatalog = "Northwind",
        UserID = "sa",
        Password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD")
            ?? throw new InvalidOperationException("Задай MSSQL_SA_PASSWORD или запусти с --Demo=true."),
        TrustServerCertificate = true
    };
    builder.Services.AddDbContext<NorthwindContext>(options =>
        options.UseSqlServer(connection.ConnectionString));
}

var app = builder.Build();
// У стандартной Northwind схема уже установлена SQL-скриптом.
if (demo)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NorthwindContext>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Categories.AnyAsync())
    {
        db.Categories.Add(new Category { CategoryName = "Beverages", Description = "Напитки" });
        await db.SaveChangesAsync();
    }
}
app.MapControllers();
app.Run();
