using System.Net.Http;
using System.Text.RegularExpressions;

namespace PerryHomesTracker.Tests.Integration;

internal static class AntiforgeryFormPost
{
    public static async Task<HttpResponseMessage> PostWithTokenAsync(
        HttpClient client,
        string getFormPath,
        string postPath,
        IReadOnlyDictionary<string, string> fields)
    {
        using var get = await client.GetAsync(getFormPath);
        get.EnsureSuccessStatusCode();
        var html = await get.Content.ReadAsStringAsync();
        var token = ExtractVerificationToken(html, getFormPath);

        var form = new Dictionary<string, string>(fields)
        {
            ["__RequestVerificationToken"] = token
        };
        using var content = new FormUrlEncodedContent(form);
        return await client.PostAsync(postPath, content);
    }

    private static string ExtractVerificationToken(string html, string getFormPath)
    {
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            match = Regex.Match(
                html,
                @"value=""([^""]+)""[^>]*name=""__RequestVerificationToken""",
                RegexOptions.IgnoreCase);
        }

        if (!match.Success)
            throw new InvalidOperationException($"Antiforgery token not found in response from {getFormPath}.");

        return match.Groups[1].Value;
    }
}
