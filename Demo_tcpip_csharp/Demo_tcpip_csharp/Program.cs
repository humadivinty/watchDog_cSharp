#define TEST_DEAMON_PROCESS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Demo_TcpClient_csharp;
using System.Diagnostics;

namespace Demo_tcpip_csharp
{
    class Program
    {
        static Socket socketServer;
        private static int myProt = 8885;   //端口  
        private static byte[]  btRevBuffer= new byte[1024];
        static Mutex g_mutex =new Mutex();
        static int g_iDogFood = 0;
        static int g_iMaxFood = 0;

        static void Main(string[] args)
        {
            string strIniPath = System.Environment.CurrentDirectory + "\\WatchDog.ini";
            string strLog1 = string.Format("Get ini file path = {0}.", strIniPath);
            gLoger.WriteLog(strLog1);

            string strPort = ConfigerHelper.ReadIniData("WatchDog", "listenPort", "8885", strIniPath);
            Console.WriteLine("listen port = {0}", strPort);
            myProt = Convert.ToInt32(strPort);

            string strValue = ConfigerHelper.ReadIniData("WatchDog", "MaxFood", "30", strIniPath);
            string strLog = string.Format("Watch dog start, MaxFood = {0}.", strValue);
            gLoger.WriteLog(strLog);

            g_iMaxFood = Convert.ToInt32(strValue);
            g_iDogFood = g_iMaxFood;

            //看门狗线程
            //Thread watchDogThread = new Thread(FuncWatchDog);
            //watchDogThread.Start();

            //------------------tcp test----------------
            //////创建线程监听
            //Thread listenThread = new Thread(ThreadStartFunc_TcpServer);
            //listenThread.Start();

            //Thread clientThread = new Thread(FuncClient);
            //clientThread.Start();

            //WatchDogClient_tcp dogClient = new WatchDogClient_tcp("127.0.0.1", 8005);
            //dogClient.FeedDog(Encoding.ASCII.GetBytes("client Say Hello"));


            //--------------udp client test--------------
            //WatchDogClient_udp dogClient = new WatchDogClient_udp("127.0.0.1", 8885);
            //dogClient.SendFeedDogMessage(Encoding.ASCII.GetBytes("client Say Hello"));

            //------------udp Server test----------------
            //Thread listenThread = new Thread(ThreadStartFunc_UdpServer);
            //listenThread.Start();
            
#if(TEST_DEAMON_PROCESS)
            gLoger.WriteLog(string.Format("启动程序,pid={0}", Process.GetCurrentProcess().Id));
            DaemonControler Deamon = new DaemonControler();
            if (!Deamon.isDeamonExist())
            {
                Deamon.StartDeamon();
            }
            gLoger.WriteLog(string.Format("当前进程,pid={0}", Process.GetCurrentProcess().Id));

            if (DaemonControler.GetCurrentProcessSum() != 2)
            {
                gLoger.WriteLog(string.Format("当前进程数大于2，不重复执行函数,pid={0}", Process.GetCurrentProcess().Id));
                //return;
            }
            else
            {
                //看门狗线程
                Thread watchDogThread = new Thread(FuncWatchDog);
                watchDogThread.Start();

                //------------udp Server test----------------
                Thread listenThread = new Thread(ThreadStartFunc_UdpServer);
                listenThread.Start();
            }

#else
            //看门狗线程
            Thread watchDogThread = new Thread(FuncWatchDog);
            watchDogThread.Start();

             //------------udp Server test----------------
                Thread listenThread = new Thread(ThreadStartFunc_UdpServer);
                listenThread.Start();
#endif

            Console.WriteLine("main end.pid{0}", Process.GetCurrentProcess().Id);
        }


        public static void FuncWatchDog()
        {
            while (true)
            {
                if (g_mutex.WaitOne(1000))
                {
                    g_iDogFood = (g_iDogFood > 0) ? (g_iDogFood - 1) : 0;

                    if (g_iDogFood == 0)
                    {    
                        string strIniPath = System.Environment.CurrentDirectory + "\\WatchDog.ini";
                        Console.WriteLine("{0}.", strIniPath);

                        string strValue = ConfigerHelper.ReadIniData("WatchDog", "batFilePath", "./test.bat", strIniPath);
                        string strLog = string.Format("watch dog is hungry, restart program {0}", strValue);
                        gLoger.WriteLog(strLog);

                        if (File.Exists(strValue))
                        {
                            //存在文件
                            System.Diagnostics.Process p = new System.Diagnostics.Process();
                            p.StartInfo.FileName = "cmd.exe";
                            string strBat = string.Format("/c {0}", strValue);
                            p.StartInfo.Arguments = strBat;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.CreateNoWindow = true;
                            p.Start();
                        }
                        else
                        {
                            string strLog2 = string.Format("program {0} is not exisit, start failed.", strValue);
                            gLoger.WriteLog(strLog2);
                        }
                        g_iDogFood = g_iMaxFood;
                    }

                    g_mutex.ReleaseMutex();
                }
                Thread.Sleep(1000);
            }
        }

