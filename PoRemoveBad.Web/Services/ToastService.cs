using System.Timers;
using Timer = System.Timers.Timer;

namespace PoRemoveBad.Web.Services;

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public ToastLevel Level { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool AutoClose { get; set; } = true;
    public int Timeout { get; set; } = 5000;
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}

public class ToastService : IDisposable
{
    public event Action<ToastMessage>? OnShow;
    public event Action<Guid>? OnHide;
    private readonly Timer _timer;
    private readonly List<ToastMessage> _activeToasts = new();

    public ToastService()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += CheckToasts;
        _timer.Start();
    }

    public void ShowToast(string message, ToastLevel level = ToastLevel.Info, string title = "", bool autoClose = true, int timeout = 5000)
    {
        var toast = new ToastMessage
        {
            Level = level,
            Message = message,
            Title = title,
            AutoClose = autoClose,
            Timeout = timeout
        };

        _activeToasts.Add(toast);
        OnShow?.Invoke(toast);
    }

    public void ShowInfo(string message, string title = "") =>
        ShowToast(message, ToastLevel.Info, title);

    public void ShowSuccess(string message, string title = "") =>
        ShowToast(message, ToastLevel.Success, title);

    public void ShowWarning(string message, string title = "") =>
        ShowToast(message, ToastLevel.Warning, title);

    public void ShowError(string message, string title = "") =>
        ShowToast(message, ToastLevel.Error, title);

    public void HideToast(Guid toastId)
    {
        var toast = _activeToasts.FirstOrDefault(t => t.Id == toastId);
        if (toast != null)
        {
            _activeToasts.Remove(toast);
            OnHide?.Invoke(toastId);
        }
    }

    private void CheckToasts(object? sender, ElapsedEventArgs e)
    {
        var now = DateTime.Now;
        var toastsToRemove = _activeToasts
            .Where(t => t.AutoClose && (now - t.TimeStamp).TotalMilliseconds >= t.Timeout)
            .ToList();

        foreach (var toast in toastsToRemove)
        {
            _activeToasts.Remove(toast);
            OnHide?.Invoke(toast.Id);
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
} 