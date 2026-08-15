using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MarqueePlaynite
{
    public partial class MarqueeWindow : Window
    {
        private readonly int contentWidth;
        private readonly int contentHeight;
        private readonly int screenWidth;
        private readonly int screenHeight;

        // Tracks which of the two stacked layers is currently the visible/front one.
        private bool layerAIsFront = true;

        public MarqueeWindow(int contentWidth, int contentHeight, int screenWidth, int screenHeight)
        {
            InitializeComponent();

            this.contentWidth = contentWidth;
            this.contentHeight = contentHeight;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            Width = screenWidth;
            Height = screenHeight;
            ContentHost.Width = contentWidth;
            ContentHost.Height = contentHeight;

            VideoA.MediaEnded += (s, e) => LoopIfStillPlaying(VideoA);
            VideoB.MediaEnded += (s, e) => LoopIfStillPlaying(VideoB);
            VideoA.MediaFailed += (s, e) => LogHost.Logger?.Error(e.ErrorException, "Marquee video A failed to load.");
            VideoB.MediaFailed += (s, e) => LogHost.Logger?.Error(e.ErrorException, "Marquee video B failed to load.");
        }

        /// <summary>
        /// Crossfades to a new image/video. The black window background never moves or
        /// fades - only the two content layers swap opacity - so nothing behind the
        /// window is ever visible mid-transition.
        /// </summary>
        public void ShowMedia(string path, int fadeMs)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            Grid frontLayer, backLayer;
            Image frontImage, backImage;
            MediaElement frontVideo, backVideo;

            if (layerAIsFront)
            {
                frontLayer = LayerA; frontImage = ImageA; frontVideo = VideoA;
                backLayer = LayerB; backImage = ImageB; backVideo = VideoB;
            }
            else
            {
                frontLayer = LayerB; frontImage = ImageB; frontVideo = VideoB;
                backLayer = LayerA; backImage = ImageA; backVideo = VideoA;
            }

            LoadMediaInto(path, backImage, backVideo);
            backLayer.Opacity = 0;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(fadeMs));
            backLayer.BeginAnimation(OpacityProperty, fadeIn);

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(fadeMs));
            fadeOut.Completed += (s, e) => StopMedia(frontImage, frontVideo);
            frontLayer.BeginAnimation(OpacityProperty, fadeOut);

            layerAIsFront = !layerAIsFront;
        }

        private void LoadMediaInto(string path, Image image, MediaElement video)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".mp4")
            {
                image.Visibility = Visibility.Collapsed;
                image.Source = null;

                video.Visibility = Visibility.Visible;
                video.Source = new Uri(path, UriKind.Absolute);
                video.Position = TimeSpan.Zero;
                video.Play();
            }
            else
            {
                video.Stop();
                video.Source = null;
                video.Visibility = Visibility.Collapsed;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.DecodePixelWidth = contentWidth;
                bitmap.EndInit();
                bitmap.Freeze();

                image.Source = bitmap;
                image.Visibility = Visibility.Visible;
            }
        }

        private void StopMedia(Image image, MediaElement video)
        {
            try
            {
                video.Stop();
                video.Source = null;
            }
            catch
            {
                // Best-effort; nothing useful to do if this throws mid-shutdown.
            }

            video.Visibility = Visibility.Collapsed;
            image.Source = null;
            image.Visibility = Visibility.Collapsed;
        }

        private void LoopIfStillPlaying(MediaElement video)
        {
            if (video.Source == null)
            {
                return;
            }

            video.Position = TimeSpan.Zero;
            video.Play();
        }

        /// <summary>
        /// Places this window at an exact physical-pixel rectangle and keeps it topmost
        /// without stealing focus from Playnite.
        /// </summary>
        public void PlaceOnScreen(int x, int y)
        {
            var hwnd = new WindowInteropHelper(this).EnsureHandle();
            NativeMethods.PlaceTopmostNoActivate(hwnd, x, y, screenWidth, screenHeight);
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                VideoA.Stop();
                VideoA.Close();
                VideoB.Stop();
                VideoB.Close();
            }
            catch
            {
                // Best-effort cleanup; nothing useful to do if this throws during shutdown.
            }

            base.OnClosed(e);
        }
    }
}
