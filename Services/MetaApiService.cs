using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MetaReport.Models;
using MetaReport.Models.Options;

namespace MetaReport.Services;

/// <summary>
/// Service for interacting with MetaAPI REST endpoints.
/// </summary>
public class MetaApiService : IMetaApiService
{
    private readonly HttpClient _httpClient;
    private readonly MetaApiOptions _options;
    private readonly ILogger<MetaApiService> _logger;

    public MetaApiService(
        HttpClient httpClient,
        IOptions<MetaApiOptions> options,
        ILogger<MetaApiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Configure base address and auth header
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("auth-token", _options.Token);
    }

    /// <inheritdoc />
    public async Task<AccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = $"/users/current/accounts/{_options.AccountId}/account-information";
        
        _logger.LogInformation("Fetching account information from MetaAPI");

        try
        {
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var accountInfo = await response.Content.ReadFromJsonAsync<AccountInfo>(cancellationToken: cancellationToken);
            
            if (accountInfo == null)
            {
                throw new InvalidOperationException("Received null response from MetaAPI account-information endpoint");
            }

            _logger.LogInformation("Successfully retrieved account info. Balance: {Balance} {Currency}", 
                accountInfo.Balance, accountInfo.Currency);

            return accountInfo;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch account information from MetaAPI");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<Deal>> GetDealsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        // Format times in ISO 8601 format for MetaAPI
        var startTimeStr = startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var endTimeStr = endTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        
        var endpoint = $"/users/current/accounts/{_options.AccountId}/history-deals/time/{startTimeStr}/{endTimeStr}";
        
        _logger.LogInformation("Fetching deals from {StartTime} to {EndTime}", startTimeStr, endTimeStr);

        try
        {
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var deals = await response.Content.ReadFromJsonAsync<List<Deal>>(cancellationToken: cancellationToken);
            
            if (deals == null)
            {
                _logger.LogWarning("Received null response from MetaAPI history-deals endpoint, returning empty list");
                return new List<Deal>();
            }

            _logger.LogInformation("Successfully retrieved {Count} deals", deals.Count);

            return deals;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch deals from MetaAPI");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<Deal>> GetLast24HoursDealsAsync(CancellationToken cancellationToken = default)
    {
        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddHours(-24);
        
        return await GetDealsAsync(startTime, endTime, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TradingReport> GenerateReportAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating trading report");

        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddHours(-24);

        // Fetch account info and deals in parallel
        var accountInfoTask = GetAccountInfoAsync(cancellationToken);
        var dealsTask = GetDealsAsync(startTime, endTime, cancellationToken);

        await Task.WhenAll(accountInfoTask, dealsTask);

        var report = new TradingReport
        {
            Account = await accountInfoTask,
            RecentDeals = await dealsTask,
            GeneratedAt = DateTime.UtcNow,
            PeriodStart = startTime,
            PeriodEnd = endTime
        };

        _logger.LogInformation("Report generated: {TradeCount} trades, Total P/L: {TotalProfit} {Currency}", 
            report.TradeCount, report.TotalProfit, report.Account.Currency);

        return report;
    }
}
