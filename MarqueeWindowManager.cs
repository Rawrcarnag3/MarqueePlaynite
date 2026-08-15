using System;
using System.Windows;
using System.Windows.Forms;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace MarqueePlaynite
{
    /// <summary>
    /// Owns a single persistent marquee window and crossfades content inside it (see
    /// MarqueeWindow.ShowMedia) rather than creating a new window per swap. A solid
    /// black window that's always there and never fades is what stops the desktop from
    /// flashing through mid-transition.
    /// </summary>
    internal class MarqueeWindowManager
    {
        private readonly IPlayniteAPI api;
        private readonly MarqueePluginSettings settings;

        private MarqueeWindow window;
        private int windowContentWidth;
        private int windowContentHeight;
        private int windowScreenIndex;

        private string lastShownPath;
        private DateTime introHoldUntilUtc = DateTime.MinValue;

        public MarqueeWindowManager(IPlayniteAPI api, MarqueePluginSettings settings)
        {
            this.api = api;
            this.settings = settings;
        }

        public void ShowIntro()
        {
            var path = MarqueeResolver.ResolveIntro(settings.MarqueesFolderPath);

            // Playnite tends to auto-highlight the first game in the library right on
            // load, which would otherwise fire OnGameSelected and instantly replace the
            // intro before you've actually touched anything. Hold the intro for a few
            // seconds and ignore selection-driven updates during that window - same grace
            // period the original PowerShell poller used.
            introHoldUntilUtc = DateTime.UtcNow.AddSeconds(Math.Max(0, settings.IntroHoldSeconds));

            ShowPath(path, force: true);
        }

        public void ShowForGame(Game game)
        {
            if (game == null)
            {
                return;
            }

            if (DateTime.UtcNow < introHoldUntilUtc)
            {
                return;
            }

            var path = MarqueeResolver.ResolveForGame(settings.MarqueesFolderPath, game.Id.ToString(), game.Name);
            ShowPath(path, force: false);
        }

        private void ShowPath(string path, bool force)
        {
            if (!settings.Enabled || string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!force && string.Equals(path, lastShownPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            RunOnUiThread(() =>
            {
                lastShownPath = path;
                EnsureWindow();
                window.ShowMedia(path, Math.Max(0, settings.FadeDurationMs));
            });
        }

        private System.Drawing.Rectangle GetTargetScreenBounds()
        {
            var screens = Screen.AllScreens;
            var index = settings.TargetScreenIndex - 1;
            if (index < 0 || index >= screens.Length)
            {
                index = 0;
            }

            return screens[index].Bounds;
        }

        /// <summary>
        /// Creates the persistent window on first use, and recreates it only if
        /// geometry-affecting settings (monitor, marquee size) changed since it was
        /// built - otherwise the same window (and its solid black background) stays up
        /// across every marquee swap.
        /// </summary>
        private void EnsureWindow()
        {
            var needsRecreate = window == null
                || windowContentWidth != settings.MarqueeWidth
                || windowContentHeight != settings.MarqueeHeight
                || windowScreenIndex != settings.TargetScreenIndex;

            if (!needsRecreate)
            {
                return;
            }

            try { window?.Close(); }
            catch { /* ignore; we're replacing it anyway */ }

            var bounds = GetTargetScreenBounds();

            window = new MarqueeWindow(settings.MarqueeWidth, settings.MarqueeHeight, bounds.Width, bounds.Height)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = 0,
                Top = 0
            };
            window.SourceInitialized += (s, e) => window.PlaceOnScreen(bounds.X, bounds.Y);
            window.Show();

            windowContentWidth = settings.MarqueeWidth;
            windowContentHeight = settings.MarqueeHeight;
            windowScreenIndex = settings.TargetScreenIndex;
        }

        public void HideCurrent()
        {
            RunOnUiThread(() =>
            {
                try { window?.Hide(); }
                catch { }
            });
        }

        public void ShowCurrent()
        {
            RunOnUiThread(() =>
            {
                try { window?.Show(); }
                catch { }
            });
        }

        public void CloseAll()
        {
            RunOnUiThread(() =>
            {
                try { window?.Close(); }
                catch { }
                window = null;
            });
        }

        private void RunOnUiThread(Action action)
        {
            var dispatcher = api?.MainView?.UIDispatcher;
            if (dispatcher == null)
            {
                action();
                return;
            }

            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }
    }
}
