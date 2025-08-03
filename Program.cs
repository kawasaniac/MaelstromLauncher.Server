using MaelstromLauncher.Server.Services;

namespace MaelstromLauncher.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ValidateConfiguration(builder.Configuration);

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<ManifestService>();
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
        var requiredSettings = new[] { "GameDirectory:Path", "DataDirectory:Path", "Server:ServerUrl" };
        foreach (var setting in requiredSettings)
        {
            if (string.IsNullOrEmpty(configuration[setting]))
                throw new InvalidOperationException($"Required configuration {setting} is missing");
        }
    }
}


