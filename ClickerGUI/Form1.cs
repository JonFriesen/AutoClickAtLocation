/*
 * 
 * This program is designed monitor the keyboard for a shortcut
 * then when it gets one, it will start clicking at the cursor position
 * at the time of the event.
 * 
 * Ctrl+F - Starts clicking
 * Ctrl+G - Stops clicking
 * 
 * Author: Jon Friesen (and other random snippets from the internets)
 * Date: March 16 2013
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace ClickerGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            
            InitializeComponent();
            //Set Static Delay 
            WindowsClicker.delay = Convert.ToInt32(textBox1.Text);
            //Set keyboard hook
            VirtualKeyboard.SetHook(label1, textBox1);
        }

        /// <summary>
        /// Event listener if someone clicks on my name it goes to my website...
        /// I know I'm greedy for credit...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://jonfriesen.ca");
        }

    }

    /// <summary>
    /// This is the static control class for the threaded clicker class.
    /// I'm not very good at threading, should be pretty straightforward
    /// though. The idea of it is it leaves is the static location
    /// for the thread to get it's data
    /// </summary>
    class WindowsClicker
    {
        //Determines if the thread should be clicking
        public static Boolean clicking = true;
        //The delay on how much delay between clicks
        public static int delay;
        //coordinates for click (should probably be a point...)
        public static int x, y;

        /// <summary>
        /// Starts the threaded click... abandons the thread... kind of gross
        /// </summary>
        public static void StartClick()
        {
            clicking = true;
            x = Cursor.Position.X;
            y = Cursor.Position.Y;
            new Thread(new ThreadStart(new ClickThread().StartClickingAt)).Start();
        }
        public static void StopClick()
        {
            clicking = false;
        }
    }

    /// <summary>
    /// This is the only non-static class. It's the class that calls into
    /// the virtual mouse class to set the click
    /// </summary>
    class ClickThread
    {

        public void StartClickingAt()
        {
            while (WindowsClicker.clicking)
            {
                Thread.Sleep(WindowsClicker.delay);
                VirtualMouse.LeftClick(WindowsClicker.x, WindowsClicker.y);
            }
        }
    }

    /// <summary>
    /// This is the virtual mouse class... it's from the internet somewhere.
    /// </summary>
    class VirtualMouse
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [Flags]
        public enum MouseEventFlags : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            Absolute = 0x8000
        }

        public static void MouseEvent(MouseEventFlags e, uint x, uint y)
        {
            mouse_event((uint)e, x, y, 0, UIntPtr.Zero);
        }

        public static void LeftClick(double x, double y)
        {
            var scr = Screen.PrimaryScreen.Bounds;
            MouseEvent(MouseEventFlags.LeftDown | MouseEventFlags.LeftUp | MouseEventFlags.Move | MouseEventFlags.Absolute,
                (uint)Math.Round(x / scr.Width * 65535),
                (uint)Math.Round(y / scr.Height * 65535));
        }

        public static void LeftClick(int x, int y)
        {
            LeftClick((double)x, (double)y);
        }
    }

    /// <summary>
    /// This is the virtual keyboard class that is from the internet somewhere too
    /// </summary>
    class VirtualKeyboard
    {
        private static int _hookHandle = 0;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int nVirtKey);

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        public const int WH_KEYBOARD_LL = 13;

        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;

        public static Label l;
        public static TextBox t;

        public static void SetHook(Label l1, TextBox t1)
        {
            l = l1;
            t = t1;
            // Set system-wide hook.
            _hookHandle = SetWindowsHookEx(
                WH_KEYBOARD_LL,
                KbHookProc,
                (IntPtr)0,
                0);
        }

        private static int KbHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = (KbLLHookStruct)Marshal.PtrToStructure(lParam, typeof(KbLLHookStruct));

                // Quick and dirty check. You may need to check if this is correct. See GetKeyState for more info.
                bool ctrlDown = GetKeyState(VK_LCONTROL) != 0 || GetKeyState(VK_RCONTROL) != 0;

                if (ctrlDown && hookStruct.vkCode == 0x46) // Ctrl+F
                {
                    //Call Method to Start
                    l.Text = "Started Clicking!";
                    WindowsClicker.delay = Convert.ToInt32(t.Text);
                    WindowsClicker.StartClick();
                    
                }
                if (ctrlDown && hookStruct.vkCode == 0x47) // Ctrl+G
                {
                    //Call Method to Stop
                    l.Text = "Stopped Clicking!";
                    WindowsClicker.StopClick();
                   
                }
            }

            // Pass to other keyboard handlers. Makes the Ctrl+V pass through.
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        public static void StopMonitoring()
        {
            UnhookWindowsHookEx(_hookHandle);
        }

        //Declare the wrapper managed MouseHookStruct class.
        [StructLayout(LayoutKind.Sequential)]
        public class KbLLHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
    }
}
