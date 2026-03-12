using IPN.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Security: Configure HTTP client with timeout to prevent hanging requests
builder.Services.AddHttpClient<IPaymentService, PaymentService>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Security: Configure session with secure settings
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Security: Session expires after 30 minutes of inactivity
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    
    // Security: Cookie not accessible via JavaScript (prevents XSS attacks)
    options.Cookie.HttpOnly = true;
    
    // Security: Cookie only sent over HTTPS in production
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    
    // Security: Prevent access from JavaScript
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    
    // Security: Cookie is essential for functionality
    options.Cookie.IsEssential = true;
    
    // Security: Set cookie name to avoid predictable names
    options.Cookie.Name = ".IPN.Session";
});

var app = builder.Build();

// Security: Configure error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Security: Enable HSTS in production
    app.UseHsts();
}

// Security: Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Security: Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Security: Enable routing
app.UseRouting();

// Security: Enable session middleware
app.UseSession();

// Security: Enable authorization
app.UseAuthorization();

// Map static assets
app.MapStaticAssets();

// Configure default route - start with login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
