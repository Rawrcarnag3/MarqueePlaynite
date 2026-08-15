using System;
using System.Runtime.InteropServices;

namespace MarqueePlaynite
{
    /// <summary>
    /// Positioning the marquee window through WPF's Left/Top/Width/Height properties
    /// goes through DPI-scaled "device independent pixels", which can quietly resize or
    /// blur the marquee on a scaled monitor. SetWindowPos works in raw physical pixels,
    /// which is what we want since the marquee is usually sized to match a specific
    /// panel/monitor's native resolution (mirrors what the old AutoHotkey tool did).
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        /// <summary>
        /// Moves/resizes a window to an exact physical-pixel rectangle, keeps it topmost,
        /// and never steals focus from Playnite.
        /// </summary>
        public static void PlaceTopmostNoActivate(IntPtr hwnd, int x, int y, int width, int height)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, x, y, width, height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }
}
