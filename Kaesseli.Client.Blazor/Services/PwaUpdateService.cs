using Microsoft.JSInterop;

namespace Kaesseli.Client.Blazor.Services;

public sealed class PwaUpdateService(IJSRuntime js) : IAsyncDisposable
{
    private DotNetObjectReference<PwaUpdateService>? _ref;

    public event Func<Task>? UpdateAvailable;

    public async Task InitializeAsync()
    {
        if (_ref is not null)
            return;
        _ref = DotNetObjectReference.Create(this);
        await js.InvokeVoidAsync("kaesseliPwa.init", _ref);
    }

    [JSInvokable]
    public Task OnUpdateAvailable() => UpdateAvailable?.Invoke() ?? Task.CompletedTask;

    public ValueTask ApplyUpdateAsync() => js.InvokeVoidAsync("kaesseliPwa.applyUpdate");

    public ValueTask DisposeAsync()
    {
        _ref?.Dispose();
        _ref = null;
        return ValueTask.CompletedTask;
    }
}
