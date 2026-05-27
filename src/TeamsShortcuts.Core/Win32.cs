using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace TeamsShortcuts.Core;

public static class Win32
{
    private const int SW_RESTORE = 9;
    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;

    public static IReadOnlyList<Win32Window> GetTopLevelWindows()
    {
        var windows = new List<Win32Window>();

        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd))
            {
                return true;
            }

            GetWindowRect(hwnd, out var rect);
            var processId = GetProcessId(hwnd);
            windows.Add(new Win32Window(
                hwnd,
                GetWindowTitle(hwnd),
                GetClassName(hwnd),
                processId,
                GetProcessName(processId),
                rect));

            return true;
        }, IntPtr.Zero);

        return windows;
    }

    public static string GetWindowTitle(IntPtr hwnd)
    {
        var length = GetWindowTextLength(hwnd);
        if (length <= 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(length + 1);
        _ = GetWindowText(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    public static string GetClassName(IntPtr hwnd)
    {
        var builder = new StringBuilder(256);
        _ = GetClassName(hwnd, builder, builder.Capacity);
        return builder.ToString();
    }

    public static int GetProcessId(IntPtr hwnd)
    {
        _ = GetWindowThreadProcessId(hwnd, out var processId);
        return unchecked((int)processId);
    }

    public static IntPtr GetForegroundWindowHandle() => GetForegroundWindow();

    public static bool BringToForeground(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        if (IsIconic(hwnd))
        {
            _ = ShowWindowAsync(hwnd, SW_RESTORE);
            Thread.Sleep(150);
        }

        var result = SetForegroundWindow(hwnd);
        Thread.Sleep(150);
        return result;
    }

    public static bool RestoreForegroundWindow(IntPtr previousHwnd)
    {
        if (previousHwnd == IntPtr.Zero)
        {
            return false;
        }

        var result = SetForegroundWindow(previousHwnd);
        Thread.Sleep(100);
        return result;
    }

    public static bool MoveMouseToBottomCenter(IntPtr hwnd)
    {
        if (!GetWindowRect(hwnd, out var rect) || rect.Width <= 0 || rect.Height <= 0)
        {
            return false;
        }

        var x = rect.Left + (rect.Width / 2);
        var yOffset = Math.Clamp(rect.Height / 6, 80, 140);
        var y = rect.Bottom - yOffset;
        return SetCursorPos(x, y);
    }

    public static void SendKeyDown(ushort virtualKey)
    {
        SendKeyboardInputs([KeyInput(virtualKey, keyUp: false)]);
    }

    public static void SendKeyUp(ushort virtualKey)
    {
        SendKeyboardInputs([KeyInput(virtualKey, keyUp: true)]);
    }

    public static void SendKeyboardInputs(IReadOnlyList<INPUT> inputs)
    {
        var array = inputs.ToArray();
        var sent = SendInput((uint)array.Length, array, Marshal.SizeOf<INPUT>());
        if (sent != array.Length)
        {
            throw new InvalidOperationException($"SendInput sent {sent} of {array.Length} keyboard events.");
        }
    }

    public static INPUT KeyDownInput(ushort virtualKey) => KeyInput(virtualKey, keyUp: false);
    public static INPUT KeyUpInput(ushort virtualKey) => KeyInput(virtualKey, keyUp: true);

    public static void MouseLeftClick()
    {
        var inputs = new[]
        {
            new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTDOWN }
                }
            },
            new INPUT
            {
                type = INPUT_MOUSE,
                U = new InputUnion
                {
                    mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTUP }
                }
            }
        };

        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException($"SendInput sent {sent} of {inputs.Length} mouse events.");
        }
    }

    private static INPUT KeyInput(ushort virtualKey, bool keyUp)
    {
        return new INPUT
        {
            type = INPUT_KEYBOARD,
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = virtualKey,
                    dwFlags = keyUp ? KEYEVENTF_KEYUP : 0
                }
            }
        };
    }

    private static string GetProcessName(int processId)
    {
        if (processId <= 0)
        {
            return string.Empty;
        }

        try
        {
            using var process = Process.GetProcessById(processId);
            return process.ProcessName;
        }
        catch
        {
            return string.Empty;
        }
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RECT
    {
        public readonly int Left;
        public readonly int Top;
        public readonly int Right;
        public readonly int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public override string ToString() => $"{Left},{Top},{Right},{Bottom} ({Width}x{Height})";
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}

public sealed record Win32Window(
    IntPtr Hwnd,
    string Title,
    string ClassName,
    int ProcessId,
    string ProcessName,
    Win32.RECT Rect)
{
    public string HwndHex => "0x" + Hwnd.ToInt64().ToString("X");
}
