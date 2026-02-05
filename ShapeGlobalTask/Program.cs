using Serilog;
using Serilog.Events;
using ShapeGlobalTask.HealthChecks;
using ShapeGlobalTask.Middleware;
using ShapeGlobalTask.Repositories;
using ShapeGlobalTask.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "ShapeGlobalTask")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] [{ThreadId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting ShapeGlobalTask Windows Service...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseWindowsService();
    builder.Host.UseSerilog();

    builder.Services.AddSingleton<IUserRepository, JsonUserRepository>();
    builder.Services.AddScoped<IUserService, UserService>();

    var aiServiceBaseUrl = builder.Configuration["AIService:BaseUrl"];
    var aiServiceTimeout = builder.Configuration.GetValue<int>("AIService:TimeoutSeconds", 30);

    if (!string.IsNullOrEmpty(aiServiceBaseUrl))
    {
        builder.Services.AddHttpClient<IAIService, AIService>(client =>
        {
            client.BaseAddress = new Uri(aiServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(aiServiceTimeout);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        Log.Information("AI Service configured with base URL: {AIServiceBaseUrl}", aiServiceBaseUrl);
    }
    else
    {
        Log.Warning("AI Service not configured - CreateUserWithInsights will work without AI analysis");
    }

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "ShapeGlobalTask User API",
            Version = "v1",
            Description = "Self-hosted Windows Service API for user management with AI-ready fields"
        });
    });

    builder.Services.AddHealthChecks()
        .AddCheck<FileStorageHealthCheck>("file_storage", tags: new[] { "storage", "ready" });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        await userRepository.InitializeAsync();
        Log.Information("User repository initialized successfully");
    }

    
    app.UseCorrelationId();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ShapeGlobalTask API v1");
        options.RoutePrefix = "swagger";
    });

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("ShapeGlobalTask service started. Listening on configured endpoints.");
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ShapeGlobalTask service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }

