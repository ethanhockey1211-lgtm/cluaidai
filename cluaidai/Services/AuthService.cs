namespace cluaidai.Services;

public class AuthService
{
    private string? _accessToken;
    private string? _refreshToken;
    private string? _userId;
    private DateTime _expiresAt;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && _expiresAt > DateTime.UtcNow;
    public string? UserId => _userId;

    public Task<string?> GetAccessTokenAsync()
    {
        if (IsAuthenticated)
        {
            return Task.FromResult<string?>(_accessToken);
        }
        return Task.FromResult<string?>(null);
    }

    public Task SetAuthenticationAsync(AuthResponse authResponse)
    {
        _accessToken = authResponse.AccessToken;
        _refreshToken = authResponse.RefreshToken;
        _userId = authResponse.UserId;
        _expiresAt = authResponse.ExpiresAt;

        // In production, store securely using SecureStorage
        SecureStorage.SetAsync("access_token", _accessToken);
        SecureStorage.SetAsync("refresh_token", _refreshToken);
        SecureStorage.SetAsync("user_id", _userId);
        SecureStorage.SetAsync("expires_at", _expiresAt.ToString("O"));

        return Task.CompletedTask;
    }

    public async Task<bool> LoadStoredAuthenticationAsync()
    {
        try
        {
            _accessToken = await SecureStorage.GetAsync("access_token");
            _refreshToken = await SecureStorage.GetAsync("refresh_token");
            _userId = await SecureStorage.GetAsync("user_id");
            var expiresAtStr = await SecureStorage.GetAsync("expires_at");

            if (!string.IsNullOrEmpty(expiresAtStr) && DateTime.TryParse(expiresAtStr, out var expiresAt))
            {
                _expiresAt = expiresAt;
                return IsAuthenticated;
            }
        }
        catch
        {
            // Handle exceptions
        }

        return false;
    }

    public async Task LogoutAsync()
    {
        _accessToken = null;
        _refreshToken = null;
        _userId = null;
        _expiresAt = DateTime.MinValue;

        SecureStorage.Remove("access_token");
        SecureStorage.Remove("refresh_token");
        SecureStorage.Remove("user_id");
        SecureStorage.Remove("expires_at");

        await Task.CompletedTask;
    }
}
