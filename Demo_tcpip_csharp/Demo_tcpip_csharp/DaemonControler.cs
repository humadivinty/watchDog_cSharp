using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace Demo_tcpip_csharp
{
    class DaemonControler
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateMutex(
         IntPtr lpMutexAttributes, // SD 
         int bInitialOwner,                       // initial owner 
         string lpName                            // object name 
         );
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr OpenMutex(
         uint dwDesiredAccess, // access 
         int bInheritHandle,    // inheritance option 
         string lpName          // object name 
         );
        [DllImport("Kernel32.Dll", CharSet = CharSet.Auto)]
        public static extern int ReleaseMutex(IntPtr hMutex);
        [DllImport("Kernel32.Dll", CharSet = CharSet.Auto)]
        public static extern int GetLastError();
        [DllImport("Kernel32.Dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string strLog);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);
        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        [DllImport("user32")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32")]
        public static extern int EnumWindows(CallBack x, int y);
        public delegate bool CallBack(IntPtr hWnd, int lParam);


        public DaemonControler()
        {
    
        }

        ~DaemonControler()
        {
            SetExit(true);
            gLoger.WriteLog(string.Format("exit DaemonControler,pid={0}", Process.GetCurrentProcess().Id));
        }

        private IntPtr m_hDeamonMutex = IntPtr.Zero;
        private string m_strMutexName = "DeamonDemo";
        public const int ERROR_ALREADY_EXISTS = 0183;
        private bool m_bExit = false;

        public bool isDeamonExist()
        {
            m_hDeamonMutex = CreateMutex(IntPtr.Zero, 1, m_strMutexName);
            if (GetLastError() == ERROR_ALREADY_EXISTS)
            {
                gLoger.WriteLog("Create deamon mutex failed, ERROR_ALREADY_EXISTS.");
                return true;
            }
            if (IntPtr.Zero == m_hDeamonMutex)
            {
                gLoger.WriteLog("Create deamon mutex failed.");
            }
            gLoger.WriteLog("Create deamon mutex success.");
            return false;
        }

        public void StartDeamon()
        {
            gLoger.WriteLog("start Deamon process.");
            try
            {
                while (!GetExit())
                {
                    if (!IsHaveChileProcess())
                    {
                        StarChildProcess();
                    }
                    System.Threading.Thread.Sleep(1000 * 3);
                }

                gLoger.WriteLog(string.Format("退出守护进程,pid={0}", Process.GetCurrentProcess().Id));
            }
            catch (Exception ex)
            {
                gLoger.WriteLog(string.Format("守护进程异常,pid={0}, err{1}", Process.GetCurrentProcess().Id, ex.ToString()));
            }
        }

        void StarChildProcess()
        {
            string strExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            gLoger.WriteLog(string.Format("开启子进程, path ={0}", strExePath));
            try
            {
                //Process pro = Process.Start(strExePath);
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = strExePath;
                //string strBat = string.Format("/c {0}", strValue);
                //p.StartInfo.Arguments = strBat;
                p.StartInfo.UseShellExecute = false;    //这个选项为true的时候，回创建新的窗口出来
                p.StartInfo.CreateNoWindow = false;     //这个选项为true的时候，子进程输出的内容在当前窗口不显示
                bool bRet = p.Start();
                p.WaitForExit();

                gLoger.WriteLog(string.Format("创建子进程返回值,pid={0}", bRet));
            }
            catch (Exception ex)
            {
                gLoger.WriteLog(string.Format("创建子进程异常,pid={0}, err{1}", Process.GetCurrentProcess().Id, ex.ToString()));
            }     
        }

        bool GetExit()
        {
            return m_bExit;
        }
        void SetExit(bool value)
        {
            m_bExit = value;
        }

        public static int GetCurrentProcessSum()
        {
            try
            {
                int iCount = 0;
                Process currentProcess = Process.GetCurrentProcess();
                string strCurrentFileName = currentProcess.MainModule.FileName;
                Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
                foreach (Process prcessTemp in processes)
                {
                    if (prcessTemp.MainModule.FileName.Contains(strCurrentFileName))
                    {
                        iCount++;
                    }
                }

                string strLog;
                strLog = string.Format("[GetCurrentProcessSum]当前进程文件名：{0},iSum={1}", strCurrentFileName, iCount);
                gLoger.WriteLog(strLog);
                return iCount;
            }
            catch (System.Exception ex)
            {
                string strMsg = "[GetCurrentProcessSum] 发生异常：";
                strMsg += ex.ToString();
                gLoger.WriteLog(strMsg);
                return 0;
            }
        }

        public bool IsHaveChileProcess()
        {
            if (GetCurrentProcessSum() <= 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
