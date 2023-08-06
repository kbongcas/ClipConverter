using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using ClipConverter.Models;
using ClipConverter.Services;
using FFmpeg.NET;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ClipConverter;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder();
        BuildConfig(builder);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var queueOpts = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };
                var azureStorageConnection = Environment.GetEnvironmentVariable("AzureStorageConnection");
                services.AddSingleton<QueueServiceClient>(x =>
                    new QueueServiceClient(azureStorageConnection, queueOpts));

                services.AddSingleton<BlobServiceClient>(x =>
                    new BlobServiceClient(azureStorageConnection));

                services.AddSingleton<IQueueService, QueueService>();

                services.AddSingleton<IClipConverterService, ClipConverterService>();

                services.AddSingleton<IStorageService, StorageService>();
            })
            .UseSerilog()
            .Build();

        var queueService = ActivatorUtilities.CreateInstance<QueueService>(host.Services);
        var storageService = ActivatorUtilities.CreateInstance<StorageService>(host.Services);
        var clipConverterService = ActivatorUtilities.CreateInstance<ClipConverterService>(host.Services);
        var clipConverterRunner = new ClipConverterRunner(
            queueService,
            storageService,
            clipConverterService
            );
        await clipConverterRunner.Run();
    }

    static void BuildConfig(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();
    }
}
