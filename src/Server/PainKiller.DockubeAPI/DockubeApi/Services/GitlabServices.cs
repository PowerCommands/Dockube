using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DockubeApi.Contracts;

namespace DockubeApi.Services;

public class GitlabService : IGitlabService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public GitlabService(string baseUrl, string personalAccessToken)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken);
    }
    public async Task<string?> CreateProjectAsync(string projectName, int? namespaceId = null)
    {
        var payload = new Dictionary<string, object> { ["name"] = projectName };
        if (namespaceId.HasValue) payload["namespace_id"] = namespaceId.Value;

        var json = JsonSerializer.Serialize(payload);
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v4/projects", new StringContent(json, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("id").GetRawText();
    }
    public async Task<bool> CreateOrUpdateFileAsync(
        string projectId, string filePath, string branch, string content, string commitMessage)
    {
        var encodedPath = Uri.EscapeDataString(filePath);
        var url = $"{_baseUrl}/api/v4/projects/{projectId}/repository/files/{encodedPath}";

        var payload = new { branch, content, commit_message = commitMessage };

        var json = JsonSerializer.Serialize(payload);
        var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        if (response.IsSuccessStatusCode) return true;

        if ((int)response.StatusCode == 400 || (int)response.StatusCode == 409)
        {
            response = await _httpClient.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            return response.IsSuccessStatusCode;
        }
        return false;
    }
    public async Task<bool> DeleteFileAsync(string projectId, string filePath, string branch, string commitMessage)
    {
        var encodedPath = Uri.EscapeDataString(filePath);
        var url = $"{_baseUrl}/api/v4/projects/{projectId}/repository/files/{encodedPath}?branch={branch}&commit_message={Uri.EscapeDataString(commitMessage)}";
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }
    public async Task<string?> CreateUserAsync(string name, string username, string email, string password)
    {
        var payload = new { name, username, email, password, skip_confirmation = true, confirmed = true };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/v4/users", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to create user: {response.StatusCode} - {errorText}");
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("id").GetRawText();
    }
}