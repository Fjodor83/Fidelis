namespace Fidelity.Client.State;

public class AppState
{
    public event Action? OnChange;
    public bool IsLoading { get; private set; }

    public void SetLoading(bool loading)
    {
        IsLoading = loading;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
