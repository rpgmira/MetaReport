using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using SendGrid;
using MetaReport.Models.Options;
using MetaReport.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure Application Insights (optional but helpful for monitoring)
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Bind configuration options
builder.Services.Configure<MetaApiOptions>(
    builder.Configuration.GetSection(MetaApiOptions.SectionName));

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

// Configure HttpClient for MetaAPI with retry policy
builder.Services.AddHttpClient<IMetaApiService, MetaApiService>()
    .AddPolicyHandler(GetRetryPolicy());

// Configure SendGrid client
builder.Services.AddSingleton<ISendGridClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiKey = configuration.GetSection(EmailOptions.SectionName)["SendGridApiKey"] 
        ?? throw new InvalidOperationException("SendGrid API key not configured");
    return new SendGridClient(apiKey);
});

// Register email service
builder.Services.AddSingleton<IEmailService, SendGridEmailService>();

builder.Build().Run();

/// <summary>
/// Creates a retry policy for HTTP requests with exponential backoff.
/// Handles transient HTTP errors and rate limiting (429).
/// </summary>
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Console.WriteLine($"Retry {retryAttempt} after {timespan.TotalSeconds}s due to {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
            });
}
