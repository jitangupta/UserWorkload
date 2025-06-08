using Azure.Security.KeyVault.Secrets;

namespace UserWorkload.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecretAsync(string secretName);
        Task SetSecretAsync(string secretName, string secretValue);
    }

    public class KeyVaultService : IKeyVaultService
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<KeyVaultService> _logger;

        public KeyVaultService(SecretClient secretClient, ILogger<KeyVaultService> logger)
        {
            _secretClient = secretClient;
            _logger = logger;
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                return secret.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secret {SecretName}", secretName);
                throw;
            }
        }

        public async Task SetSecretAsync(string secretName, string secretValue)
        {
            try
            {
                await _secretClient.SetSecretAsync(secretName, secretValue);
                _logger.LogInformation("Secret {SecretName} updated successfully", secretName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set secret {SecretName}", secretName);
                throw;
            }
        }
    }
}
