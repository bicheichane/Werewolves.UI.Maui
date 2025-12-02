using Plugin.Maui.Audio;
using Werewolves.GameLogic.Services;
using Werewolves.StateModels.Core;
using Werewolves.StateModels.Enums;
using Werewolves.StateModels.Models;

namespace Werewolves.Client.Services;

public class GameClientManager
{
    private readonly IAudioManager _audioManager;
    private readonly AudioMap _audioMap;
    private IAudioPlayer? _currentAudioPlayer;

    // Event to notify UI of state changes
    public event Action? StateChanged;

    // TODO: Replace 'object' with actual GameSession type when available
    public IGameSession? ActiveSession { get; private set; }
    private readonly GameService _gameService = new();
    
    public List<string>? Players => ActiveSession?.GetPlayers().Select(p => p.Name).ToList();

    public GameClientManager(IAudioManager audioManager, AudioMap audioMap)
    {
        _audioManager = audioManager;
        _audioMap = audioMap;
    }

    public async Task ProcessInputAsync(ModeratorResponse response)
    {
        if(ActiveSession == null) return;

		var nextInstruction = _gameService.ProcessInstruction(ActiveSession.Id, response);
        
        StateChanged?.Invoke();
        await SaveStateAsync();
    }

    private async Task SaveStateAsync()
    {
        // TODO: Implement persistence
        await Task.CompletedTask;
    }

    public async void PlayAudio(string instructionId)
    {
        var fileName = _audioMap.GetAudioFile(instructionId);
        if (string.IsNullOrEmpty(fileName)) return;

        try
        {
            // Stop current audio if playing
            if (_currentAudioPlayer != null && _currentAudioPlayer.IsPlaying)
            {
                _currentAudioPlayer.Stop();
                _currentAudioPlayer.Dispose();
                _currentAudioPlayer = null;
            }

            // Load and play new audio
            var stream = await FileSystem.OpenAppPackageFileAsync($"Audio/{fileName}");
            _currentAudioPlayer = _audioManager.CreatePlayer(stream);
            _currentAudioPlayer.Loop = true; // Default to looping as per requirements
            _currentAudioPlayer.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing audio: {ex.Message}");
        }
    }

    public async Task StartGameAsync(List<string> players, List<MainRoleType> startingRoleList)
    {
        var instruction = _gameService.StartNewGame(players, startingRoleList);
        ActiveSession = _gameService.GetGameStateView(instruction.GameGuid);
        
        // Enable Wake Lock
        DeviceDisplay.Current.KeepScreenOn = true;

        StateChanged?.Invoke();
        await SaveStateAsync();
    }
}
