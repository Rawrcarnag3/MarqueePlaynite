using System;
using System.Linq;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;

namespace MarqueePlaynite
{
    public class MarqueePlaynitePlugin : GenericPlugin
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private readonly MarqueePluginSettings settings;
        private readonly MarqueeWindowManager windowManager;

        public override Guid Id { get; } = Guid.Parse("9f1e2713-e536-48d0-8de6-9f93f855a8c3");

        public MarqueePlaynitePlugin(IPlayniteAPI api) : base(api)
        {
            LogHost.Logger = logger;
            settings = new MarqueePluginSettings(this);
            windowManager = new MarqueeWindowManager(api, settings);
            Properties = new GenericPluginProperties { HasSettings = true };

            settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MarqueePluginSettings.Enabled))
                {
                    if (settings.Enabled)
                    {
                        windowManager.ShowCurrent();
                    }
                    else
                    {
                        windowManager.HideCurrent();
                    }
                }
            };
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            try
            {
                if (settings.ShowIntroOnStartup)
                {
                    windowManager.ShowIntro();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Marquee: failed to show intro marquee on startup.");
            }
        }

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            try
            {
                var game = args.NewValue?.FirstOrDefault();
                if (game != null)
                {
                    windowManager.ShowForGame(game);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Marquee: failed to update marquee on game selection.");
            }
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            try
            {
                if (settings.UpdateOnGameStarting)
                {
                    windowManager.ShowForGame(args.Game);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Marquee: failed to update marquee on game start.");
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            try
            {
                windowManager.CloseAll();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Marquee: failed to clean up marquee window on shutdown.");
            }
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new MarqueeSettingsView();
        }
    }
}
