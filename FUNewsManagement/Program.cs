using DataAccessObject;
using DataAccessObjects;
using FUNewsManagement.Hubs;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// ==================== SESSION CONFIGURATION ====================
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout 30 phút
    options.Cookie.HttpOnly = true; // Bảo mật: không cho JavaScript access cookie
    options.Cookie.IsEssential = true; // GDPR compliance
    options.Cookie.Name = ".FUNewsManagement.Session";
});

// ==================== DATABASE CONFIGURATION ====================
builder.Services.AddDbContext<FUNewsManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

// ==================== REPOSITORIES (DAO) ====================
builder.Services.AddScoped<ICategoryRepository, CategoryDAO>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleDAO>();
builder.Services.AddScoped<ISystemAccountRepository, SystemAccountDAO>();
builder.Services.AddScoped<ITagRepository, TagDAO>();

// ==================== SERVICES ====================
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<Services.IAuthenticationService, Services.AuthenticationService>();
builder.Services.AddSignalR();

// ==================== HTTP CONTEXT ACCESSOR ====================
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var accountService = scope.ServiceProvider.GetRequiredService<ISystemAccountService>();
        await accountService.EnsureDefaultAdminAsync();
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ==================== SESSION MIDDLEWARE (QUAN TRỌNG) ====================
app.UseSession();
app.MapHub<SignalrServer>("/SignalrServer");
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=News}/{action=Index}/{id?}");

app.Run();