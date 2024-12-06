using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adaugă serviciul de sesiune
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Durata sesiunii
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Necesar pentru GDPR
});

builder.Services.AddScoped<IRetetaService, RetetaService>();
builder.Services.AddScoped<IRecenzieService, RecenzieService>();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<HealPlateDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<HealPlateDbContext>();
builder.Services.AddRazorPages();

var app = builder.Build();
// Activează utilizarea sesiunii
app.UseSession();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();