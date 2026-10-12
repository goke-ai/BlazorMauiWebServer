namespace GokeApp.Pages.Controls;

internal sealed class MapWebViewHost
{
    private readonly WebView _webView;
    private bool _isMapHtmlLoaded;
    private bool _isMapPageReady;

    public MapWebViewHost(WebView webView, Func<Task> onNavigatedAsync)
    {
        _webView = webView;
        _webView.Navigated += async (_, _) =>
        {
            _isMapPageReady = true;
            await onNavigatedAsync();
        };
    }

    public bool IsMapHtmlLoaded => _isMapHtmlLoaded;

    public bool IsMapPageReady => _isMapPageReady;

    public async Task LoadHtmlAsync(string html)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _isMapPageReady = false;
            _isMapHtmlLoaded = true;
            _webView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        });
    }

    public async Task InvokeWhenReadyAsync(string script)
    {
        if (!_isMapHtmlLoaded || !_isMapPageReady)
        {
            return;
        }

        for (var attempt = 0; attempt < 5; attempt += 1)
        {
            var result = await MainThread.InvokeOnMainThreadAsync(
                () => _webView.EvaluateJavaScriptAsync(script));

            if (string.Equals(result, "ready", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(result, "\"ready\"", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await Task.Delay(100);
        }
    }
}
