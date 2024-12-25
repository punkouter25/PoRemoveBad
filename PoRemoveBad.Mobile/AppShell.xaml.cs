using System.Diagnostics;

namespace PoRemoveBad.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		try
		{
			Debug.WriteLine("Starting AppShell initialization...");
			InitializeComponent();
			Debug.WriteLine("AppShell initialization completed");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error in AppShell initialization: {ex.Message}");
			Debug.WriteLine($"StackTrace: {ex.StackTrace}");
			throw;
		}
	}
}
