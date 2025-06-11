public interface IKeyVaultService
{
    Task<string> GetSecretAsync(SecretKey secretKey);
}