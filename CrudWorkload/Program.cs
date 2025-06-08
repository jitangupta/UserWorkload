using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using UserWorkload.Context;
using UserWorkload.Services;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
var credential = new DefaultAzureCredential();
var secretClient = new SecretClient(new Uri(keyVaultUri), credential);

// Database
var connectionString = await secretClient.GetSecretAsync("DemoDeckDbConnectionString");
builder.Services.AddDbContext<DemoDeckDbContext>(options =>
    options.UseSqlServer(connectionString.Value.Value));

// Register the SecretClient and KeyVaultService
builder.Services.AddSingleton(secretClient);
builder.Services.AddScoped<IKeyVaultService, KeyVaultService>();

// Add services to the container.
builder.Services.AddRazorPages();

//// Azure Blob Storage
//builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetConnectionString("AzureStorage")));
//builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
