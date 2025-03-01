using MyECommerce.Data;
using Microsoft.EntityFrameworkCore;
using MyECommerce.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();


builder.Services.   AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConn")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<SendGridService>();
builder.Services.AddHttpContextAccessor(); // No need for AddSingleton<IHttpContextAccessor, HttpContextAccessor>()


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";  // Redirect to login page if not authenticated
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Keep user logged in for 7 days
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();



app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");


app.Run();
