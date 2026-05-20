using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using eduRateSystem.Data;
using eduRateSystem.Models;
using eduRateSystem.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await IdentitySeeder.SeedRolesAsync(roleManager);
        await IdentitySeeder.SeedAdminUserAsync(userManager, roleManager);
    }
}
catch (Exception ex)
{
    Console.WriteLine("STARTUP ERROR:");
    Console.WriteLine(ex.ToString());
    throw;
}

app.Run();
