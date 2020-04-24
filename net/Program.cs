using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using DotNetEnv;

namespace azure_storage_azurite_azuresdks_storage_explorer
{
     class Program
    {
        static async Task Main(string[] args)
        {
            Env.Load("../.env");

            var blobHost = Environment.GetEnvironmentVariable("AZURE_STORAGE_BLOB_HOST");
            var queueHost = Environment.GetEnvironmentVariable("AZURE_STORAGE_QUEUE_HOST");
            var account = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT");
            var accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_KEY");
            var container = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER");
            var queue = Environment.GetEnvironmentVariable("AZURE_STORAGE_QUEUE");
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            var emulator = account == "devstoreaccount1";
            var blobBaseUri = $"https://{(emulator ? $"{blobHost}/{account}" : $"{account}.{blobHost}")}/";
            var queueBaseUri = $"https://{(emulator ? $"{queueHost}/{account}" : $"{account}.{queueHost}")}/";

            var blobContainerUri = $"{blobBaseUri}{container}";
            var queueUri = $"{queueBaseUri}{queue}";

            // Generate random string for blob content and file name
            var content = Guid.NewGuid().ToString("n").Substring(0, 8);
            var file = $"{content}.txt";

            // For Azurite 3.7+ with HTTPS and OAuth enabled
            // azurite --oauth basic --cert cert-name.pem --key cert-name-key.pem

            // With container uri and DefaultAzureCredential
            var client = new BlobContainerClient(new Uri(blobContainerUri), new DefaultAzureCredential());

            // With connection string
            // var client = new BlobContainerClient(connectionString, container);

            // With account name and key
            // var client = new BlobContainerClient(new Uri(blobContainerUri), new StorageSharedKeyCredential(account, accountKey));

            // Create container
            await client.CreateIfNotExistsAsync();

            // Get content stream
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(content));

            // Upload blob
            await client.UploadBlobAsync(file, stream);

            // Construct QueueClient

            // With connection string
            // var qclient = new QueueClient(connectionString, queue);

            // With uri and DefaultAzureCredential()
            // var qclient = new QueueClient(new Uri(queueUri), new DefaultAzureCredential());

            var qclient = new QueueClient(new Uri(queueUri), new StorageSharedKeyCredential(account, accountKey));


            // Create queue
            await qclient.CreateIfNotExistsAsync();

            // Send message to queue
            await qclient.SendMessageAsync(content);

        }
    }
}
