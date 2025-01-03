﻿using System.Diagnostics;
using PoRemoveBad.Core.Models;
using PoRemoveBad.Core.Services;
using PoRemoveBad.Mobile.ViewModels;

namespace PoRemoveBad.Mobile;

/// <summary>
/// Represents the main page of the mobile application.
/// </summary>
public partial class MainPage : ContentPage
{
	private readonly ITextProcessingService _textProcessingService;
	private readonly IExportService _exportService;
	private readonly MainPageViewModel _viewModel;
	private string _processedText;

	/// <summary>
	/// Initializes a new instance of the <see cref="MainPage"/> class.
	/// </summary>
	/// <param name="textProcessingService">The text processing service.</param>
	/// <param name="exportService">The export service.</param>
	public MainPage(ITextProcessingService textProcessingService, IExportService exportService)
	{
		try
		{
			Debug.WriteLine("Starting MainPage initialization...");
			_textProcessingService = textProcessingService;
			_exportService = exportService;
			_viewModel = new MainPageViewModel();

			Debug.WriteLine("Initializing MainPage components...");
			InitializeComponent();
			Debug.WriteLine("MainPage components initialized");

			BindingContext = _viewModel;
			Debug.WriteLine("BindingContext set");

			// Initialize dictionary
			InitializeAsync();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error in MainPage constructor: {ex.Message}");
			Debug.WriteLine($"StackTrace: {ex.StackTrace}");
			throw;
		}
	}

	/// <summary>
	/// Initializes the text processing dictionary asynchronously.
	/// </summary>
	private async void InitializeAsync()
	{
		try
		{
			await _textProcessingService.InitializeDictionaryAsync();
			await MainThread.InvokeOnMainThreadAsync(() =>
				DisplayAlert("Success", "Text sanitizer is ready to use!", "OK"));
		}
		catch (Exception ex)
		{
			await MainThread.InvokeOnMainThreadAsync(() =>
				DisplayAlert("Error", "Failed to initialize text sanitizer: " + ex.Message, "OK"));
		}
	}

	/// <summary>
	/// Handles the input text changed event.
	/// </summary>
	/// <param name="sender">The event sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnInputTextChanged(object sender, TextChangedEventArgs e)
	{
		ProcessButton.IsEnabled = !string.IsNullOrWhiteSpace(e.NewTextValue);
		if (string.IsNullOrWhiteSpace(e.NewTextValue))
		{
			_viewModel.Reset();
			_processedText = null;
			UpdateOutputDisplay("");
		}
	}

	/// <summary>
	/// Updates the output display with the specified text.
	/// </summary>
	/// <param name="text">The text to display.</param>
	private void UpdateOutputDisplay(string text)
	{
		var htmlContent = $@"
			<!DOCTYPE html>
			<html>
			<head>
				<meta charset='utf-8'>
				<meta name='viewport' content='width=device-width, initial-scale=1'>
				<style>
					body {{
						font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
						padding: 10px;
						margin: 0;
						line-height: 1.5;
						font-size: 16px;
						word-wrap: break-word;
					}}
					mark {{
						background-color: yellow;
						border-radius: 3px;
						padding: 0 2px;
					}}
				</style>
			</head>
			<body>
				{text}
			</body>
			</html>";

		MainThread.BeginInvokeOnMainThread(() =>
		{
			OutputWebView.Source = new HtmlWebViewSource { Html = htmlContent };
		});
	}

	/// <summary>
	/// Handles the process button click event.
	/// </summary>
	/// <param name="sender">The event sender.</param>
	/// <param name="e">The event arguments.</param>
	private async void OnProcessClicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(InputEditor.Text)) return;

		try
		{
			ProcessButton.IsEnabled = false;
			Debug.WriteLine($"Processing text: {InputEditor.Text}");

			var progress = new Progress<double>();
			var result = await _textProcessingService.ProcessTextAsync(InputEditor.Text, progress);
			
			Debug.WriteLine($"Processed text: {result.ProcessedText}");
			Debug.WriteLine($"Replaced words count: {result.Statistics.ReplacedWordsCount}");

			_processedText = result.ProcessedText;
			UpdateOutputDisplay(_processedText);
			
			_viewModel.HasProcessedText = true;
			_viewModel.UpdateStatistics(result.Statistics);

			ExportTxtButton.IsEnabled = true;
			ExportHtmlButton.IsEnabled = true;
			ExportJsonButton.IsEnabled = true;

			if (result.Statistics.ReplacedWordsCount > 0)
			{
				await DisplayAlert("Success", 
					$"Successfully replaced {result.Statistics.ReplacedWordsCount} inappropriate words.", 
					"OK");
			}
			else
			{
				await DisplayAlert("Info", "No inappropriate words found in the text.", "OK");
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error processing text: {ex.Message}");
			Debug.WriteLine($"StackTrace: {ex.StackTrace}");
			await DisplayAlert("Error", "Failed to process text: " + ex.Message, "OK");
		}
		finally
		{
			ProcessButton.IsEnabled = true;
		}
	}

	/// <summary>
	/// Handles the export to TXT button click event.
	/// </summary>
	/// <param name="sender">The event sender.</param>
	/// <param name="e">The event arguments.</param>
	private async void OnExportTxtClicked(object sender, EventArgs e)
	{
		await ExportFile("txt");
	}

	/// <summary>
	/// Handles the export to HTML button click event.
	/// </summary>
	/// <param name="sender">The event sender.</param>
	/// <param name="e">The event arguments.</param>
	private async void OnExportHtmlClicked(object sender, EventArgs e)
	{
		await ExportFile("html");
	}

	/// <summary>
	/// Handles the export to JSON button click event.
	/// </summary>
	/// <param name="sender">The event sender.</param>
	/// <param name="e">The event arguments.</param>
	private async void OnExportJsonClicked(object sender, EventArgs e)
	{
		await ExportFile("json");
	}

	/// <summary>
	/// Exports the processed text to a file in the specified format.
	/// </summary>
	/// <param name="format">The file format.</param>
	private async Task ExportFile(string format)
	{
		if (string.IsNullOrEmpty(_processedText)) return;

		try
		{
			var statistics = new TextStatistics
			{
				TotalWords = _viewModel.TotalWords,
				TotalCharacters = _viewModel.TotalCharacters,
				ReplacedWordsCount = _viewModel.ReplacedWordsCount,
				SentenceCount = _viewModel.SentenceCount,
				ParagraphCount = _viewModel.ParagraphCount
			};

			var fileBytes = await _exportService.ExportToFileAsync(_processedText, statistics, format);
			var fileName = $"cleaned_text_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";

			var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
			await File.WriteAllBytesAsync(filePath, fileBytes);

			await Share.RequestAsync(new ShareFileRequest
			{
				Title = $"Share {format.ToUpper()} File",
				File = new ShareFile(filePath)
			});
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error exporting file: {ex.Message}");
			Debug.WriteLine($"StackTrace: {ex.StackTrace}");
			await DisplayAlert("Error", $"Failed to export text as {format.ToUpper()}: " + ex.Message, "OK");
		}
	}
}
