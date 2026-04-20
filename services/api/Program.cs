using Common.Configuration;
using Common.Llm;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppConfig>(options =>
{
    options.AspNetCoreEnvironment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
    options.CorsOrigins = builder.Configuration["CORS_ORIGINS"] ?? string.Empty;
    options.LlmProvider = builder.Configuration["LlmProvider"] ?? "mistral";
    options.LlmApiKey = builder.Configuration["LlmApiKey"] ?? string.Empty;
    options.LlmBaseUrl = builder.Configuration["LlmBaseUrl"] ?? "https://api.mistral.ai";
    options.LlmChatModel = builder.Configuration["LlmChatModel"] ?? "mistral-small-latest";
});

builder.Services.AddSingleton<IMistralClient>(sp =>
{
    var config = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppConfig>>().Value;
    return LlmClientFactory.Create(config);
});

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration["CORS_ORIGINS"] ?? "";
    options.AddDefaultPolicy(policy =>
    {
        if (!string.IsNullOrEmpty(origins) && origins.Trim() != "*")
        {
            policy.WithOrigins(origins.Split(',', StringSplitOptions.TrimEntries))
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();
