
using DiscussionForum.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    // Wait for SQL Server and apply database migrations.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var retries = 10;

        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                Console.WriteLine("Database migrations applied successfully.");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration attempt {attempt} failed:");
                Console.WriteLine(ex.Message);

                if (attempt == retries)
                {
                    throw;
                }

                await Task.Delay(3000);
            }
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }