using System;
using System.IO;

namespace MarqueePlaynite
{
    /// <summary>
    /// Figures out which media file to show for a game. Same lookup order as the
    /// original AutoHotkey tool's auto-folder method:
    ///   1. &lt;MarqueesFolder&gt;\&lt;GameId&gt;.(mp4|png|jpg|jpeg|webp)
    ///   2. &lt;MarqueesFolder&gt;\&lt;GameName&gt;.(mp4|png|jpg|jpeg|webp)   (sanitized)
    ///   3. &lt;MarqueesFolder&gt;\default_marquee.*                     (fallback)
    /// </summary>
    internal static class MarqueeResolver
    {
        private static readonly string[] Extensions = { ".mp4", ".png", ".jpg", ".jpeg", ".webp" };

        public static string ResolveForGame(string marqueesFolder, string gameId, string gameName)
        {
            if (string.IsNullOrWhiteSpace(marqueesFolder) || !Directory.Exists(marqueesFolder))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(gameId))
            {
                var byId = FindWithExtensions(marqueesFolder, gameId);
                if (byId != null)
                {
                    return byId;
                }
            }

            if (!string.IsNullOrWhiteSpace(gameName))
            {
                var safeName = SanitizeFileName(gameName);
                var byName = FindWithExtensions(marqueesFolder, safeName);
                if (byName != null)
                {
                    return byName;
                }
            }

            return ResolveDefault(marqueesFolder);
        }

        public static string ResolveDefault(string marqueesFolder)
        {
            return FindWithExtensions(marqueesFolder, "default_marquee");
        }

        public static string ResolveIntro(string marqueesFolder)
        {
            return FindWithExtensions(marqueesFolder, "INTRO-Playnite_marquee")
                   ?? ResolveDefault(marqueesFolder);
        }

        private static string FindWithExtensions(string folder, string baseName)
        {
            foreach (var ext in Extensions)
            {
                var candidate = Path.Combine(folder, baseName + ext);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalid, chars[i]) >= 0)
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }
    }
}
