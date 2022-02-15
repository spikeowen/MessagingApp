using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Packets;

namespace ServerProj
{
    class ServerClass
    {
        TcpListener m_TcpListener;
        ConcurrentDictionary<int, ConnectedClient> m_Clients;
        string[] m_Names = new string[10];
        private UdpClient m_UDPListener;
        BinaryFormatter m_UDPFormatter;
        int clientIndex = 0;

        public ServerClass(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            m_TcpListener = new TcpListener(ip, port);
            m_UDPListener = new UdpClient(port);
        }

        public void TCPStart()
        {
            m_Clients = new ConcurrentDictionary<int, ConnectedClient>();
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

                Thread thread = new Thread(() => { TCPClientMethod(index); });
                thread.Start();

                Thread thread2 = new Thread(() => { UDPListen(); });
                thread2.Start();
            }
        }

        public void Stop()
        {
            m_TcpListener.Stop();
        }

        private void TCPClientMethod(int index)
        {
            ChatMessagePacket messageT = new ChatMessagePacket("You have connected to the Server - send 0 for me to tell you and everyone valid options, \nor press the help button for valid options to be sent privately.");
            Announcement(index);
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
                            foreach (ConnectedClient a in m_Clients.Values)
                            {
                                a.Send(new ChatMessagePacket("From " + m_Names[index] + " to all: " + chatPacket._message));
                                a.Send(new ChatMessagePacket("From Server to " + m_Names[index] + ": " + TCPGetReturnMessage(chatPacket._message)));
                            }
                            if (TCPGetReturnMessage(chatPacket._message) == "Bye then!")
                                m_Clients[index].Send(new TerminatePacket());
                            break;
                        case PacketType.Client_Name:
                            NamePacket namePacket = (NamePacket)recievedMessage;
                            m_Names[index] = namePacket._message;
                            //Console.WriteLine(m_Names[index]);
                            foreach (ConnectedClient b in m_Clients.Values)
                            {
                                b.Send(new NamePacket(namePacket._message));
                            }
                            break;
                        case PacketType.Help:
                            HelpPacket helpPacket = (HelpPacket)recievedMessage;
                            m_Clients[index].Send(new ChatMessagePacket("Hi there, if you want to talk to the Server it's commands are \n1)'hi' \n2)'rps' (This is to activate Rock, Paper, Scissors but isn't implemented yet)\n3)'bye' \nHappy Chatting!"));
                            break;
                    }
                }
            }

            m_Clients[index].Close();
            ConnectedClient c;
            m_Clients.TryRemove(index, out c);
        }



        private string TCPGetReturnMessage(string recievedMessage)
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

        private void UDPListen()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    byte[] buffer = m_UDPListener.Receive(ref endPoint);
                    MemoryStream UDPMemStream = new MemoryStream(buffer);
                    Packet packet = m_UDPFormatter.Deserialize(UDPMemStream) as Packet;
                    foreach (ConnectedClient c in m_Clients.Values)
                    {
                        if(endPoint.ToString() == c.endPoint.ToString())
                        {
                            m_UDPListener.Send(buffer, buffer.Length, c.endPoint);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("UDP Read Exception: " + e.Message);
            }
        }

        private void Announcement(int index)
        {
        
            foreach (ConnectedClient c in m_Clients.Values)
            {
                c.Send(new ChatMessagePacket("Announcement: NEW SERVER MEMBER HAS JOINED"));
            }  
         }
    }
}
