namespace PrReviewAgent;

/// <summary>
/// Loads review criteria from <c>.cursor/rules/code-review-agent.mdc</c> (YAML front matter stripped).
/// Override path with env <c>REVIEW_RULES_PATH</c>.
/// </summary>
internal static class ReviewRules
{
    private const string RelativeRulesPath = ".cursor/rules/code-review-agent.mdc";

    internal static async Task<string> LoadSystemPromptAsync(CancellationToken ct)
    {
        var path = ResolvePath();
        if (path is null)
        {
            throw new FileNotFoundException(
                "Review rules not found. Set REVIEW_RULES_PATH to code-review-agent.mdc, or run from a directory under the repo root so "
                + RelativeRulesPath
                + " exists.");
        }

        var raw = await File.ReadAllTextAsync(path, ct);
        var body = StripYamlFrontMatter(raw).Trim();
        if (string.IsNullOrWhiteSpace(body))
            throw new InvalidOperationException("Review rules file has no content after front matter: " + path);

        return """
            You are reviewing a pull request unified diff for a .NET 8 ASP.NET Core MVC app (EF Core, SQL Server). Be concise and specific; cite file paths and what changed when you flag an issue. Use markdown with ### headings per theme.

            Apply the following project rules:

            """
            + body;
    }

    private static string? ResolvePath()
    {
        var env = Environment.GetEnvironmentVariable("REVIEW_RULES_PATH")?.Trim();
        if (!string.IsNullOrEmpty(env) && File.Exists(env))
            return Path.GetFullPath(env);

        foreach (var root in EnumerateCandidateRoots())
        {
            var candidate = Path.Combine(root, ".cursor", "rules", "code-review-agent.mdc");
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }

        return null;
    }

    private static IEnumerable<string> EnumerateCandidateRoots()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            if (string.IsNullOrEmpty(start))
                continue;

            var dir = Path.GetFullPath(start);
            while (!string.IsNullOrEmpty(dir))
            {
                if (seen.Add(dir))
                    yield return dir;

                var parent = Directory.GetParent(dir);
                if (parent is null)
                    break;
                dir = parent.FullName;
            }
        }
    }

    /// <summary>
    /// Strips leading YAML front matter delimited by <c>---</c> lines (Cursor <c>.mdc</c> format).
    /// </summary>
    private static string StripYamlFrontMatter(string raw)
    {
        using var reader = new StringReader(raw);
        var first = reader.ReadLine();
        if (first is null || first.Trim() != "---")
            return raw;

        while (reader.ReadLine() is { } line)
        {
            if (line.Trim() != "---")
                continue;

            var rest = reader.ReadToEnd();
            return rest.TrimStart('\r', '\n', ' ', '\t');
        }

        return raw;
    }
}
