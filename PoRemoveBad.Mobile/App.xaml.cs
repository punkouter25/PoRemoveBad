using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace PoRemoveBad.Mobile;

public partial class App : Application
{
	public App(IServiceProvider services)
	{
		try
		{
			Debug.WriteLine("Starting App initialization...");
			InitializeComponent();
			Debug.WriteLine("InitializeComponent completed");

			try
			{
				Debug.WriteLine("Creating MainPage directly...");
				MainPage = services.GetRequiredService<MainPage>();
				Debug.WriteLine("MainPage created and set");
			}
			catch (Exception pageEx)
			{
				Debug.WriteLine($"Error creating MainPage: {pageEx.Message}");
				Debug.WriteLine($"StackTrace: {pageEx.StackTrace}");

				// Create a simple error page
				MainPage = new ContentPage
				{
					Content = new Label
					{
						Text = "Error: " + pageEx.Message,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				};
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Critical error in App constructor: {ex.Message}");
			Debug.WriteLine($"StackTrace: {ex.StackTrace}");
			
			MainPage = new ContentPage
			{
				Content = new Label
				{
					Text = "Critical Error: " + ex.Message,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				}
			};
		}
	}
}