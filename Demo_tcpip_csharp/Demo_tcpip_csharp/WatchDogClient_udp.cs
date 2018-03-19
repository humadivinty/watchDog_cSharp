using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Demo_tcpip_csharp;

namespace Demo_tcpip_csharp
{
    class WatchDogClient_udp
    {
        private string m_strSeverIP;
        private int m_iServerPort;
        private Socket m_socket = null;

        public WatchDogClient_udp(string serverIP, int port)
        {
            m_strSeverIP = serverIP;
            m_iServerPort = port;
        }

        public bool SendFeedDogMessage(byte[] msg)
        {
            bool bRet = false;
            try
            {
                IPAddress ip = IPAddress.Parse(m_strSeverIP);
                IPEndPoint serverEndpoint = new IPEndPoint(ip, m_iServerPort);
                if (m_socket == null)
                {
                    m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    int iSendLength = m_socket.SendTo(msg, serverEndpoint);
                    Console.WriteLine("udp feed dog , send {0}, length = {1}", Encoding.ASCII.GetString(msg, 0, iSendLength), iSendLength);
                    bRet = true;
                }
                else
                {
                    Console.WriteLine("the socket is no null, close it first.");
                }
            }
            catch (Exception ex)
            {
                gLoger.WriteLog(ex.ToString());
                bRet = false;
            }
            finally
            {
                if (m_socket != null)
                {
                    m_socket.Close();
                }                
            }
            return bRet;
        }
    }
}
