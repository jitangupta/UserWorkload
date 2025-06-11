using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using UserWorkload.Context;

public class LoginModel : PageModel
{
    private readonly DemoDeckDbContext _db;
    private readonly IConfiguration _configuration;

    public LoginModel(DemoDeckDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; }
    [BindProperty, Required]
    public string Password { get; set; }
    public string ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Email and password are required.";
            return Page();
        }

        var encryptionKey = _configuration.GetValue<string>("Encryption:Key");
        var hasher = new PasswordHasher(encryptionKey);

        var user = _db.Users.FirstOrDefault(u => u.Email == Email);
        if (user == null || string.IsNullOrEmpty(user.PasswordSalt))
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        if (!hasher.VerifyPassword(Password, user.PasswordHash, user.PasswordSalt))
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToPage("/Index");
    }
}