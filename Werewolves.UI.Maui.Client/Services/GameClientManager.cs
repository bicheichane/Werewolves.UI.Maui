using Plugin.Maui.Audio;
using Werewolves.Core.StateModels.Core;
using Werewolves.Core.StateModels.Enums;
using Werewolves.Core.StateModels.Models;
using Werewolves.Core.StateModels.Models.Instructions;

namespace Werewolves.UI.Maui.Client.Services;

public class GameClientManager
{
    private readonly IAudioManager _audioManager;
    private readonly AudioMap _audioMap;
    private readonly IGameService _gameService;
    private readonly Dictionary<SoundEffectsEnum, IAudioPlayer> _activeSounds = new();

    // Event to notify UI of state changes
    public event Action? StateChanged;

    // Audio mute state
    public bool IsMuted { get; private set; }

    // TODO: Replace 'object' with actual GameSession type when available
    public IGameSession? ActiveSession { get; private set; }
    public ModeratorInstruction? CurrentInstruction { get; private set; }

	public List<string>? Players => ActiveSession?.GetPlayers().Select(p => p.Name).ToList();

	public IEnumerable<IPlayer>? PlayerData => ActiveSession?.GetPlayers();

    public GameClientManager(IAudioManager audioManager, AudioMap audioMap, IGameService gameService)
    {
        _audioManager = audioManager;
        _audioMap = audioMap;
        _gameService = gameService;
    }

    /// <summary>
    /// Forces a StateChanged event. Used by test infrastructure for state injection.
    /// </summary>
    internal void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Allows test infrastructure to set the current instruction directly.
    /// </summary>
    internal void SetCurrentInstruction(ModeratorInstruction instruction)
    {
        CurrentInstruction = instruction;
        NotifyStateChanged();
    }

    /// <summary>
    /// Allows test infrastructure to set the active session directly.
    /// </summary>
    internal void SetActiveSession(IGameSession session)
    {
        ActiveSession = session;
    }

    public async Task StartGameAsync(GameSessionConfig config)
    {
        var instruction = _gameService.StartNewGame(config);
        ActiveSession = _gameService.GetGameStateView(instruction.GameGuid);
        
        // Enable Wake Lock
        DeviceDisplay.Current.KeepScreenOn = true;

        StateChanged?.Invoke();
        await SaveStateAsync();
    }

    public async Task ProcessInputAsync(ModeratorResponse response)
    {
        if(ActiveSession == null) return;

		var result = _gameService.ProcessInstruction(ActiveSession.Id, response);
        if (result.IsSuccess)
        {
            CurrentInstruction = result.ModeratorInstruction;
			StateChanged?.Invoke();
            await SaveStateAsync();

            // Reconcile audio based on new instruction's sound effects
            ReconcileAudio();

            if (CurrentInstruction is FinishedGameConfirmationInstruction)
            {
                FinishGame();
            }
		}
        
    }

    private async Task SaveStateAsync()
    {
        // TODO: Implement persistence
        await Task.CompletedTask;
    }

    /// <summary>
    /// Reconciles audio based on the current instruction's SoundEffects list.
    /// Starts sounds that should be playing, stops sounds that shouldn't.
    /// </summary>
    private void ReconcileAudio()
    {
        var desiredSounds = CurrentInstruction?.SoundEffects ?? new List<SoundEffectsEnum>();
        var desiredSet = new HashSet<SoundEffectsEnum>(desiredSounds.Where(s => s != SoundEffectsEnum.None));

        // Stop sounds that are playing but not in the desired list
        var soundsToStop = _activeSounds.Keys.Where(s => !desiredSet.Contains(s)).ToList();
        foreach (var sound in soundsToStop)
        {
            StopAudio(sound);
        }

        // Start sounds that should be playing but aren't
        foreach (var sound in desiredSet)
        {
            if (!_activeSounds.ContainsKey(sound))
            {
                _ = StartAudioAsync(sound);
            }
        }
    }

    /// <summary>
    /// Starts playing a specific sound effect.
    /// </summary>
    private async Task StartAudioAsync(SoundEffectsEnum soundEffect)
    {
        var fileName = _audioMap.GetAudioFile(soundEffect);
        if (string.IsNullOrEmpty(fileName)) return;

        try
        {
            var stream = await FileSystem.OpenAppPackageFileAsync($"Audio/{fileName}");
            var player = _audioManager.CreatePlayer(stream);
            player.Loop = true;

            _activeSounds[soundEffect] = player;

            if (!IsMuted)
            {
                player.Play();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing audio {soundEffect}: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops and disposes a specific sound effect.
    /// </summary>
    public void StopAudio(SoundEffectsEnum soundEffect)
    {
        if (_activeSounds.TryGetValue(soundEffect, out var player))
        {
            if (player.IsPlaying)
            {
                player.Stop();
            }
            player.Dispose();
            _activeSounds.Remove(soundEffect);
        }
    }

    /// <summary>
    /// Stops all currently playing sounds.
    /// </summary>
    public void StopAllAudio()
    {
        foreach (var sound in _activeSounds.Keys.ToList())
        {
            StopAudio(sound);
        }
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;

        foreach (var player in _activeSounds.Values)
        {
            if (IsMuted && player.IsPlaying)
            {
                player.Pause();
            }
            else if (!IsMuted && !player.IsPlaying)
            {
                player.Play();
            }
        }

        StateChanged?.Invoke();
    }

    

    private void FinishGame()
    {
        StopAllAudio();
        DeviceDisplay.Current.KeepScreenOn = false;
	}
}
