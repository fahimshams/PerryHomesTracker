using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PerryHomesTracker.Tests.Helpers;

internal sealed class MemoryTempDataProvider : ITempDataProvider
{
    private readonly Dictionary<string, object> _store = new(StringComparer.OrdinalIgnoreCase);

    public IDictionary<string, object> LoadTempData(HttpContext context) =>
        new Dictionary<string, object>(_store, StringComparer.OrdinalIgnoreCase);

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
        _store.Clear();
        foreach (var kv in values)
            _store[kv.Key] = kv.Value;
    }
}
