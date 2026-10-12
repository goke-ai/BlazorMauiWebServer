namespace GokeApp.Services;

public static class ServiceHelper
{
    public static T GetService<T>() =>
        IPlatformApplication.Current!.Services.GetService<T>() ?? throw new InvalidOperationException($"Service of type {typeof(T)} not found.");
}
