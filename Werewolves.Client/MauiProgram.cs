using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Plugin.Maui.Audio;
using Werewolves.Client.Services;

#if WINDOWS
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace Werewolves.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // MudBlazor
            builder.Services.AddMudServices();

            // Audio
            builder.Services.AddSingleton(AudioManager.Current);

            // Game Services
            builder.Services.AddSingleton<AudioMap>();
            builder.Services.AddSingleton<IconMap>();
            builder.Services.AddSingleton<IGameService, GameServiceWrapper>();
            builder.Services.AddSingleton<GameClientManager>();

#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(wndLifeCycleBuilder =>
                {
                    wndLifeCycleBuilder.OnWindowCreated(window =>
                    {
                        var nativeWindow = window as Microsoft.UI.Xaml.Window;
                        if (nativeWindow is null) return;

                        int newWidth = 400;
                        int newHeight = 800;
                        nativeWindow.AppWindow.Resize(new SizeInt32(newWidth, newHeight));

                        if (nativeWindow.AppWindow.Presenter is OverlappedPresenter presenter)
                        {
                            presenter.IsResizable = false;
                            presenter.IsMaximizable = false;
                        }
                    });
                });
            });
#endif

            return builder.Build();
        }
    }
}
