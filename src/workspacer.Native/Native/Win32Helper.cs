using System;
using System.Runtime.InteropServices;

namespace workspacer
{
    public static class Win32Helper
    {

        public static void QuitApplication(IntPtr hwnd)
        {
            Win32.SendNotifyMessage(hwnd, Win32.WM_SYSCOMMAND, Win32.SC_CLOSE, IntPtr.Zero);
        }

        public static bool IsCloaked(IntPtr hwnd)
        {
            bool isCloaked;
            var attr = Win32.DwmGetWindowAttribute(hwnd, (int)Win32.DwmWindowAttribute.DWMWA_CLOAKED, out isCloaked, Marshal.SizeOf(typeof(bool)));
            return isCloaked;
        }

        public static  bool IsAppWindow(IntPtr hwnd)
        {
            return Win32.IsWindowVisible(hwnd) &&
                   !Win32.GetWindowExStyleLongPtr(hwnd).HasFlag(Win32.WS_EX.WS_EX_NOACTIVATE) &&
                   !Win32.GetWindowStyleLongPtr(hwnd).HasFlag(Win32.WS.WS_CHILD);
        }

        // http://blogs.msdn.com/b/oldnewthing/archive/2007/10/08/5351207.aspx
        // http://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does
        public static bool IsAltTabWindow(IntPtr hWnd)
		{
			var exStyle = Win32.GetWindowExStyleLongPtr(hWnd);
			if (exStyle.HasFlag(Win32.WS_EX.WS_EX_TOOLWINDOW) ||
				Win32.GetWindow(hWnd, Win32.GW.GW_OWNER) != IntPtr.Zero)
			{
				return false;
			}
			if (exStyle.HasFlag(Win32.WS_EX.WS_EX_APPWINDOW))
			{
				return true;
			}

		    return true;
            // I am leaving this code here for testing purposes, but I don't think I need it.
            // the old-school alt-tab implementation clearly doesn't 100% line up with the aforementioned
            // blog post, or the below implementation in C# is wrong, because some windows are hidden when 
            // popups are created. For my purposes, I want to layout them anyway, so always return true;
		    /*
            // Start at the root owner
            var hWndTry = Win32.GetAncestor(hWnd, Win32.GA.GA_ROOTOWNER);
            IntPtr oldHWnd;

            // See if we are the last active visible popup
            do
            {
                oldHWnd = hWndTry;
                hWndTry = Win32.GetLastActivePopup(hWndTry);
            }
            while (oldHWnd != hWndTry && !Win32.IsWindowVisible(hWndTry));

            return hWndTry == hWnd;
            */
		}

        public static void ForceForegroundWindow(IntPtr hWnd)
        {
            FocusStealer.Steal(hWnd);
        }

        public static bool IsDebuggedWindow(int processId) 
        { 
            IntPtr processHandle = Win32.OpenProcess(Win32.ProcessAccessFlag.QueryInformation, false, processId);
            if (processHandle == IntPtr.Zero) 
            {
                return false;
            }

            bool result = false;

            bool test = Win32.CheckRemoteDebuggerPresent(processHandle, ref result);

            Win32.CloseHandle(processHandle);
            return result;
        }

        public static void BindWindowsHookEx(IConfigContext configContext, int hookType, Win32.HookProc lpfn, IntPtr hMod, int dwThreadId)
        {
            Win32.HookProc enqueueTask = (int code, UIntPtr wParam, IntPtr lParam) => {
                configContext.Tasks.QueueTask(new Action(()=>lpfn(code, wParam, lParam)));
                return 0;
            };
            Win32.SetWindowsHookEx(hookType, enqueueTask, hMod, dwThreadId);
        }

        public static void BindWindowsEventHook(IConfigContext configContext, Win32.EVENT_CONSTANTS eventMin, Win32.EVENT_CONSTANTS eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags)
        {
            WinEventDelegate enqueueTask = (IntPtr hWinEventHook, Win32.EVENT_CONSTANTS eventType, IntPtr hwnd, Win32.OBJID idObject, int idChild, uint dwEventThread, uint dwmsEventTime) =>
            {
                configContext.Tasks.QueueTask(new Action(() => lpfnWinEventProc(hWinEventHook, eventType, hwnd, idObject, idChild, dwEventThread, dwmsEventTime)));
            };
            Win32.SetWinEventHook(eventMin, eventMax, hmodWinEventProc, enqueueTask, idProcess, idThread, dwFlags);
        }

    }
}
