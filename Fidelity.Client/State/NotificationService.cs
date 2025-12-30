namespace Fidelity.Client.State;

public class NotificationService
{
    public event Action<string, string>? OnShow; // Message, Type (success, error)

    public void ShowSuccess(string message)
    {
        OnShow?.Invoke(message, "success");
    }

    public void ShowError(string message)
    {
        OnShow?.Invoke(message, "error");
    }
}
