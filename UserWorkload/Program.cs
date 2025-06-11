using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UserWorkload.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    if (string.IsNullOrWhiteSpace(keyVaultName))
    {
        throw new Exception("KeyVaultName is not configured.");
    }
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

    builder.Configuration.AddAzureKeyVault(
        keyVaultUri,
        new DefaultAzureCredential(),
        new Azure.Extensions.AspNetCore.Configuration.Secrets.KeyVaultSecretManager());
}

var connectionString = builder.Configuration["ConnectionStrings:DemoDeckDb"];
builder.Services.AddDbContext<DemoDeckDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add BlobStorageService
var storageConnectionString = builder.Configuration["Storage:Blob"];
builder.Services.AddSingleton(new BlobStorageService(storageConnectionString));
builder.Services.AddSingleton(x => new BlobServiceClient(storageConnectionString));

// Add services to the container.
builder.Services.AddRazorPages();

// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Redirect to /Login if not authenticated
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/api/dbstatus", async (DemoDeckDbContext db) =>
{
    try
    {
        // Try a lightweight query
        await db.Users.AnyAsync();
        return Results.Ok(new { status = "ready" });
    }
    catch
    {
        return Results.Ok(new { status = "starting" });
    }
});

app.Run();
