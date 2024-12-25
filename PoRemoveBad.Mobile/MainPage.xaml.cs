using PoRemoveBad.Core.Services;
using PoRemoveBad.Mobile.ViewModels;

namespace PoRemoveBad.Mobile;

public partial class MainPage : ContentPage
{
	private readonly ITextProcessingService _textProcessingService;
	private readonly IExportService _exportService;
	private readonly MainPageViewModel _viewModel;
	private string _processedText;

	public MainPage(ITextProcessingService textProcessingService, IExportService exportService)
	{
		InitializeComponent();
		_textProcessingService = textProcessingService;
		_exportService = exportService;
		_viewModel = new MainPageViewModel();
		BindingContext = _viewModel;

		InitializeAsync();
	}

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

	private void OnInputTextChanged(object sender, TextChangedEventArgs e)
	{
		ProcessButton.IsEnabled = !string.IsNullOrWhiteSpace(e.NewTextValue);
		if (string.IsNullOrWhiteSpace(e.NewTextValue))
		{
			_viewModel.Reset();
			_processedText = null;
		}
	}

	private async void OnProcessClicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(InputEditor.Text)) return;

		try
		{
			ProcessButton.IsEnabled = false;
			var progress = new Progress<double>();
			var result = await _textProcessingService.ProcessTextAsync(InputEditor.Text, progress);
			
			_processedText = result.ProcessedText;
			OutputEditor.Text = _processedText;
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
			await DisplayAlert("Error", "Failed to process text: " + ex.Message, "OK");
		}
		finally
		{
			ProcessButton.IsEnabled = true;
		}
	}

	private async void OnExportTxtClicked(object sender, EventArgs e)
	{
		await ExportFile("txt");
	}

	private async void OnExportHtmlClicked(object sender, EventArgs e)
	{
		await ExportFile("html");
	}

	private async void OnExportJsonClicked(object sender, EventArgs e)
	{
		await ExportFile("json");
	}

	private async Task ExportFile(string format)
	{
		if (string.IsNullOrEmpty(_processedText)) return;

		try
		{
			var fileBytes = await _exportService.ExportToFileAsync(_processedText, _viewModel, format);
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
			await DisplayAlert("Error", $"Failed to export text as {format.ToUpper()}: " + ex.Message, "OK");
		}
	}
}

