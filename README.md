# PoRemoveBad

A powerful text processing application built with Blazor WebAssembly and .NET 9.

## Features

- Advanced text analysis and statistics
- Word replacement and text processing
- Buzzword detection and removal
- Export functionality for processed text
- Real-time diagnostics and health monitoring

## Live Application

🌐 **Live Demo**: [https://app-hdoqo3wpwuuja.azurewebsites.net](https://app-hdoqo3wpwuuja.azurewebsites.net)

📊 **Health Check**: [https://app-hdoqo3wpwuuja.azurewebsites.net/diag](https://app-hdoqo3wpwuuja.azurewebsites.net/diag)

## Architecture

- **Frontend**: Blazor WebAssembly
- **Backend**: .NET 9 Web API
- **Cloud**: Azure App Service
- **CI/CD**: GitHub Actions
- **Infrastructure**: Azure Bicep

## Development

### Prerequisites

- .NET 9 SDK
- Azure CLI
- Azure Developer CLI (azd)

### Local Development

```bash
# Clone the repository
git clone https://github.com/punkouter25/PoRemoveBad.git
cd PoRemoveBad

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project PoRemoveBad.Server
```

### Deployment

The application uses Azure Developer CLI for deployment:

```bash
# Deploy to Azure
azd up

# Deploy code changes only
azd deploy
```

## CI/CD Pipeline

The project includes automated GitHub Actions workflows:

- **Build and Test**: Runs on every push and PR
- **Deploy**: Automatically deploys to Azure on main branch commits

## Project Structure

```
PoRemoveBad/
├── PoRemoveBad.Client/     # Blazor WebAssembly client
├── PoRemoveBad.Server/     # .NET Web API server
├── PoRemoveBad.Core/       # Shared business logic
├── infra/                  # Azure infrastructure (Bicep)
└── .github/workflows/      # CI/CD pipelines
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests locally
5. Submit a pull request

## License

This project is licensed under the MIT License.
