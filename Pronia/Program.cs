using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Interfaces;
using Pronia.Middlewares;
using Pronia.Models;
using Pronia.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<LayoutService>();
builder.Services.AddScoped<IEmailService,EmailService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{ options.Password.RequiredLength = 8;
  options.Password.RequireNonAlphanumeric = false;
  options.Password.RequireLowercase = true;
  options.Password.RequireUppercase = true;
  options.Password.RequireDigit = true;

    options.User.AllowedUserNameCharacters = default;
    options.User.RequireUniqueEmail = true;

    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);

}
).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
var app = builder.Build();

app.UseRouting();
app.UseAuthentication(); //Bu ardiciliqla olmalidi, yoxsa cookies'de problem yarana biler
app.UseAuthorization();
app.UseStaticFiles();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.MapControllerRoute(
    "Category",
    "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    "default",
    "{controller=home}/{action=index}/{id?}"
    );


app.Run();
