# PoRemoveBad

## Overview
A photo vault application with multiple project types.

## Projects
- Core Library: PoRemoveBad.Core
- Web Application: PoRemoveBad.Web
- Mobile Application: PoRemoveBad.Mobile
- Azure Functions: PoRemoveBad.Functions
- Unit Tests: PoRemoveBad.Core.Tests, PoRemoveBad.Web.Tests

## Prerequisites
- Latest .NET SDK
- Visual Studio 2022
- Azure Functions Core Tools

## Core Library (PoRemoveBad.Core)
The core library provides the main functionality for text processing and word replacement. It includes services for text processing, exporting processed text, and models for representing text statistics and word replacements.

### Usage Example
```csharp
using PoRemoveBad.Core.Services;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
    .AddCoreServices()
    .BuildServiceProvider();

var textProcessingService = serviceProvider.GetRequiredService<ITextProcessingService>();
await textProcessingService.InitializeDictionaryAsync();

var inputText = "This is a bad example.";
var result = await textProcessingService.ProcessTextAsync(inputText);

Console.WriteLine("Processed Text: " + result.ProcessedText);
Console.WriteLine("Replaced Words Count: " + result.Statistics.ReplacedWordsCount);
```

## Web Application (PoRemoveBad.Web)
The web application is built using Blazor WebAssembly and provides a user interface for text processing and word replacement. It allows users to input text, process it, and view the results.

### Usage Example
1. Clone the repository.
2. Navigate to the `PoRemoveBad.Web` directory.
3. Run the following command to start the web application:
   ```bash
   dotnet run
   ```
4. Open a web browser and navigate to `https://localhost:5001`.

## Mobile Application (PoRemoveBad.Mobile)
The mobile application is built using .NET MAUI and provides a user interface for text processing and word replacement on mobile devices. It allows users to input text, process it, and view the results.

### Usage Example
1. Clone the repository.
2. Open the `PoRemoveBad.Mobile` project in Visual Studio 2022.
3. Set the startup project to `PoRemoveBad.Mobile`.
4. Select the target platform (Android, iOS, etc.) and run the application.

## Running Unit Tests
The repository includes unit tests for the core library and web application. To run the unit tests, follow these steps:

1. Navigate to the test project directory (e.g., `PoRemoveBad.Core.Tests` or `PoRemoveBad.Web.Tests`).
2. Run the following command to execute the tests:
   ```bash
   dotnet test
   ```

## Sample Data
The repository includes sample data for word replacements in the `PoRemoveBad.Core/Resources` directory. The sample data is used by the text processing service to replace inappropriate words with suitable alternatives.
