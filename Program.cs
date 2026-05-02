using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Data;

var builder = WebApplication.CreateBuilder(args);

var testingDbName = builder.Environment.IsEnvironment("Testing")
    ? $"PerryHomesTests_{Guid.NewGuid():N}"
    : null;

builder.Services.AddDbContext<PerryHomesDbContext>(options =>
{
    if (testingDbName != null)
        options.UseInMemoryDatabase(testingDbName);
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IPerryHomesDbContext>(sp => sp.GetRequiredService<PerryHomesDbContext>());

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PerryHomesDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
