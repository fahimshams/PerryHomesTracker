using System.Net;
using System.Text.Json;

namespace PerryHomesTracker.Tests.Smoke;

public class HomesAndStagesSmokeTests
{
    private static string? ResolveBaseUrl()
    {
        var env = Environment.GetEnvironmentVariable("SMOKE_TEST_BASE_URL");
        if (!string.IsNullOrWhiteSpace(env))
            return env.TrimEnd('/');

        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.Smoke.json");
        if (!File.Exists(path))
            return null;

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (!doc.RootElement.TryGetProperty("Smoke", out var smoke))
            return null;
        if (!smoke.TryGetProperty("BaseUrl", out var baseUrlEl))
            return null;
        var fromFile = baseUrlEl.GetString();
        return string.IsNullOrWhiteSpace(fromFile) ? null : fromFile.TrimEnd('/');
    }

    [SkippableFact]
    public async Task Get_Homes_Index_Returns200()
    {
        var baseUrl = ResolveBaseUrl();
        Skip.If(string.IsNullOrEmpty(baseUrl), "Set Smoke:BaseUrl in appsettings.Smoke.json or SMOKE_TEST_BASE_URL.");

        using var client = new HttpClient { BaseAddress = new Uri(baseUrl + "/") };
        var response = await client.GetAsync("Homes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [SkippableFact]
    public async Task Get_Stages_Index_Returns200()
    {
        var baseUrl = ResolveBaseUrl();
        Skip.If(string.IsNullOrEmpty(baseUrl), "Set Smoke:BaseUrl in appsettings.Smoke.json or SMOKE_TEST_BASE_URL.");

        using var client = new HttpClient { BaseAddress = new Uri(baseUrl + "/") };
        var response = await client.GetAsync("Stages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
