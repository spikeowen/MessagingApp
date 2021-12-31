using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using Packets;

namespace ServerProj
{
    class ServerClass
    {
        TcpListener m_TcpListener;
        ConcurrentDictionary<int, ConnectedClient> m_Clients;
        string[] m_Names = new string[10];
        public ServerClass(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            m_TcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            m_Clients = new ConcurrentDictionary<int, ConnectedClient>();
            int clientIndex = 0;
            m_TcpListener.Start();

            while (clientIndex <= 10)
            {
                Console.WriteLine("Listening.....");
                Socket socket = m_TcpListener.AcceptSocket();
                Console.WriteLine("Connection Made.");
                ConnectedClient m_ConnectedClient = new ConnectedClient(socket);
                int index = clientIndex;
                clientIndex++;
                m_Clients.TryAdd(index, m_ConnectedClient);

                Thread thread = new Thread(() => { ClientMethod(index); });
                thread.Start();
            }
        }

        public void Stop()
        {
            m_TcpListener.Stop();
        }

        private void ClientMethod(int index)
        {
            ChatMessagePacket messageT = new ChatMessagePacket("You have connected to the Server - send 0 for valid options.");
            Packet recievedMessage;

            //m_Clients[index].Send("You have connected to the Server - send 0 for valid options.");

            m_Clients[index].Send(messageT);

            while((recievedMessage = m_Clients[index].Read()) != null)
            {
                if (recievedMessage != null)
                {
                    switch (recievedMessage.packetType)
                    {
                        case PacketType.Chat_Message:
                            ChatMessagePacket chatPacket = (ChatMessagePacket)recievedMessage;
                            m_Clients[index].Send(new ChatMessagePacket(GetReturnMessage(chatPacket._message)));
                            break;
                        case PacketType.Client_Name:
                            NamePacket namePacket = (NamePacket)recievedMessage;
                            m_Names[index] = namePacket._message;
                            //m_Clients[index].Send(new ChatMessagePacket(("To " + namePacket._message + ": ")));
                            break;
                    }
                }
            }

            m_Clients[index].Close();
            ConnectedClient c;
            m_Clients.TryRemove(index, out c);
        }



        private string GetReturnMessage(string recievedMessage)
        {
            if (recievedMessage.ToLower() == "hi")
            {
                return "Hello";
            }
            else if (recievedMessage == "0")
            {
                return "Say 'Hi' to me! Say 'RPS' to play Rock Paper Scissors! T Or 'Bye' to Exit";
            }
            else if (recievedMessage.ToLower() == "bye")
            {
                return "Bye then!";
            }
            else if (recievedMessage.ToLower() == "rps")
            {
                return "Rock, Paper Scissors Time!";
            }
            else
            {
                return "What?";
            }
        }

        //private void Announcement(int index)
        //{
        //    for (int i = 0; i <= m_Clients.Count; i++)
        //    {
        //        m_Clients[i].Send("Test Announcement: WELCOME");
        //    }
        //}
    }
}
