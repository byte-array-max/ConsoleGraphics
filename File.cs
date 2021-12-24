using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace ConsoleGraphics
{
    public static class AdvanceConsole
    {
        public class PictureBox
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetConsoleWindow();

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

            [DllImport("user32.dll")]
            public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

            [DllImport("user32.dll")]
            private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            const int MF_BYCOMMAND = 0x00000000;
            const int SC_MINIMIZE = 0xF020;
            const int SC_MAXIMIZE = 0xF030;
            const int SC_SIZE = 0xF000;

            private Graphics g;
            private IntPtr hWnd;
            private static Bitmap img;

            private int Left;
            private int Top;

            public PictureBox()
            {
                hWnd = GetConsoleWindow();
                IntPtr consoleHandle = GetStdHandle(-10);
                GetConsoleMode(consoleHandle, out uint consoleMode);
                consoleMode &= 0xFFFFFFBF;
                SetConsoleMode(consoleHandle, consoleMode);
                DeleteMenu(GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(GetSystemMenu(hWnd, false), SC_SIZE, MF_BYCOMMAND);
                Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
                g = Graphics.FromHwnd(hWnd);
                img = new Bitmap(100, 100);
                Left = 0;
                Top = 0;
            }
            public int X
            {
                get
                {
                    return Left;
                }
                set
                {
                    g.Clear(Color.FromArgb(12, 12, 12));
                    g = ImageX(img, g, value);
                }
            }
            public int Y
            {
                get
                {
                    return Top;
                }
                set
                {
                    g.Clear(Color.FromArgb(12, 12, 12));
                    g = ImageY(img, g, value);
                }
            }
            private Graphics ImageX(Bitmap bmp, Graphics graphics,int left)
            {
                img = bmp;
                Left = left;
                graphics.DrawImage(bmp, Left, Top);
                return graphics;
            }
            private Graphics ImageY(Bitmap bmp, Graphics graphics,int top)
            {
                img = bmp;
                Top = top;

                graphics.DrawImage(bmp, Left, Top);
                return graphics;
            }
            public Graphics GetGraphics
            {
                get
                {
                    return g;
                }
            }
            public Bitmap GetBitmap
            {
                get
                {
                    return img;
                }
            }
            public void Image(Bitmap bmp)
            {
                img = bmp;

                g.DrawImage(bmp, Left, Top);
            }
        }
        public class Button
        {
            private LowLevelMouseProc _proc;

            private IntPtr _hookID = IntPtr.Zero;

            [DllImport("kernel32.dll")]
            static extern IntPtr GetConsoleWindow();

            [DllImport("user32.dll")]
            static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

            const int MONITOR_DEFAULTTOPRIMARY = 1;

            [DllImport("user32.dll")]
            static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

            [StructLayout(LayoutKind.Sequential)]
            struct MONITORINFO
            {
                public uint cbSize;
                public RECT rcMonitor;
                public RECT rcWork;
                public uint dwFlags;
                public static MONITORINFO Default
                {
                    get { var inst = new MONITORINFO(); inst.cbSize = (uint)Marshal.SizeOf(inst); return inst; }
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            struct RECT
            {
                public int Left, Top, Right, Bottom;
            }

            [DllImport("user32.dll", SetLastError = true)]
            static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

            [StructLayout(LayoutKind.Sequential)]
            struct WINDOWPLACEMENT
            {
                public uint Length;
                public uint Flags;
                public uint ShowCmd;
                public POINT MinPosition;
                public POINT MaxPosition;
                public RECT NormalPosition;
                public static WINDOWPLACEMENT Default
                {
                    get
                    {
                        var instance = new WINDOWPLACEMENT();
                        instance.Length = (uint)Marshal.SizeOf(instance);
                        return instance;
                    }
                }
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

            private void Start()
            {
                IntPtr consoleHandle = GetStdHandle(-10);
                GetConsoleMode(consoleHandle, out uint consoleMode);
                consoleMode &= 0xFFFFFFBF;
                SetConsoleMode(consoleHandle, consoleMode);

                _hookID = SetHook(_proc);

                Application.Run();

                UnhookWindowsHookEx(_hookID);
            }

            private static IntPtr SetHook(LowLevelMouseProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)

                {
                    return SetWindowsHookEx(WH_MOUSE_LL, proc,

                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);


            private const int WH_MOUSE_LL = 14;

            private enum MouseMessages
            {
                WM_LBUTTONDOWN = 0x0201,
                WM_LBUTTONUP = 0x0202,
                WM_MOUSEMOVE = 0x0200,
                WM_MOUSEWHEEL = 0x020A,
                WM_RBUTTONDOWN = 0x0204,
                WM_RBUTTONUP = 0x0205
            }

            [StructLayout(LayoutKind.Sequential)]

            private struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]

            private struct MSLLHOOKSTRUCT
            {
                public POINT pt;
                public uint mouseData;
                public uint flags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

            [return: MarshalAs(UnmanagedType.Bool)]

            private static extern bool UnhookWindowsHookEx(IntPtr hhk);


            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]

            private static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    if (GetForegroundWindow() != (IntPtr)0)
                    {
                        var mi = MONITORINFO.Default;
                        GetMonitorInfo(MonitorFromWindow(hWnd, MONITOR_DEFAULTTOPRIMARY), ref mi);
                        var wp = WINDOWPLACEMENT.Default;
                        GetWindowPlacement(hWnd, ref wp);
                        int X = hookStruct.pt.x;
                        int Y = hookStruct.pt.y;
                        if (((Left + wp.NormalPosition.Left + 7) <= X) && (X <= (Left + img.Width + wp.NormalPosition.Left + 7)) && ((Top + wp.NormalPosition.Top + 30) <= Y) && (Y <= (Top + img.Height + wp.NormalPosition.Top + 30)))
                        {
                            Onclick();
                        }
                    }
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            [DllImport("user32.dll")]
            public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

            [DllImport("user32.dll")]
            private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            public function Onclick
            {
                get;
                set;
            }
           
            private void Empty()
            {

            }
            public delegate void function();
            private int Left;
            private int Top;
            public int X 
            {
                get
                {
                    return Left;
                }
                set
                {
                    Left = value;
                    g.Clear(Color.FromArgb(12,12,12));
                    g.DrawImage(img, new Point(Left, Top));
                }
            }
            public int Y
            {
                get
                {
                    return Left;
                }
                set
                {
                    Top = value;
                    g.Clear(Color.FromArgb(12, 12, 12));
                    g.DrawImage(img, new Point(Left, Top));
                }
            }
            public void Image(Bitmap bitmap)
            {
                img = bitmap;
                g.DrawImage(img, Left, Top);
            }
            public Graphics GetGraphics
            {
                get
                {
                    return g;
                }
            }
            public Bitmap GetImage
            {
                get
                {
                    return img;
                }
            }
            private Bitmap img;
            private IntPtr hWnd;
            private Graphics g;
            const int MF_BYCOMMAND = 0x00000000;
            const int SC_MAXIMIZE = 0xF030;
            const int SC_SIZE = 0xF000;
            public Button()
            {
                _proc = HookCallback;
                hWnd = GetConsoleWindow();
                IntPtr consoleHandle = GetStdHandle(-10);
                GetConsoleMode(consoleHandle, out uint consoleMode);
                consoleMode &= 0xFFFFFFBF;
                SetConsoleMode(consoleHandle, consoleMode);
                DeleteMenu(GetSystemMenu(hWnd, false), SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(GetSystemMenu(hWnd, false), SC_SIZE, MF_BYCOMMAND);
                Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
                Left = 0;
                Top = 0;
                img = new Bitmap(100, 100);
                g = Graphics.FromHwnd(hWnd);
                new Thread(Start).Start();
            }
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCurrentConsoleFont(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            [Out][MarshalAs(UnmanagedType.LPStruct)] ConsoleFontInfo lpConsoleCurrentFont);

        [StructLayout(LayoutKind.Sequential)]
        internal class ConsoleFontInfo
        {
            internal int nFont;
            internal Coord dwFontSize;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Coord
        {
            [FieldOffset(0)]
            internal short X;
            [FieldOffset(2)]
            internal short Y;
        }

        public static (int,int) GetConsoleFontSize()
        {
            // getting the console out buffer handle
            IntPtr outHandle = CreateFile("CONOUT$", unchecked((int)0x80000000) | 0x40000000,
                1 | 2,
                IntPtr.Zero,
                3,
                0,
                IntPtr.Zero);
            int errorCode = Marshal.GetLastWin32Error();
            if (outHandle.ToInt32() == -1)
            {
                throw new IOException("Unable to open CONOUT$", errorCode);
            }

            ConsoleFontInfo cfi = new ConsoleFontInfo();
            if (!GetCurrentConsoleFont(outHandle, false, cfi))
            {
                throw new InvalidOperationException("Unable to get font information.");
            }

            return (cfi.dwFontSize.X, cfi.dwFontSize.Y);
        }
    }
}
