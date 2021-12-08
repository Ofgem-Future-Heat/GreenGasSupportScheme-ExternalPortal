namespace ExternalPortal.Configuration
{
    public class ServiceConfig
    {
        public string KeyVaultUri { get; set; }

        public string StorageAccountBlobBaseUrl { get; set; }

        public string StorageContainerName { get; set; }

        public string StorageBlobName { get; set; }

        public ApiConfig Api { get; set; }
    }
}
