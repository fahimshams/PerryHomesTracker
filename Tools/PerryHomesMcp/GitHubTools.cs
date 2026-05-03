using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace PerryHomesMcp;

[McpServerToolType]
public sealed class PerryHomesGitHubTools(GitHubApi github)
{
    private static readonly JsonSerializerOptions JsonWriteOptions = new() { WriteIndented = true };

    [McpServerTool(Name = "get_recent_deployments")]
    [Description("Returns recent GitHub Deployments for a repository (REST: GET /repos/{owner}/{repo}/deployments).")]
    public async Task<string> get_recent_deployments(
        [Description("Repository as owner/repo. Defaults to GITHUB_REPOSITORY.")] string? repository = null,
        [Description("Max deployments to return (1–30).")] int per_page = 10,
        CancellationToken cancellationToken = default)
    {
        if (!github.HasToken)
            return ErrorJson("GITHUB_TOKEN is not set.");

        try
        {
            var repo = github.ResolveRepository(repository);
            var (owner, name) = GitHubApi.ParseRepository(repo);
            per_page = Math.Clamp(per_page, 1, 30);
            var url =
                $"https://api.github.com/repos/{owner}/{name}/deployments?per_page={per_page}";
            var json = await github.GetStringAsync(url, cancellationToken);
            return FormatJson(json);
        }
        catch (Exception ex)
        {
            return ErrorJson(ex.Message);
        }
    }

