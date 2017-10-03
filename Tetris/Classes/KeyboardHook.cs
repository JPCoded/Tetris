#region

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#endregion


namespace Tetris
{

    internal sealed class KeyboardHook : IKeyboardHook
    {
        public delegate void KeyDownEventHandler(Keys key);

        public delegate void KeyUpEventHandler(Keys key);

        private const int WH_KEYBOARD_LL = 13;
        private const int HC_ACTION = 0;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;

        private const int WM_SYSKEYUP = 0x105;

        private readonly IntPtr _hookId;

        private readonly HookProc hookDelegate = KeyboardProc;

        public KeyboardHook()
        {
            _hookId = (IntPtr) SetWindowsHookEx(WH_KEYBOARD_LL, hookDelegate, IntPtr.Zero, 0);
            if (_hookId == IntPtr.Zero)
            {
                throw new Exception("Could not set keyboard hook");
            }
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc HookProc, IntPtr hInstance, int wParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        public static event KeyDownEventHandler KeyDown;
        public static event KeyUpEventHandler KeyUp;

        private static int KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HC_ACTION)
            {
                var @struct = default(KBDLLHOOKSTRUCT);
                switch (wParam)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        KeyDown?.Invoke(((KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, @struct.GetType())).vkCode);

                        break;
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        KeyUp?.Invoke(((KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, @struct.GetType())).vkCode);

                        break;
                }
            }
            return CallNextHookEx((int) IntPtr.Zero, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public readonly Keys vkCode;
            public readonly uint scanCode;
            public readonly KBDLLHOOKSTRUCTFlags flags;
            public readonly uint time;
            public readonly UIntPtr dwExtraInfo;
        }

        [Flags]
        private enum KBDLLHOOKSTRUCTFlags : uint
        {
            LLKHF_EXTENDED = 0x1,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80
        }

        private delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        #region "IDisposable Support"

        private bool disposedValue;

        // IDisposable
        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                if (!(_hookId == IntPtr.Zero))
                {
                    UnhookWindowsHookEx((int) _hookId);
                }
            }
            disposedValue = true;
        }

        ~KeyboardHook()
        {
            while (true)
            {
                Dispose(false);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}