        public static void ThreadStartFunc_TcpServer()
        {
            try
            {
                IPAddress ip = IPAddress.Any;
                IPEndPoint point = new IPEndPoint(ip, myProt);
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketServer.Bind(point);
                socketServer.Listen(10);

                Console.WriteLine("begin the ThreadStartFunc.");
                while (true)
                {
                    Socket clientSocket = socketServer.Accept();
                    clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
                    Console.WriteLine("accept an new client socket, create a new thread to process it.");
                    //Thread receiveThread = new Thread(ThreadProcClientSocket);
                    //receiveThread.Start(clientSocket);
                    FuncProcClientSocket_Tcp(clientSocket);
                    Console.WriteLine("finish create thread.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (socketServer != null)
                {
                    socketServer.Shutdown(SocketShutdown.Both);
                    socketServer.Close();
                    socketServer = null;
                }
            }
        }

        public static void ThreadStartFunc_UdpServer()
        {
            try
            {
                IPAddress ip = IPAddress.Any;
                IPEndPoint Sendpoint = new IPEndPoint(ip, myProt);
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socketServer.Bind(Sendpoint);

                Console.WriteLine("begin the ThreadStartFunc udp.");
                while (true)
                {
                    EndPoint remoteEndPoint = (EndPoint)Sendpoint;
                    int iRevLeng = socketServer.ReceiveFrom(btRevBuffer, ref remoteEndPoint);
                    Console.WriteLine("receive message from {0}, length = {1}", remoteEndPoint.ToString(), iRevLeng);

                    if (iRevLeng > 0)
                    {
                        string strValue = Encoding.ASCII.GetString(btRevBuffer, 0, iRevLeng);
                        if (strValue.Contains("FeedDog"))
                        {
                            if (g_mutex.WaitOne(1000))
                            {
                                g_iDogFood = g_iMaxFood;
                                g_mutex.ReleaseMutex();
                            }
                            string strLog2 = string.Format("watch dog receive 'FeedDog' from  {0} .", remoteEndPoint.ToString());
                            gLoger.WriteLog(strLog2);
                        }
                        Console.WriteLine("msg = {0}",  strValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (socketServer != null)
                {
                    socketServer.Shutdown(SocketShutdown.Both);
                    socketServer.Close();
                    socketServer = null;
                }
            }
        }

        public static void FuncProcClientSocket_Tcp(object clientSocket)
        {
            Console.WriteLine("begin the ThreadProcClientSocket.");
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                if (myClientSocket == null)
                    break;
                try
                {
                    for (int i = 0; i < 1024; i++)
                    {
                        btRevBuffer[i] = 0;
                    }
                    int iReceiveLen = myClientSocket.Receive(btRevBuffer);

                    if (iReceiveLen == 0)
                    {
                        Console.WriteLine("Receive done from the client {0} ", myClientSocket.RemoteEndPoint.ToString());
                        break;
                    }
                    else
                    {
                        string strValue = Encoding.ASCII.GetString(btRevBuffer, 0, iReceiveLen);
                        if (strValue.Contains("FeedDog"))
                        {
                            if (g_mutex.WaitOne(1000))
                            {
                                g_iDogFood = g_iMaxFood;
                                g_mutex.ReleaseMutex();
                            }
                            string strLog2 = string.Format("watch dog receive 'FeedDog' from  {0} .", myClientSocket.RemoteEndPoint.ToString());
                            gLoger.WriteLog(strLog2);
                        }
                        Console.WriteLine("Receive from the client {0} , msg = {1}", myClientSocket.RemoteEndPoint.ToString(),  strValue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }

            myClientSocket.Shutdown(SocketShutdown.Both);
            myClientSocket.Close();
            Console.WriteLine("finish process client socket, close it.");
        }

        public static void FuncClient()
        {
            byte[] receiveBuffer = new byte[1024];

            string strIniPath = System.Environment.CurrentDirectory + "\\WatchDog.ini";
            string strLog1 = string.Format("Get ini file path = {0}.", strIniPath);
            gLoger.WriteLog(strLog1);

            string strIp = ConfigerHelper.ReadIniData("WatchDog", "serverIP", "127.0.0.1", strIniPath);

            string strPort = ConfigerHelper.ReadIniData("WatchDog", "listenPort", "8885", strIniPath);
            Console.WriteLine("listen port = {0}", strPort);
            int iClientPort = Convert.ToInt32(strPort);

            IPAddress ip = IPAddress.Parse(strIp);

            Socket clientSoct = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSoct.Connect(new IPEndPoint(ip, iClientPort));
                Console.WriteLine("connect sever {0}success.",  clientSoct.RemoteEndPoint.ToString());

                int iSendLen = clientSoct.Send(Encoding.ASCII.GetBytes(string.Format("hello server")));
                Console.WriteLine("send 'hello server' to server, length = {0}", iSendLen);

                int iReceiveLen = clientSoct.Receive(receiveBuffer);
                Console.WriteLine("receive '{0}' to server, length = {1}", Encoding.ASCII.GetString(receiveBuffer, 0, iReceiveLen), iReceiveLen);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                clientSoct.Shutdown(SocketShutdown.Both);      
            }
            finally
            {                 
                 clientSoct.Close();  
            }
        }
    }
}
