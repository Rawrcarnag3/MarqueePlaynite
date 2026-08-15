using System.Collections.Generic;
using System.IO;
using Playnite.SDK;

namespace MarqueePlaynite
{
    public class MarqueePluginSettings : ObservableObject, ISettings
    {
        private readonly MarqueePlaynitePlugin plugin;

        private bool enabled = true;
        private int targetScreenIndex = 2;
        private int marqueeWidth = 1920;
        private int marqueeHeight = 360;
        private int fadeDurationMs = 300;
        private bool showIntroOnStartup = true;
        private int introHoldSeconds = 3;
        private bool updateOnGameStarting = true;
        private string marqueesFolderPath = string.Empty;

        public bool Enabled { get => enabled; set => SetValue(ref enabled, value); }
        public int TargetScreenIndex { get => targetScreenIndex; set => SetValue(ref targetScreenIndex, value); }
        public int MarqueeWidth { get => marqueeWidth; set => SetValue(ref marqueeWidth, value); }
        public int MarqueeHeight { get => marqueeHeight; set => SetValue(ref marqueeHeight, value); }
        public int FadeDurationMs { get => fadeDurationMs; set => SetValue(ref fadeDurationMs, value); }
        public bool ShowIntroOnStartup { get => showIntroOnStartup; set => SetValue(ref showIntroOnStartup, value); }
        public int IntroHoldSeconds { get => introHoldSeconds; set => SetValue(ref introHoldSeconds, value); }
        public bool UpdateOnGameStarting { get => updateOnGameStarting; set => SetValue(ref updateOnGameStarting, value); }
        public string MarqueesFolderPath { get => marqueesFolderPath; set => SetValue(ref marqueesFolderPath, value); }

        // Parameterless constructor required by LoadPluginSettings.
        public MarqueePluginSettings()
        {
        }

        public MarqueePluginSettings(MarqueePlaynitePlugin plugin)
        {
            this.plugin = plugin;

            var saved = plugin.LoadPluginSettings<MarqueePluginSettings>();
            if (saved != null)
            {
                Enabled = saved.Enabled;
                TargetScreenIndex = saved.TargetScreenIndex;
                MarqueeWidth = saved.MarqueeWidth;
                MarqueeHeight = saved.MarqueeHeight;
                FadeDurationMs = saved.FadeDurationMs;
                ShowIntroOnStartup = saved.ShowIntroOnStartup;
                IntroHoldSeconds = saved.IntroHoldSeconds;
                UpdateOnGameStarting = saved.UpdateOnGameStarting;
                MarqueesFolderPath = saved.MarqueesFolderPath;
            }

            if (string.IsNullOrWhiteSpace(MarqueesFolderPath))
            {
                // Default location lives outside the extension's own install folder because
                // Playnite wipes and replaces that folder on every update - see "Data directories"
                // in the extension docs. Point this at your old Marquees\ folder in the
                // Settings panel if you want to keep using your existing images as-is.
                MarqueesFolderPath = Path.Combine(plugin.GetPluginUserDataPath(), "Marquees");
            }

            Directory.CreateDirectory(MarqueesFolderPath);
        }

        public void BeginEdit()
        {
            // Nothing to snapshot beyond what CancelEdit already restores from disk.
        }

        public void CancelEdit()
        {
            var saved = plugin.LoadPluginSettings<MarqueePluginSettings>();
            if (saved == null)
            {
                return;
            }

            Enabled = saved.Enabled;
            TargetScreenIndex = saved.TargetScreenIndex;
            MarqueeWidth = saved.MarqueeWidth;
            MarqueeHeight = saved.MarqueeHeight;
            FadeDurationMs = saved.FadeDurationMs;
            ShowIntroOnStartup = saved.ShowIntroOnStartup;
                IntroHoldSeconds = saved.IntroHoldSeconds;
            UpdateOnGameStarting = saved.UpdateOnGameStarting;
            MarqueesFolderPath = saved.MarqueesFolderPath;
        }

        public void EndEdit()
        {
            if (!string.IsNullOrWhiteSpace(MarqueesFolderPath))
            {
                Directory.CreateDirectory(MarqueesFolderPath);
            }

            plugin.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();

            if (MarqueeWidth <= 0 || MarqueeHeight <= 0)
            {
                errors.Add("Marquee width and height must both be greater than 0.");
            }

            if (TargetScreenIndex <= 0)
            {
                errors.Add("Target screen must be 1 (primary) or higher.");
            }

            if (string.IsNullOrWhiteSpace(MarqueesFolderPath))
            {
                errors.Add("Marquees folder path can't be empty.");
            }

            return errors.Count == 0;
        }
    }
}
