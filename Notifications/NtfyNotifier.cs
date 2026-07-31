using DAProjectChecker.Configurations;
using Microsoft.Extensions.Options;

namespace DAProjectChecker.Notifications;

public class NtfyNotifier
{
    private readonly HttpClient _httpClient;
    private readonly NtfyOptions _options;

    public NtfyNotifier(
        IOptions<NtfyOptions> options)
    {
        _options = options.Value;

        _httpClient = new HttpClient();
    }

    public async Task SendAsync(
        string title,
        string message)
    {
        var url =
            $"{_options.Server.TrimEnd('/')}/{_options.Topic}";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Add("Title", title);

        request.Content = new StringContent(message);

        using var response =
            await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }
}