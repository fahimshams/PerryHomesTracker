using System.Net.Http.Headers;

namespace PerryHomesMcp;

/// <summary>
/// Minimal GitHub REST client (same patterns as <c>PrReviewAgent</c>: Bearer token, User-Agent, timeouts).
/// </summary>
public sealed class GitHubApi
{
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromMinutes(2);

    private readonly string? _token;

    public GitHubApi()
    {
        _token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")?.Trim()
                 ?? Environment.GetEnvironmentVariable("GH_TOKEN")?.Trim();
    }

    public bool HasToken => !string.IsNullOrEmpty(_token);

    public HttpClient CreateClient()
    {
        if (!HasToken)
            throw new InvalidOperationException("GITHUB_TOKEN (or GH_TOKEN) is not set.");

        var client = new HttpClient { Timeout = HttpTimeout };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PerryHomesMcp/1.0");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token!);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");
        return client;
    }

    public string ResolveRepository(string? repository)
    {
        var r = repository?.Trim();
        if (!string.IsNullOrEmpty(r))
            return r;

        r = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")?.Trim();
        if (!string.IsNullOrEmpty(r))
            return r;

        throw new InvalidOperationException("Provide repository (owner/repo) or set GITHUB_REPOSITORY.");
    }

    public static (string Owner, string Repo) ParseRepository(string ownerSlashRepo)
    {
        var parts = ownerSlashRepo.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            throw new ArgumentException("Repository must be 'owner/repo'.", nameof(ownerSlashRepo));
        return (parts[0], parts[1]);
    }

    public async Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
