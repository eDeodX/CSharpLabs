using Lab12;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5113");
builder.Services.AddRazorPages();
builder.Services.AddDbContext<HospitalContext>(options =>
    options.UseSqlite("Data Source=hospital.db"));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HospitalContext>();
    await db.Database.EnsureCreatedAsync();
}
app.UseStaticFiles();
app.MapRazorPages();
app.Run();
