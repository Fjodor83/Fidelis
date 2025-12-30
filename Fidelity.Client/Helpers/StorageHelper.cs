using Blazored.LocalStorage;

namespace Fidelity.Client.Helpers;

public class StorageHelper
{
    private readonly ILocalStorageService _localStorage;
    private const string REF_TOKEN_KEY = "refreshToken";

    public StorageHelper(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SaveRefreshTokenAsync(string token)
    {
        await _localStorage.SetItemAsync(REF_TOKEN_KEY, token);
    }
}
