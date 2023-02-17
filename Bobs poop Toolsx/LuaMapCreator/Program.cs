using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace LuaMapCreator
{
    class Program
    {
        private static IntPtr hookId = IntPtr.Zero;
        private static StreamWriter mapFile;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static LowLevelKeyboardProc _proc = HookCallback;

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                mapFile.WriteLine("sendkeys.KeyPress((Keys)" + vkCode + ")");
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        static void Main(string[] args)
        {
            mapFile = new StreamWriter("map.lua");

            hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            //Application.Run();
            UnhookWindowsHookEx(hookId);

            
            Console.ReadLine();
            mapFile.Close();
        }
    }
}
