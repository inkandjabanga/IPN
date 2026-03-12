using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using IPN.Web.Models;

namespace IPN.Web.Controllers;

/// <summary>
/// Controller handling authentication and payment operations
/// Security: All actions require session-based authentication
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Constructor - injects logger for debugging and auditing
    /// </summary>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// GET: /Home/Login
    /// Displays the login form
    /// </summary>
    /// <returns>Login view</returns>
    [HttpGet]
    public IActionResult Login()
    {
        // Security: If already logged in, redirect to index
        if (HttpContext.Session.GetString("IsLoggedIn") == "true")
        {
            return RedirectToAction("Index");
        }
        return View();
    }

    /// <summary>
    /// POST: /Home/Login
    /// Processes login credentials
    /// Security: In production, use proper authentication (ASP.NET Identity, etc.)
    /// </summary>
    /// <param name="username">User's username</param>
    /// <param name="password">User's password</param>
    /// <returns>Redirect to Index on success, or Login view with error</returns>
    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // Security: Validate inputs are not empty
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Please enter username and password";
            return View();
        }

        // Security: In production, validate against database with hashed passwords
        // This is a demo login - accepts any credentials
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            // Security: Store session with user identifier
            HttpContext.Session.SetString("IsLoggedIn", "true");
            
            // Security: Don't store sensitive data in session
            // Store display name only (sanitized)
            HttpContext.Session.SetString("Username", SanitizeInput(username));
            
            _logger.LogInformation("User {Username} logged in successfully", username);
            return RedirectToAction("Index");
        }
        
        ViewBag.Error = "Invalid credentials";
        return View();
    }

    /// <summary>
    /// GET: /Home/Logout
    /// Logs out the current user and clears session
    /// </summary>
    /// <returns>Redirect to Login page</returns>
    [HttpGet]
    public IActionResult Logout()
    {
        var username = HttpContext.Session.GetString("Username");
        
        // Security: Clear all session data
        HttpContext.Session.Clear();
        
        _logger.LogInformation("User {Username} logged out", username);
        
        return RedirectToAction("Login");
    }

    /// <summary>
    /// GET: /Home/Index
    /// Displays the payment form
    /// Security: Requires authentication (session check)
    /// </summary>
    /// <returns>Payment view with new client reference</returns>
    [HttpGet]
    public IActionResult Index()
    {
        // Security: Check authentication
        if (HttpContext.Session.GetString("IsLoggedIn") != "true")
        {
            _logger.LogWarning("Unauthorized access attempt to payment page");
            return RedirectToAction("Login");
        }
        
        // Generate unique client reference for each page load
        // Security: Use GUID to prevent prediction
        var model = new P2PPaymentViewModel
        {
            ClientReference = $"REF-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}"
        };
        
        // Pass sanitized username to view
        ViewBag.Username = SanitizeInput(HttpContext.Session.GetString("Username") ?? "User");
        
        return View(model);
    }

    /// <summary>
    /// GET: /Home/Privacy
    /// Displays privacy information
    /// </summary>
    /// <returns>Privacy view</returns>
    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// GET: /Home/Error
    /// Displays error page
    /// Security: Prevents caching of error pages
    /// </summary>
    /// <returns>Error view</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel 
        { 
            // Security: Don't expose internal request IDs in production
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }

    /// <summary>
    /// Helper: Sanitizes user input to prevent XSS attacks
    /// </summary>
    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        // Remove potential HTML/script tags
        return input.Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&#x27;")
                    .Trim();
    }
}
