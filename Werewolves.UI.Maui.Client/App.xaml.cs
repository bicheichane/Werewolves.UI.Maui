using Werewolves.UI.Maui.Client.Services;

namespace Werewolves.UI.Maui.Client
{
    public partial class App : Application
    {
        private readonly IconMap _iconMap;
        private readonly AudioMap _audioMap;

        public App(IconMap iconMap, AudioMap audioMap)
        {
            InitializeComponent();
            _iconMap = iconMap;
            _audioMap = audioMap;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "Werewolves.Client" };
            
            // Validate assets on startup
            window.Created += async (s, e) =>
            {
                await _iconMap.ValidateIconsAsync();
                await _audioMap.ValidateAudioFilesAsync();
            };

            return window;
        }
    }
}
