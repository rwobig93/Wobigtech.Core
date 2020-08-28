using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Wobigtech.Core.Enums;

namespace Wobigtech.Companion.Shared
{
    public static class WINAPI
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static readonly IntPtr winHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
        public static void WindowStateChange(WINAPIWindowState preferredState)
        {
            ShowWindow(winHandle, (int)preferredState);
        }
    }
}
