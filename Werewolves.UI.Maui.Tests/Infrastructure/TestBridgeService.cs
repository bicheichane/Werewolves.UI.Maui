using Microsoft.JSInterop;
using System.Text.Json;
using Werewolves.UI.Maui.Client.Services;
using Werewolves.UI.Maui.Tests.Mocks;

namespace Werewolves.UI.Maui.Tests.Infrastructure;

/// <summary>
/// Service that bridges JavaScript TestBridge calls to C# mock services.
/// Enables the QA Agent to inject state via Chrome DevTools.
/// </summary>
public class TestBridgeService : IDisposable
{
    private readonly MockGameService _mockGameService;
    private readonly GameClientManager _gameClientManager;
    private DotNetObjectReference<TestBridgeService>? _dotNetRef;

    public TestBridgeService(MockGameService mockGameService, GameClientManager gameClientManager)
    {
        _mockGameService = mockGameService;
        _gameClientManager = gameClientManager;
    }

    /// <summary>
    /// Called from JavaScript to inject a new test state.
    /// </summary>
    /// <param name="jsonState">JSON string containing the test state</param>
    [JSInvokable]
    public void InjectState(string jsonState)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var stateDto = JsonSerializer.Deserialize<TestStateDto>(jsonState, options);
        
        if (stateDto == null)
            throw new ArgumentException("Failed to deserialize test state", nameof(jsonState));

        // Apply state to mock service
        _mockGameService.ApplyTestState(stateDto);
        
        // Update GameClientManager
        _gameClientManager.SetActiveSession(_mockGameService.CurrentSession);
        
        if (stateDto.CurrentInstruction != null)
            _gameClientManager.SetCurrentInstruction(stateDto.CurrentInstruction);
        
        // Notify UI of state change
        _gameClientManager.NotifyStateChanged();
    }

    /// <summary>
    /// Registers this service with the JavaScript TestBridge.
    /// Call this after the Blazor app has started.
    /// </summary>
    public async Task RegisterWithJavaScript(IJSRuntime jsRuntime)
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        await jsRuntime.InvokeVoidAsync("TestBridge.register", _dotNetRef);
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}
