# PoRemoveBad

PoRemoveBad is a focused text-processing application that identifies and removes unwanted words and "buzzwords" from input text, produces statistics, and allows export of processed text. The implementation is a single-page web UI with a backend HTTP API. This README is language-agnostic and intended to help developers run, test, and deploy the project.

## Summary

- Purpose: Clean and transform text by detecting buzzwords and applying replacement rules, and provide analysis/exports.
- Interface: Web UI (Blazor WebAssembly in this implementation) + HTTP API.
- Diagnostics: Built-in diagnostics and health endpoints for monitoring dependencies.

## Live Application

- Live Demo: https://app-hdoqo3wpwuuja.azurewebsites.net
- Diagnostics: https://app-hdoqo3wpwuuja.azurewebsites.net/diag

(If these change, update appsettings and deployment as needed.)

## Features

- Advanced text analysis (tokenization, POS tagging, frequency).
- Replacement/sanitization suggestions and apply/remove actions.
- Export processed text (plain text; optional CSV with stats).
- Diagnostics page with dependency health checks.
- Small, focused API surface for automation and integration.

## Major Data & API Connections

- Replacement lists: stored as JSON resources in the repo and optionally mirrored to Table Storage.
- Table Storage (Azure Table Storage): used to store replacement rules / word lists (PoAppName[TableName] convention recommended).
- Blob Storage (Azure Blob Storage): optional for storing exported files.
- Health endpoints:
  - GET /healthz — overall system and per-dependency health
  - Diagnostics UI: /diag (reads health endpoints)
- Core API endpoints (see PRD.md for full spec):
  - POST /api/analyze — analyze text, returns tokens, stats, suggestions
  - POST /api/replace — apply selected replacement rules
  - POST /api/export — generate downloadable exports

## Project Structure

```
PoRemoveBad/
├── PoRemoveBad.Client/     # Blazor WebAssembly client (UI)
├── PoRemoveBad.Server/     # .NET Web API server
├── PoRemoveBad.Core/       # Shared business logic and models
├── infra/                  # Azure infrastructure (Bicep)
├── Diagrams/               # Mermaid source and generated SVGs (added)
├── PRD.md                  # Product Requirements Document (language-agnostic)
└── README.md               # This file
```

## Prerequisites

- .NET 9 SDK
- Node.js + npm (only required to regenerate diagrams with mermaid-cli)
- Azure CLI (for deployments)
- Azure Developer CLI (azd) (optional; used by this project's dev workflow)
- (Optional) Azurite for local Table/Blob Storage emulation

## Local Development

1. Clone the repository
   git clone https://github.com/punkouter25/PoRemoveBad.git
   cd PoRemoveBad

2. Restore and build
   dotnet restore

3. Run the server (API + serve client static assets)
   dotnet watch run --project PoRemoveBad.Server

   - The server runs on the configured ports in launchSettings (commonly https://localhost:5001 and http://localhost:5000)
   - The frontend is served as static assets by the server in the production build, but in dev mode the client can also be run with the Blazor tooling.

4. Use the UI
   - Open https://localhost:5001 (or the configured server URL)
   - Diagnostics: https://localhost:5001/diag

## Running Analysis via curl (examples)

Analyze text:
curl -X POST "https://localhost:5001/api/analyze" -H "Content-Type: application/json" -d "{\"text\":\"This is some example text with buzzword synergy.\"}"

Apply replacements:
curl -X POST "https://localhost:5001/api/replace" -H "Content-Type: application/json" -d "{\"text\":\"...\",\"apply\":true}"

Export:
curl -X POST "https://localhost:5001/api/export" -H "Content-Type: application/json" -d "{\"text\":\"...\",\"format\":\"txt\"}"

(Adjust host/port for local dev vs deployed environment.)

## Diagrams & PRD

- PRD.md (root) — language-agnostic product requirements and API contract.
- Diagrams/ — contains Mermaid (.mmd) source files and generated SVGs:
  - component.mmd / component.svg
  - domain-model.mmd / domain-model.svg
  - user-workflow.mmd / user-workflow.svg
  - feature-sequence.mmd / feature-sequence.svg
  - deployment-artifacts.mmd / deployment-artifacts.svg

Diagrams are intentionally language-agnostic so another assistant or toolchain can regenerate the project.

## Regenerating Diagrams (mermaid-cli)

Node + npm required. Commands to run from the repository root:

1. Initialize npm (if a package.json does not exist):
   npm init -y

2. Install mermaid CLI as a local dev dependency:
   npm install -D @mermaid-js/mermaid-cli

3. Convert .mmd -> .svg (examples):
   npx mmdc -i Diagrams/component.mmd -o Diagrams/component.svg
   npx mmdc -i Diagrams/domain-model.mmd -o Diagrams/domain-model.svg
   npx mmdc -i Diagrams/user-workflow.mmd -o Diagrams/user-workflow.svg
   npx mmdc -i Diagrams/feature-sequence.mmd -o Diagrams/feature-sequence.svg
   npx mmdc -i Diagrams/deployment-artifacts.mmd -o Diagrams/deployment-artifacts.svg

(This repository will include generated SVGs so that PRs and docs render diagrams without requiring the CLI.)

## Infrastructure & Deployment

- IaC: infra/main.bicep — templates to provision Azure resources (Resource Group, Storage Account with Table/Blob, App Service).
- Recommended deployment flow:
  - Provision infra with azd or az cli using the provided Bicep templates.
  - Configure App Service settings (storage connection strings) as environment variables.
  - CI/CD: GitHub Actions (project includes workflow configs) — builds, tests, and deploys to the App Service.

## Notes for Contributors

- Replacement lists are shipped as JSON resources (PoRemoveBad.Core/Resources/*.json); these can be migrated to Table Storage if desired.
- Health checks include a TableStorage health check; ensure connection strings are configured for production.
- Logging is file-based for local runs (log.txt) and should be configured for telemetry in production (Application Insights or other).

## License

MIT License
