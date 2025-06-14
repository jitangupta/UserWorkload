using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using UserWorkload.Context;
using UserWorkload.Models;
using Azure.Storage.Blobs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace UserWorkload.Pages
{
    [Authorize]
    public class UserCreateModel : PageModel
    {
        private readonly DemoDeckDbContext _db;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public UserCreateModel(DemoDeckDbContext db, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _db = db;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        [BindProperty, Required]
        public string Name { get; set; }
        [BindProperty, Required, EmailAddress]
        public string Email { get; set; }
        [BindProperty]
        public string Bio { get; set; }
        [BindProperty, Required]
        public IFormFile Photo { get; set; }
        [BindProperty, Required]
        public string Password { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Check if email already exists (case-insensitive)
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email.ToLower() == Email.ToLower());
            if (emailExists)
            {
                ErrorMessage = "A user with this email already exists.";
                return Page();
            }

            // Validate file type
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(Photo.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || Array.IndexOf(allowedTypes, ext) < 0)
            {
                ErrorMessage = "Only JPG, JPEG, and PNG files are allowed.";
                return Page();
            }

            // Process and compress image using ImageSharp
            byte[] imageBytes;
            using (var inputStream = Photo.OpenReadStream())
            using (var image = Image.Load(inputStream))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(64, 64),
                    Mode = ResizeMode.Crop
                }));

                using (var outputStream = new MemoryStream())
                {
                    image.Save(outputStream, new JpegEncoder { Quality = 75 });
                    imageBytes = outputStream.ToArray();
                }
            }

            // Upload to Azure Blob Storage
            var container = _blobServiceClient.GetBlobContainerClient("user-attachments");
            await container.CreateIfNotExistsAsync();
            var blobName = $"{Guid.NewGuid()}.jpg";
            var blobClient = container.GetBlobClient(blobName);
            using (var uploadStream = new MemoryStream(imageBytes))
            {
                await blobClient.UploadAsync(uploadStream, overwrite: true);
            }
            var photoUrl = blobClient.Uri.ToString();

            // Hash password with salt
            var encryptionKey = _configuration.GetValue<string>("Encryption:Key");
            var hasher = new PasswordHasher(encryptionKey);
            var (hash, salt) = hasher.HashPassword(Password);

            var user = new User
            {
                Name = Name,
                Email = Email,
                Bio = Bio,
                PhotoUrl = photoUrl,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return RedirectToPage("/User");
        }
    }
}