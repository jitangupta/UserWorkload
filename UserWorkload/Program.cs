using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UserWorkload.Context;
using UserWorkload.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IKeyVaultService, KeyVaultService>();

if (!builder.Environment.IsDevelopment())
{
    var keyVaultEndpoint = new Uri(builder.Configuration["KeyVault:VaultUri"]);
    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
}

var connectionString = builder.Configuration["Secrets:DemoDeckDbConnectionString"];
builder.Services.AddDbContext<DemoDeckDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add BlobStorageService
var storageConnectionString = builder.Configuration["Secrets:StorageConnectionString"];
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
