# 📊 MetaReport

**Daily MT4/MT5 Trading Reports via Azure Functions**

MetaReport is an open-source Azure Functions application that fetches your trading account data from [MetaAPI](https://metaapi.cloud/) and emails you a beautiful daily summary report. Perfect for traders who want automated insights into their trading performance.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Azure Functions](https://img.shields.io/badge/Azure%20Functions-v4-blue.svg)

## ✨ Features

- 📈 **Account Summary**: Balance, equity, margin, and leverage at a glance
- 📊 **24-Hour Trade History**: All trades from the last 24 hours with profit/loss
- 📧 **Beautiful HTML Emails**: Clean, responsive email reports via Azure Communication Services
- 👥 **Multiple Recipients**: Send reports to multiple email addresses
- ⏰ **Scheduled Reports**: Daily timer trigger (default: 8 PM your timezone)
- 🔗 **On-Demand Reports**: HTTP endpoint for instant report generation
- 💰 **Cost Effective**: Designed to run within Azure free/low-cost tiers
- 🔒 **Secure**: No secrets stored in code; all configuration via App Settings

## 📋 Prerequisites

Before you begin, you'll need:

1. **Azure Subscription** — [Create a free account](https://azure.microsoft.com/free/)
2. **MetaAPI Account** — [Sign up at metaapi.cloud](https://metaapi.cloud/)
   - Add your MT4/MT5 account to MetaAPI
   - Get your API token from [app.metaapi.cloud/token](https://app.metaapi.cloud/token)
   - Note your MetaAPI Account ID (not your MT4 login)
3. **Azure Communication Services** — Create via Azure Portal (Email service with Azure-managed domain)
4. **.NET 8 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
5. **Azure Functions Core Tools** — [Install guide](https://docs.microsoft.com/azure/azure-functions/functions-run-local)

## 🚀 Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/MetaReport.git
cd MetaReport
```

### 2. Create Local Settings

Copy the template and fill in your values:

```bash
cp local.settings.template.json local.settings.json
```

Edit `local.settings.json` with your credentials:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    
    "MetaApi__Token": "your-metaapi-token-here",
    "MetaApi__AccountId": "your-metaapi-account-id",
    "MetaApi__BaseUrl": "https://mt-client-api-v1.new-york.agiliumtrade.ai",
    
    "Email__AzureConnectionString": "endpoint=https://your-acs.communication.azure.com/;accesskey=...",
    "Email__FromAddress": "DoNotReply@your-domain.azurecomm.net",
    "Email__FromName": "MetaReport",
    "Email__ToAddresses": "trader1@email.com,trader2@email.com",
    "Email__ToName": "Recipients",
    
    "ScheduleCronExpression": "0 0 20 * * 1-5",
    "WEBSITE_TIME_ZONE": "SA Pacific Standard Time"
  }
}
```

### 3. Run Locally

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Start the function app
func start
```

### 4. Test the HTTP Endpoint

```bash
# Trigger a manual report (use the function key from func start output)
curl "http://localhost:7071/api/report"
```

## ⚙️ Configuration Reference

| Setting | Description | Required | Default |
|---------|-------------|----------|---------|
| `MetaApi__Token` | Your MetaAPI auth token | ✅ | — |
| `MetaApi__AccountId` | MetaAPI provisioned account ID | ✅ | — |
| `MetaApi__BaseUrl` | MetaAPI regional endpoint | ❌ | `https://mt-client-api-v1.new-york.agiliumtrade.ai` |
| `Email__AzureConnectionString` | Azure Communication Services connection string | ✅ | — |
| `Email__FromAddress` | Sender email (use Azure-managed domain address) | ✅ | — |
| `Email__FromName` | Sender display name | ❌ | `MetaReport` |
| `Email__ToAddresses` | Recipient emails (comma-separated for multiple) | ✅ | — |
| `Email__ToName` | Recipient display name | ❌ | — |
| `ScheduleCronExpression` | CRON expression for daily report | ❌ | `0 0 20 * * 1-5` (8 PM weekdays) |
| `WEBSITE_TIME_ZONE` | Timezone for timer trigger and email times | ❌ | `SA Pacific Standard Time` (Bogota) |

### CRON Expression Format

The format is: `{second} {minute} {hour} {day} {month} {day-of-week}`

Examples:
- `0 0 20 * * *` — 8:00 PM daily
- `0 30 8 * * *` — 8:30 AM daily
- `0 0 9,21 * * *` — 9 AM and 9 PM daily
- `0 0 20 * * 1-5` — 8 PM on weekdays only

### Timezone Configuration

Set `WEBSITE_TIME_ZONE` to your local timezone. This setting is used for:
- ⏰ **Timer trigger**: The daily report runs at the configured time in your timezone
- 📧 **Email times**: All times in the email (report generation, deal times) are converted to your timezone

Common values:

| Timezone | Value |
|----------|-------|
| Bogota, Colombia (UTC-5) | `SA Pacific Standard Time` |
| New York (UTC-5/-4) | `Eastern Standard Time` |
| London (UTC+0/+1) | `GMT Standard Time` |
| Tokyo (UTC+9) | `Tokyo Standard Time` |

> ⚠️ **Note**: Timezone setting only works on **Windows** Consumption plan. The project is configured for Windows deployment.

## ☁️ Deploying to Azure

### Option 1: Azure Portal (Manual)

1. **Create a Function App**:
   - Go to [Azure Portal](https://portal.azure.com)
   - Create a new Function App
   - Runtime: `.NET 8 (Isolated)`
   - Operating System: **Windows**
   - Plan: **Consumption (Serverless)**

2. **Configure App Settings**:
   - Go to Function App → Configuration → Application settings
   - Add all settings from the Configuration Reference table above

3. **Deploy**:
   ```bash
   func azure functionapp publish <your-function-app-name>
   ```

### Option 2: GitHub Actions (Automated)

1. **Get Publish Profile**:
   - Go to Function App → Overview → Get publish profile
   - Download the `.PublishSettings` file

2. **Add GitHub Secret**:
   - Go to your GitHub repo → Settings → Secrets → Actions
   - Create `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`
   - Paste the entire contents of the publish profile

3. **Update Workflow**:
   - Edit `.github/workflows/deploy.yml`
   - Change `AZURE_FUNCTIONAPP_NAME` to your function app name

4. **Push to Master**:
   ```bash
   git push origin master
   ```
   The workflow will automatically build and deploy.

## 💰 Cost Estimation (Free Tier)

MetaReport is designed to run within free tier limits:

| Service | Free Tier / Cost | MetaReport Usage |
|---------|-----------|------------------|
| **Azure Functions** | 1M executions/month free | ~60 executions/month (2/day) |
| **Azure Storage** | — | ~$0.10-0.50/month* |
| **Azure Communication Services** | First 1000 emails free, then ~$0.00025/email | 1-3 emails/day |
| **GitHub Actions** | Unlimited (public repos) | ~2 min/deployment |

\* Azure Storage is required for timer trigger state and is not included in free tier, but costs are minimal.

### Why Azure Storage is Required

Azure Functions uses a Storage Account for:
- 📅 Timer trigger state (tracks last execution time)
- 🔒 Singleton coordination (prevents duplicate runs)
- 🔑 Function keys storage
- 📦 Deployment package storage

## 🏗️ Project Structure

```
MetaReport/
├── .github/workflows/deploy.yml   # GitHub Actions CI/CD
├── Functions/
│   ├── DailyReportFunction.cs     # Timer trigger (8 PM daily)
│   └── ManualReportFunction.cs    # HTTP GET /api/report
├── Models/
│   ├── AccountInfo.cs             # MetaAPI account response
│   ├── Deal.cs                    # Trade/deal history item
│   ├── TradingReport.cs           # Aggregated report data
│   └── Options/
│       ├── MetaApiOptions.cs      # MetaAPI config binding
│       └── EmailOptions.cs        # Email config binding
├── Services/
│   ├── IMetaApiService.cs         # MetaAPI interface
│   ├── MetaApiService.cs          # MetaAPI implementation
│   ├── IEmailService.cs           # Email interface
│   ├── IReportFormatter.cs        # Report formatting interface
│   ├── ReportFormatter.cs         # Report formatting with timezone support
│   └── AzureEmailService.cs       # Azure Communication Services implementation
├── Program.cs                     # DI and startup config
├── host.json                      # Azure Functions host config
├── local.settings.template.json   # Settings template (safe to commit)
└── local.settings.json            # Your local settings (gitignored)
```

## 🔧 Development

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Azurite](https://docs.microsoft.com/azure/storage/common/storage-use-azurite) (local storage emulator) or Azure Storage connection string

### Running Tests

```bash
dotnet test
```

### Building for Production

```bash
dotnet publish --configuration Release --output ./publish
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [MetaAPI](https://metaapi.cloud/) for the trading account API
- [Azure Communication Services](https://azure.microsoft.com/services/communication-services/) for email delivery
- [Azure Functions](https://azure.microsoft.com/services/functions/) for serverless compute

---

**Made with ❤️ for traders who love automation**
