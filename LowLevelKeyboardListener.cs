using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;


using DesktopWPFAppLowLevelKeyboardHook;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopWPFAppLowLevelKeyboardHook
{
    
    public class LowLevelKeyboardListener
    {
        




        private const int WH_MOUSE_LL = 14;
        private const int MK_XBUTTON2 = 0x0040;
        private const int MK_XBUTTON1 = 0x0020;
        private const int MK_RBUTTON = 0x0002;
        private const int MK_LBUTTON = 0x0001;
        private const int MK_MBUTTON = 0x0010;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_NCXBUTTONDOWN = 0x00AB;
        private const int WM_MBUTTONDOWN = 0x0207;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        [DllImport("user32.dll")]
        public static extern void SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<KeyPressedArgs> OnKeyPressed;

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData,
        UIntPtr dwExtraInfo);


      
        [Flags]
        internal enum MOUSEEVENTF : uint
        {
            ABSOLUTE = 0x8000,
            HWHEEL = 0x01000,
            MOVE = 0x0001,
            MOVE_NOCOALESCE = 0x2000,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            VIRTUALDESK = 0x4000,
            WHEEL = 0x0800,
            XDOWN = 0x0080,
            XUP = 0x0100
        }
       
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            internal int dx;
            internal int dy;
            internal int mouseData;
            internal MOUSEEVENTF dwFlags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }






        public event EventHandler<MousePressedArgs> OnMousePressed;

        private LowLevelKeyboardProc _proc_mouse;
        private IntPtr _mouse_hookID = IntPtr.Zero;


        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

       
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        public const UInt32 SPI_SETMOUSESPEED = 0x0071;

        
        internal static class NativeMethods
        {
            internal static ushort HIWORD(IntPtr dwValue)
            {
                return (ushort)((((long)dwValue) >> 0x10) & 0xffff);
            }

            internal static ushort HIWORD(int dwValue)
            {
                return (ushort)(dwValue >> 0x10);
            }

            internal static int GET_WHEEL_DELTA_WPARAM(IntPtr wParam)
            {
                return (short)HIWORD(wParam);
            }

            internal static int GET_WHEEL_DELTA_WPARAM(int wParam)
            {
                return (short)HIWORD(wParam);
            }
        }
        
        public LowLevelKeyboardListener()
        {
            _proc_mouse = MouseHookCallback;
            _proc = HookCallback;
        }



        public void HookMouse()
        {
            _mouse_hookID = SetMouseHook(_proc_mouse);
        }

        public void UnHookMouse()
        {
            UnhookWindowsHookEx(_mouse_hookID);
        }



        public void HookKeyboard()
        {
            _hookID = SetHook(_proc);
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
        }


        private IntPtr SetMouseHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        
        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            
            if (nCode >= 0 && (int)wParam == WM_MOUSEWHEEL) //scrollwheel
            {
                MSLLHOOKSTRUCT mouseData = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                int wheelMovement = (int)mouseData.mouseData>>16;
                
                string mes = wheelMovement.ToString();
    
                if (OnMousePressed != null) { OnMousePressed(this, new MousePressedArgs(mes)); }
            
            }
            else if (nCode >= 0 && (int)wParam == WM_MBUTTONDOWN) // midlle mouse button
            {
               

                string mes = "MiddleButton";

                if (OnMousePressed != null) { OnMousePressed(this, new MousePressedArgs(mes)); }
            }
            else if (nCode >= 0 && (int)wParam == WM_XBUTTONDOWN)// side buttons
            {
                MSLLHOOKSTRUCT mouseData = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                int wheelMovement = (int)mouseData.mouseData >> 16;
                string mes;
                if (wheelMovement == 1)
                {
                    mes = "XButton1";

                }
                else { mes = "XButton2"; }
                
               

                if (OnMousePressed != null) { OnMousePressed(this, new MousePressedArgs(mes)); }
            }




            else if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {

                
                string mes = "LeftButton";
                
                
                if (OnMousePressed != null) { OnMousePressed(this, new MousePressedArgs(mes)); }
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_RBUTTONDOWN) { 

                
                string mes = "RightButton";
                
                if (OnMousePressed != null) { OnMousePressed(this, new MousePressedArgs(mes)); }
            }
            

            return CallNextHookEx(_mouse_hookID, nCode, wParam, lParam);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                if (OnKeyPressed != null) { OnKeyPressed(this, new KeyPressedArgs(KeyInterop.KeyFromVirtualKey(vkCode))); }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }

    public class MousePressedArgs : EventArgs {
        public string MousePressed { get; private set; }

        public MousePressedArgs(string key) {
            
            
            MousePressed = key;
         
        
        }
    
    
    
    }
    
    public class KeyPressedArgs : EventArgs
    {
        public Key KeyPressed { get; private set; }

        public KeyPressedArgs(Key key)
        {
            KeyPressed = key;
        }
    }
}
