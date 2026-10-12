using System.Collections.Concurrent;
using System.Text;

namespace GokeApp.Pages.Controls;

internal static class MapWebViewAssetCache
{
    private const string MapCoreModulePlaceholder = "__MAP_CORE_MODULE_URL__";

    private static readonly ConcurrentDictionary<string, Lazy<Task<string>>> RawAssetCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, Lazy<Task<string>>> ModuleUrlCache = new(StringComparer.OrdinalIgnoreCase);

    public static Task<string> GetTemplateHtmlAsync(string templateFileName)
    {
        return GetRawAssetAsync(templateFileName);
    }

    public static Task<string> GetModuleUrlAsync(string mapCoreScriptFileName, string mapScriptFileName)
    {
        var cacheKey = $"{mapCoreScriptFileName}|{mapScriptFileName}";

        return ModuleUrlCache.GetOrAdd(
            cacheKey,
            _ => new Lazy<Task<string>>(() => BuildModuleUrlAsync(mapCoreScriptFileName, mapScriptFileName), LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;
    }

    private static async Task<string> BuildModuleUrlAsync(string mapCoreScriptFileName, string mapScriptFileName)
    {
        var mapCoreModuleUrl = CreateJavaScriptModuleUrl(await GetRawAssetAsync(mapCoreScriptFileName));
        var mapScript = (await GetRawAssetAsync(mapScriptFileName))
            .Replace(MapCoreModulePlaceholder, mapCoreModuleUrl, StringComparison.Ordinal);

        return CreateJavaScriptModuleUrl(mapScript);
    }

    private static Task<string> GetRawAssetAsync(string fileName)
    {
        return RawAssetCache.GetOrAdd(
            fileName,
            static key => new Lazy<Task<string>>(() => ReadAssetAsync(key), LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;
    }

    private static async Task<string> ReadAssetAsync(string fileName)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }

    private static string CreateJavaScriptModuleUrl(string script)
    {
        var bytes = Encoding.UTF8.GetBytes(script);
        return $"data:text/javascript;base64,{Convert.ToBase64String(bytes)}";
    }
}
