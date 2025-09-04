using MaelstromLauncher.Server.Helpers;
using MaelstromLauncher.Server.Services;
using Microsoft.OpenApi.Models;

namespace MaelstromLauncher.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ValidateConfiguration(builder.Configuration);

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

        var app = builder.Build();

        // HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    static void ValidateConfiguration(ConfigurationManager configuration)
    {
        var requiredSettings = new[] { "GameDirectory:Path", "DataDirectory:Path" };
        foreach (var setting in requiredSettings)
        {
            if (string.IsNullOrEmpty(configuration[setting]))
                throw new InvalidOperationException($"Required configuration {setting} is missing");
        }
    }
}


