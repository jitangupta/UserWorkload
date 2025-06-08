using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserWorkload.Context;

public class UserProfileImageViewComponent : ViewComponent
{
    private readonly DemoDeckDbContext _context;
    private readonly BlobStorageService _blobStorageService;

    public UserProfileImageViewComponent(DemoDeckDbContext context, BlobStorageService blobStorageService)
    {
        _context = context;
        _blobStorageService = blobStorageService;
    }

    public IViewComponentResult Invoke()
    {
        if (!User.Identity.IsAuthenticated)
            return Content(string.Empty);

        // Get email from claims
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var email = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Content(string.Empty);



        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        string? profileImageUrl = null;
        if (user?.PhotoUrl != null)
        {
            var blobName = System.IO.Path.GetFileName(user.PhotoUrl);
            profileImageUrl = _blobStorageService.GetUserProfileSasUrl(blobName);
        }

        var model = new UserProfileImageViewModel
        {
            Name = User.Identity.Name ?? user?.Name ?? email,
            ImageUrl = profileImageUrl
        };

        return View("Default", model);
    }
}