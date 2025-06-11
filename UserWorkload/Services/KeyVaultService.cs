
namespace UserWorkload.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private readonly ILogger<KeyVaultService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;

        public KeyVaultService(ILogger<KeyVaultService> logger, IConfiguration configuration, IHostEnvironment env)
        {
            _logger = logger;
            _configuration = configuration;
            _env = env;
        }

        private static string GetKeyName(SecretKey key) => key switch
        {
            SecretKey.PasswordEncryptionKey => "PasswordEncryptionKey",
            SecretKey.DemoDeckDbConnectionString => "DemoDeckDbConnectionString",
            SecretKey.StorageConnectionString => "StorageConnectionString",
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
        };

        public Task<string> GetSecretAsync(SecretKey secretKey)
        {
            var keyName = GetKeyName(secretKey);
            var value = _configuration[$"Secrets:{keyName}"];
            if (string.IsNullOrEmpty(value))
            {
                _logger.LogError("Secret '{SecretName}' not found in configuration.", keyName);
                throw new InvalidOperationException($"Secret '{keyName}' not found in configuration.");
            }
            return Task.FromResult(value);
        }
    }
}
