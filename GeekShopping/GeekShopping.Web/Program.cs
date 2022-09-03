using GeekShopping.Web.Services;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultScheme = "Cookies";
    opts.DefaultChallengeScheme = "oidc";
})
    .AddCookie("Cookies", c => c.ExpireTimeSpan = TimeSpan.FromMinutes(300))
    .AddOpenIdConnect(
    "oidc",
    opts =>
    {
        opts.Authority = builder.Configuration["ServiceUrls:IdentityServer"];
        opts.GetClaimsFromUserInfoEndpoint = true;
        opts.ClientId = "geek_shopping";
        opts.ClientSecret = "mysupersecret";
        opts.ResponseType = "code";
        opts.ClaimActions.MapJsonKey("role", "role", "role");
        opts.ClaimActions.MapJsonKey("sub", "sub", "sub");
        opts.TokenValidationParameters.NameClaimType = "name";
        opts.TokenValidationParameters.RoleClaimType = "Role";
        opts.Scope.Add("geek_shopping");
        opts.SaveTokens = true;
    });
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddHttpClient<IProductService, ProductService>(
    c=> c.BaseAddress = new Uri(configuration["ServiceUrls:ProductAPI"])
    );

builder.Services.AddHttpClient<ICartService, CartService>(
    c => c.BaseAddress = new Uri(configuration["ServiceUrls:CartAPI"])
    );

builder.Services.AddHttpClient<ICouponService, CouponService>(
    c => c.BaseAddress = new Uri(configuration["ServiceUrls:CouponAPI"])
    );


builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
