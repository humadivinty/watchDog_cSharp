using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Demo_tcpip_csharp;

namespace Demo_TcpClient_csharp
{
    class WatchDogClient_tcp
    {
        private string m_strSeverIP;
        private int m_iServerPort;
        private Socket m_socket;

        public WatchDogClient_tcp(string serverIP, int port)
        {
            m_strSeverIP = serverIP;
            m_iServerPort = port;
        }

        ~WatchDogClient_tcp()
        {
            CloseConnect();
       }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool FeedDog(byte[] msg)
        {
            try
            {
                if (ConnectToServer(ref m_socket, m_strSeverIP, m_iServerPort))
                {
                    int iSendlength = m_socket.Send(msg);
                    Console.WriteLine("feed dog , send {0}, length = {1}", Encoding.ASCII.GetString(msg, 0, iSendlength), iSendlength);
                }
                CloseConnect();
                return true;
            }
            catch (Exception ex)
            {
                gLoger.WriteLog(ex.ToString());
                CloseConnect();
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool ConnectToServer(ref Socket clientSocket, string ipaddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipaddress);
            m_iServerPort = port;
            IPEndPoint point = new IPEndPoint(ip, port);            
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(point);
                gLoger.WriteLog("Udp connect success.");
                return true;
            }
            catch (Exception ex)
            {
                gLoger.WriteLog(ex.ToString());
                return false;
            }            
        }

        void CloseConnect()
        {
            try
            {
                if (m_socket != null)
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                    m_socket.Close();
                    m_socket = null;
                }
            }
            catch (Exception ex)
            {
                gLoger.WriteLog(ex.ToString());
            }  
        }

    }
}
