/*
    This code is taken and modified from:
    Great Blog Post: Low-Level Mouse Hook in C#, 
    written by Stephen Toub at
    http://blogs.msdn.com/b/toub/archive/2006/05/03/589468.aspx
 */

namespace DragOutsideWPFAppDemo
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows;

    /// <summary>
    ///     Handles windows mosue
    /// </summary>
    internal class InterceptMouse
    {
        //Hook and subscribe to the messages of whole Windows OS and filter only we are interesting in
        // low level mouse hook identifeir key
        private const int WH_MOUSE_LL = 14;

        // Window Messasage Left Button Down event indentifier key
        private const int WM_LBUTTONUP = 0x0202;

        /// <summary>
        ///     Callback method used with the SetWindowsHookEx function.
        ///     The system calls this method  every time a new mouse input event is about to be posted into a thread input queue.
        /// </summary>
        internal static LowLevelMouseProc m_proc = HookCallback;

        /// <summary>
        ///     Handle to a  hook
        /// </summary>
        internal static IntPtr m_hookID = IntPtr.Zero;

        /// <summary>
        ///     Indicate when the mouse is outside the application
        /// </summary>
        public static bool IsMouseOutsideApp { get; set; }

        /// <summary>
        ///     Hook and subscribe to the messages of whole Windows OS and filter
        /// </summary>
        /// <param name="proc">Callback method to call for messages</param>
        /// <returns>Handle to </returns>
        internal static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        /// <summary>
        ///     Event handler callback method that handles windows messages
        /// </summary>
        /// <param name="nCode">A code the hook procedure uses to determine how to process the messag</param>
        /// <param name="wParam">The identifier of the mouse message. </param>
        /// <param name="lParam">A pointer to an MSG structure. </param>
        /// <returns></returns>
        internal static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                //check if POint in main window
                var pt = new Point(hookStruct.pt.x, hookStruct.pt.y);
                var ptw = Application.Current.MainWindow.PointFromScreen(pt);
                var w = Application.Current.MainWindow.Width;
                var h = Application.Current.MainWindow.Height;
                //if point is outside MainWindow
                if (ptw.X < 0 || ptw.Y < 0 || ptw.X > w || ptw.Y > h) IsMouseOutsideApp = true;
                else IsMouseOutsideApp = false;
            }
            return CallNextHookEx(m_hookID, nCode, wParam, lParam);
        }

        internal delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

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
            public readonly int x;

            public readonly int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;

            public readonly uint mouseData;

            public readonly uint flags;

            public readonly uint time;

            public readonly IntPtr dwExtraInfo;
        }

        #region to  methods to handle windows operating messages

        /// <note>A 32-bit DLL cannot be injected into a 64-bit process, and a 64-bit DLL cannot be injected into a 32-bit process. </note>
        /// <summary>
        ///     Application-defined hook procedure into a hook chain. You would install a hook procedure to monitor the system for
        ///     certain types of events.
        ///     These events are associated either with a specific thread or with all threads in the same desktop as the calling
        ///     thread.
        /// </summary>
        /// <param name="idHook">The type of hook procedure to be installe</param>
        /// <param name="lpfn">A pointer to the hook procedure.</param>
        /// <param name="hMod">
        ///     A handle to the DLL containing the hook procedure pointed to by the lpfn parameter.
        ///     NULL if the dwThreadId parameter specifies a thread created by the current process and if the hook procedure is
        ///     within the code associated with the current process.
        /// </param>
        /// <param name="dwThreadId">
        ///     The identifier of the thread with which the hook procedure is to be associated.
        ///     For desktop apps, if this parameter is zero, the hook procedure is associated with all existing threads running in
        ///     the same desktop as the calling threa
        /// </param>
        /// <returns>Handle to hook procedure</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        ///     Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="hhk">
        ///     A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to
        ///     SetWindowsHookEx.
        /// </param>
        /// <returns> nonzero if function succedds othereise zero.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        ///     Passes the hook information to the next hook procedure in the current hook chain
        /// </summary>
        /// <param name="hhk">Identifies the current hook. This parameter is ignored</param>
        /// <param name="nCode">The hook code passed to the current hook procedure</param>
        /// <param name="wParam">The wParam value passed to the current hook procedure</param>
        /// <param name="lParam">The lParam value passed to the current hook procedure. </param>
        /// <returns>s The value returned by the next hook procedure in the chain.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        ///     Retrieves a module handle for the calling process that loaded.
        /// </summary>
        /// <param name="lpModuleName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}