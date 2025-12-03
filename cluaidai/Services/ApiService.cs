using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using cluaidai.Models;

namespace cluaidai.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private readonly string _baseUrl;

    public ApiService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _baseUrl = "https://localhost:5001/api"; // Configure this
    }

    private async Task<HttpClient> GetAuthenticatedClient()
    {
        var token = await _authService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return _httpClient;
    }

    // Auth
    public async Task<AuthResponse?> RegisterAsync(string email, string password, string displayName)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/auth/register", new
        {
            email,
            password,
            displayName
        });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        return null;
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/auth/login", new
        {
            email,
            password
        });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        return null;
    }

    // Posts
    public async Task<List<Post>?> GetFeedAsync(int page = 0, int size = 10)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}/posts/feed?page={page}&size={size}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Post>>();
        }

        return null;
    }

    public async Task<Post?> CreatePostAsync(string caption, string? imageUrl)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.PostAsJsonAsync($"{_baseUrl}/posts", new
        {
            caption,
            imageUrl
        });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Post>();
        }

        return null;
    }

    public async Task<bool> LikePostAsync(Guid postId)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.PostAsync($"{_baseUrl}/posts/{postId}/like", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnlikePostAsync(Guid postId)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"{_baseUrl}/posts/{postId}/like");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Comment>?> GetCommentsAsync(Guid postId)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}/posts/{postId}/comments");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Comment>>();
        }

        return null;
    }

    public async Task<Comment?> CreateCommentAsync(Guid postId, string text)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.PostAsJsonAsync($"{_baseUrl}/posts/{postId}/comments", new { text });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Comment>();
        }

        return null;
    }

    // Users
    public async Task<User?> GetCurrentUserAsync()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}/users/me");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<User>();
        }

        return null;
    }

    public async Task<User?> GetUserAsync(string userId)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}/users/{userId}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<User>();
        }

        return null;
    }

    public async Task<bool> FollowUserAsync(string userId)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.PostAsync($"{_baseUrl}/users/{userId}/follow", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UnfollowUserAsync(string userId)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.DeleteAsync($"{_baseUrl}/users/{userId}/follow");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<User>?> SearchUsersAsync(string query)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}/users/search?q={Uri.EscapeDataString(query)}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<User>>();
        }

        return null;
    }

    // Messages
    public async Task<List<Conversation>?> GetConversationsAsync()
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}/messages/conversations");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Conversation>>();
        }

        return null;
    }

    public async Task<List<Message>?> GetConversationAsync(string otherUserId, int page = 0, int size = 50)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}/messages/conversation/{otherUserId}?page={page}&size={size}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Message>>();
        }

        return null;
    }

    // Notifications
    public async Task<List<Notification>?> GetNotificationsAsync(int page = 0, int size = 20)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.GetAsync($"{_baseUrl}/notifications?page={page}&size={size}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Notification>>();
        }

        return null;
    }

    // Uploads
    public async Task<(string UploadUrl, string PublicUrl)?> GetSignedUploadUrlAsync(string fileName, string contentType)
    {
        var client = await GetAuthenticatedClient();
        var response = await client.PostAsJsonAsync($"{_baseUrl}/uploads/signed-url", new
        {
            fileName,
            contentType
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<SignedUrlResponse>();
            if (result != null)
            {
                return (result.UploadUrl, result.PublicUrl);
            }
        }

        return null;
    }

    public async Task<string?> UploadImageDirectAsync(Stream imageStream, string fileName, string contentType)
    {
        var client = await GetAuthenticatedClient();
        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(imageStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await client.PostAsync($"{_baseUrl}/uploads/direct", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
            return result?.Url;
        }

        return null;
    }

    public async Task<string?> UploadImageAsync(byte[] imageBytes, string fileName)
    {
        using var stream = new MemoryStream(imageBytes);
        var contentType = GetContentType(fileName);
        return await UploadImageDirectAsync(stream, fileName, contentType);
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    public async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
    }
}

public class AuthResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class SignedUrlResponse
{
    public string UploadUrl { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
}

public class UploadResponse
{
    public string Url { get; set; } = string.Empty;
}
