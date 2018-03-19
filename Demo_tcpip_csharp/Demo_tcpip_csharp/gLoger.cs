using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Demo_tcpip_csharp
{
    class gLoger
    {
        static Mutex g_LogMutex = new Mutex();
        static string g_strLogPath = System.Environment.CurrentDirectory;
        [STAThread]
        public static void WriteLog(string log)
        {          

            try
            {  
                g_LogMutex.WaitOne();
                string strFileName = string.Format("WathchDog_{0}.log", DateTime.Now.ToString("yyyy_MM_dd"));
                string strLogPath = g_strLogPath + "\\log\\";
                if (!Directory.Exists(strLogPath))
                {
                    Directory.CreateDirectory(strLogPath);
                }
                StreamWriter sWriter = new StreamWriter(strLogPath + strFileName, true, Encoding.GetEncoding("gb2312"));

                string strLog = string.Format("[{0}]: {1}", DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"), log);
                sWriter.WriteLine(strLog);
                sWriter.Close();
                System.Console.WriteLine(strLog);
            }
            catch (Exception ex)
            {                
                Console.WriteLine(ex.Message);                
            }
            finally
            {
                g_LogMutex.ReleaseMutex();
            }            
        }
    }
}
