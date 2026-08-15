using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace MarqueePlaynite
{
    public partial class MarqueeSettingsView : UserControl
    {
        public List<ScreenChoice> ScreenChoices { get; } = new List<ScreenChoice>();

        public MarqueeSettingsView()
        {
            InitializeComponent();
            BuildScreenChoices();
            ScreenCombo.ItemsSource = ScreenChoices;
        }

        private void BuildScreenChoices()
        {
            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                var s = screens[i];
                var label = $"Screen {i + 1} - {s.Bounds.Width}x{s.Bounds.Height}" + (s.Primary ? " (Primary)" : "");
                ScreenChoices.Add(new ScreenChoice { Index = i + 1, Description = label });
            }

            if (ScreenChoices.Count == 0)
            {
                ScreenChoices.Add(new ScreenChoice { Index = 1, Description = "Screen 1" });
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select your Marquees folder";
                if (Directory.Exists(SettingsContext?.MarqueesFolderPath))
                {
                    dialog.SelectedPath = SettingsContext.MarqueesFolderPath;
                }

                if (dialog.ShowDialog() == DialogResult.OK && SettingsContext != null)
                {
                    SettingsContext.MarqueesFolderPath = dialog.SelectedPath;
                }
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var path = SettingsContext?.MarqueesFolderPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
            Process.Start("explorer.exe", $"\"{path}\"");
        }

        private MarqueePluginSettings SettingsContext => DataContext as MarqueePluginSettings;
    }

    public class ScreenChoice
    {
        public int Index { get; set; }
        public string Description { get; set; }
    }
}