    [McpServerTool(Name = "get_pr_summary")]
    [Description("Returns a JSON summary of one pull request (title, state, branches, author, merge info, changed files when available).")]
    public async Task<string> get_pr_summary(
        [Description("Pull request number.")] int pull_number,
        [Description("Repository as owner/repo. Defaults to GITHUB_REPOSITORY.")] string? repository = null,
        CancellationToken cancellationToken = default)
    {
        if (!github.HasToken)
            return ErrorJson("GITHUB_TOKEN is not set.");

        try
        {
            var repo = github.ResolveRepository(repository);
            var (owner, name) = GitHubApi.ParseRepository(repo);
            var url = $"https://api.github.com/repos/{owner}/{name}/pulls/{pull_number}";
            var json = await github.GetStringAsync(url, cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var summary = new
            {
                number = root.GetProperty("number").GetInt32(),
                title = root.TryGetProperty("title", out var t) ? t.GetString() : null,
                state = root.TryGetProperty("state", out var s) ? s.GetString() : null,
                draft = root.TryGetProperty("draft", out var d) && d.GetBoolean(),
                html_url = root.TryGetProperty("html_url", out var h) ? h.GetString() : null,
                user = root.TryGetProperty("user", out var u) && u.TryGetProperty("login", out var login)
                    ? login.GetString()
                    : null,
                head = root.TryGetProperty("head", out var head) && head.TryGetProperty("ref", out var href)
                    ? href.GetString()
                    : null,
                @base = root.TryGetProperty("base", out var b) && b.TryGetProperty("ref", out var bref)
                    ? bref.GetString()
                    : null,
                created_at = root.TryGetProperty("created_at", out var c) ? c.GetString() : null,
                updated_at = root.TryGetProperty("updated_at", out var up) ? up.GetString() : null,
                merged_at = root.TryGetProperty("merged_at", out var m) ? m.GetString() : null,
                mergeable_state = root.TryGetProperty("mergeable_state", out var ms) ? ms.GetString() : null,
                additions = root.TryGetProperty("additions", out var add) ? add.GetInt32() : (int?)null,
                deletions = root.TryGetProperty("deletions", out var del) ? del.GetInt32() : (int?)null,
                changed_files = root.TryGetProperty("changed_files", out var cf) ? cf.GetInt32() : (int?)null,
            };
            return JsonSerializer.Serialize(summary, JsonWriteOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson(ex.Message);
        }
    }

    [McpServerTool(Name = "get_pipeline_status")]
    [Description("Returns CI signal for a branch: latest commit SHA, combined commit status, and recent GitHub Actions workflow runs.")]
    public async Task<string> get_pipeline_status(
        [Description("Branch name (e.g. main, develop).")] string branch = "main",
        [Description("Repository as owner/repo. Defaults to GITHUB_REPOSITORY.")] string? repository = null,
        [Description("Max workflow runs to include.")] int per_page = 10,
        CancellationToken cancellationToken = default)
    {
        if (!github.HasToken)
            return ErrorJson("GITHUB_TOKEN is not set.");

        try
        {
            var repo = github.ResolveRepository(repository);
            var (owner, name) = GitHubApi.ParseRepository(repo);
            per_page = Math.Clamp(per_page, 1, 30);

            var branchUrl = $"https://api.github.com/repos/{owner}/{name}/branches/{Uri.EscapeDataString(branch)}";
            var branchJson = await github.GetStringAsync(branchUrl, cancellationToken);
            using var branchDoc = JsonDocument.Parse(branchJson);
            var sha = branchDoc.RootElement.GetProperty("commit").GetProperty("sha").GetString() ?? "";

            string? statusState = null;
            try
            {
                var statusUrl = $"https://api.github.com/repos/{owner}/{name}/commits/{sha}/status";
                var statusJson = await github.GetStringAsync(statusUrl, cancellationToken);
                using var st = JsonDocument.Parse(statusJson);
                statusState = st.RootElement.TryGetProperty("state", out var stState) ? stState.GetString() : null;
            }
            catch
            {
                // Status API may 404 if no checks; still return runs.
            }

            var runsUrl =
                $"https://api.github.com/repos/{owner}/{name}/actions/runs?branch={Uri.EscapeDataString(branch)}&per_page={per_page}";
            var runsJson = await github.GetStringAsync(runsUrl, cancellationToken);
            using var runsDoc = JsonDocument.Parse(runsJson);
            var workflowRuns = new List<object>();
            if (runsDoc.RootElement.TryGetProperty("workflow_runs", out var wr) && wr.ValueKind == JsonValueKind.Array)
            {
                foreach (var run in wr.EnumerateArray())
                {
                    workflowRuns.Add(new
                    {
                        id = run.TryGetProperty("id", out var id) ? id.GetInt64() : 0,
                        name = run.TryGetProperty("name", out var n) ? n.GetString() : null,
                        status = run.TryGetProperty("status", out var st) ? st.GetString() : null,
                        conclusion = run.TryGetProperty("conclusion", out var con) ? con.GetString() : null,
                        event_type = run.TryGetProperty("event", out var ev) ? ev.GetString() : null,
                        head_sha = run.TryGetProperty("head_sha", out var hs) ? hs.GetString() : null,
                        html_url = run.TryGetProperty("html_url", out var hu) ? hu.GetString() : null,
                        created_at = run.TryGetProperty("created_at", out var ca) ? ca.GetString() : null,
                        updated_at = run.TryGetProperty("updated_at", out var ua) ? ua.GetString() : null,
                    });
                }
            }

            var result = new
            {
                repository = repo,
                branch,
                head_sha = sha,
                combined_status = statusState,
                workflow_runs = workflowRuns,
            };

            return JsonSerializer.Serialize(result, JsonWriteOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson(ex.Message);
        }
    }

    [McpServerTool(Name = "get_open_prs")]
    [Description("Lists open pull requests for the repository (number, title, author, branches, updated_at).")]
    public async Task<string> get_open_prs(
        [Description("Repository as owner/repo. Defaults to GITHUB_REPOSITORY.")] string? repository = null,
        [Description("Max PRs to return (1–100).")] int per_page = 20,
        CancellationToken cancellationToken = default)
    {
        if (!github.HasToken)
            return ErrorJson("GITHUB_TOKEN is not set.");

        try
        {
            var repo = github.ResolveRepository(repository);
            var (owner, name) = GitHubApi.ParseRepository(repo);
            per_page = Math.Clamp(per_page, 1, 100);
            var url =
                $"https://api.github.com/repos/{owner}/{name}/pulls?state=open&sort=updated&direction=desc&per_page={per_page}";
            var json = await github.GetStringAsync(url, cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var list = new List<object>();
            foreach (var pr in doc.RootElement.EnumerateArray())
            {
                list.Add(new
                {
                    number = pr.GetProperty("number").GetInt32(),
                    title = pr.TryGetProperty("title", out var t) ? t.GetString() : null,
                    user = pr.TryGetProperty("user", out var u) && u.TryGetProperty("login", out var l) ? l.GetString() : null,
                    head = pr.TryGetProperty("head", out var h) && h.TryGetProperty("ref", out var href) ? href.GetString() : null,
                    @base = pr.TryGetProperty("base", out var b) && b.TryGetProperty("ref", out var bref) ? bref.GetString() : null,
                    draft = pr.TryGetProperty("draft", out var d) && d.GetBoolean(),
                    html_url = pr.TryGetProperty("html_url", out var hu) ? hu.GetString() : null,
                    updated_at = pr.TryGetProperty("updated_at", out var up) ? up.GetString() : null,
                });
            }

            return JsonSerializer.Serialize(new { repository = repo, count = list.Count, pull_requests = list }, JsonWriteOptions);
        }
        catch (Exception ex)
        {
            return ErrorJson(ex.Message);
        }
    }

    private static string ErrorJson(string message) =>
        JsonSerializer.Serialize(new { error = message }, JsonWriteOptions);

    private static string FormatJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, JsonWriteOptions);
        }
        catch
        {
            return json;
        }
    }
}
