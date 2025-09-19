using MaelstromLauncher.Server.Helpers;
using MaelstromLauncher.Server.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

namespace MaelstromLauncher.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ValidateConfiguration(builder.Configuration);

        builder.Logging.SetMinimumLevel(LogLevel.Debug); // TEMP

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Maelstrom Launcher Server API", Version = "v1" });
        });

        builder.Services.AddScoped<ManifestService>();
        builder.Services.AddScoped<GameLauncherService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IServerUrlProvider, ServerUrlProvider>();
        builder.Services.AddHostedService<FileWatcherService>();

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        var gameDirectory = builder.Configuration["GameDirectory:Path"];
        if (!string.IsNullOrEmpty(gameDirectory))
        {
            // Ensure absolute path
            if (!Path.IsPathRooted(gameDirectory))
            {
                gameDirectory = Path.Combine(builder.Environment.ContentRootPath, gameDirectory);
            }

            var fileProvider = new PhysicalFileProvider(gameDirectory);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = "",
                ServeUnknownFileTypes = true,
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
                }
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = fileProvider,
                RequestPath = ""
            });
        }

        //app.UseHttpsRedirection(); TODO: Enable when we will have HTTPS
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    static void ValidateConfiguration(ConfigurationManager configuration)
    {
        var requiredSettings = new (string Key, string DefaultValue)[]
        {
            ("GameDirectory:Path", "/opt/maelstrom-launcher/files"),
            ("DataDirectory:Path", "/opt/maelstrom-launcher/data")
        };

        foreach (var (key, defaultValue) in requiredSettings)
        {
            var value = configuration[key] ?? defaultValue;
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException($"Required configuration '{key}' is missing");
        }
    }
}