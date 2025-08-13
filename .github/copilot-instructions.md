CODING RULES

1. Core Philosophy
Modern Workflow: All applications will be built with .NET 9. The structure will consist of a Blazor WebAssembly frontend hosted by a .NET API.
Architectural Flexibility: The backend will blend concepts from Onion Architecture and Vertical Slice Architecture or simple UI with services, choose the best approach for the specific needs of the app depending on its complexities
Automation First: Use the command line (dotnet, az, gh) for all possible actions to minimize manual configuration.
Code Quality: Adhere to SOLID principles and apply Gang of Four (GoF) design patterns where appropriate. Code should be self-documenting, with comments reserved for explaining complex algorithms or key design choices.
Strict File Size: Propose refactoring for any .cs or .razor file, including auto-generated ones, that exceeds 500 lines.
Proactive Cleanup: Before removing any files, code, or project references, provide a list of the items to be removed with a clear justification and wait for approval.
2. Project Setup & Structure
Naming Convention: The solution will be named PoAppName, and all associated projects must be prefixed with Po. (e.g., PoAppName.Api).
Hybrid Architecture: The initial project scaffolding will be created via a dotnet new commands, combining the best ideas from both the ardalis/clean-architecture and fullstackhero/dotnet-starter-kit templates.
Use any other popular free apis and libraries for .NET that will make the app better than without it
Local Development: The environment will be configured to launch and debug only the PoAppName.Api project at https://localhost:5001 and http://localhost:5000.
Use dotnet watch so I can make code change without restarting the app 
Create Readme.md file if one doesnt exist that gives a description of the app and the UI and how to use it
3. Backend & API Design
Store all keys in appsettings.development.json / I will make github project private so it is ok to store them there
Global Exception Handling: Implement middleware in the API for centralized exception handling. It must use Serilog to log complete exception details and return a standardized Problem Details (RFC 7807) response.
API Documentation: Configure Swagger/OpenAPI support from the start
Make it easy to simulate user interaction through the calling of api methods with curl so I don’t need to manually test through the web UI
4. Frontend Architecture (Blazor/Javscript)
UI Components: Begin with built-in Blazor components. For complex UI elements like data grids or charts, Radzen.Blazor is the default library of choice.
Diagnostics Page: Every application must include a diagnostics page at the /diag route. This page will display the connection status of critical dependencies by fetching data from a /healthz API endpoint, which will be implemented using .NET's Health Check services.
Connect to popular .js and .css libraries using CDN / do not store locally
Use javascript libraries as needed for 3d / physics / collision etc.
5. Data & Persistence
Database: The primary data store will be Azure Table Storage. Local development will be done against the Azurite emulator.
Data Access SDK: Interaction with Table Storage will be implemented using the Azure.Data.Tables client library, which is the current standard for .NET applications.[1][2]
Abstraction: Data access will be abstracted using the Repository Pattern, implemented within the Infrastructure project.
Table Naming: Azure Storage Tables must be named using the convention PoAppName[TableName] (e.g., PoAppNameProducts).
Get artifacts like graphics files, texture files, sounds etc. form the internet automatically so I don’t need to manually get them and add theme to the project
6. Testing Strategy
Frameworks: Use xUnit for test execution
Use dotnet watch so code changes can be made without restarting web server / automate sending browser console data back to the coding llm to avoid manual copy and pasting
Project Structure: Separate tests into three distinct projects: PoAppName.UnitTests, PoAppName.IntegrationTests, and PoAppName.FunctionalTests.
Functional Test Scope: The FunctionalTests project is specifically for testing the API endpoints by making HTTP requests and verifying the responses to simulate actions the user would do in the app
Test-First Workflow:
Propose changes to the Domain and Application layers and await approval.
Write the Application layer services/handlers.
Immediately write Integration Tests for the happy path, validation failures, and edge cases.
After tests pass, implement the API endpoint and the Blazor UI.
7. DevOps & Logging
Secrets Management: Use appsettings.development.json for local secrets and Azure App Service Application Settings or Key Vault for deployed environments.
Logging: Implement Serilog with two sinks:
Console Sink: For real-time development feedback.
File Sink: Logs at Verbose level to src/PoAppName.Api/log.txt. This file must be overwritten on each application run to provide a clean log of the last session for analysis.
8. Azure Deployment
Use AZD CLI to deploy app to Azure when the app is verified to run locally
When deployed to azure the keys should be stored in the app service env variable or the appsettings.json / Do not use key vault
Create the resource group in Azure for the app / The resource group name should be the same as the .sln name
Only deploy the APP SERVICE use AZD to the new resource group / Do not deploy anything there except that app service / Other resources already exist in PoShared resource group / Do not deploy app service plan (use the app service plan in resource group PoShared)


