using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace PassKeep.Material.Common
{
    class CursorInfo
    {
        [DllImport("user32.dll")]
        private static extern void GetCursorPos(out POINT pt);

        [DllImport("user32.dll")]
        private static extern int ScreenToClient(IntPtr hwnd, ref POINT pt);

        private struct POINT
        {
            public UInt32 X;
            public UInt32 Y;
        }

        public static Point GetCursorPosition()
        {
            POINT p;
            GetCursorPos(out p);
            return new Point(p.X, p.Y);
        }

        public static Point GetNowPosition(System.Windows.Media.Visual v)
        {
            POINT p;
            GetCursorPos(out p);

            var source = HwndSource.FromVisual(v) as HwndSource;
            var hwnd = source.Handle;

            ScreenToClient(hwnd, ref p);
            return new Point(p.X, p.Y);
        }
    }
}
