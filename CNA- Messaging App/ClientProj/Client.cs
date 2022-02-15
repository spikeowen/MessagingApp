using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using Packets;

namespace ClientProj
{
   
    public class Client
    {
        TcpClient m_TcpClient;
        NetworkStream m_TCPStream;
        BinaryWriter m_TCPWriter;
        BinaryReader m_TCPReader;
        BinaryFormatter m_TCPFormatter;
        private CNA__Tut5.MainWindow ClientForm;
        private UdpClient m_UdpClient;
        NetworkStream m_UDPStream;
        BinaryWriter m_UDPWriter;
        BinaryReader m_UDPReader;
        BinaryFormatter m_UDPFormatter;

        public Client()
        {
            m_TcpClient = new TcpClient();
        }

        public bool TCPConnect(string ip, int port)
        {
            try
            {
                m_TcpClient.Connect(ip, port);
                m_TCPStream = m_TcpClient.GetStream();
                m_TCPWriter = new BinaryWriter(m_TCPStream, Encoding.UTF8);
                m_TCPReader = new BinaryReader(m_TCPStream, Encoding.UTF8);
                m_TCPFormatter = new BinaryFormatter();
                m_UdpClient = new UdpClient();
                m_UdpClient.Connect(ip, port);
                m_UDPWriter = new BinaryWriter(m_TCPStream, Encoding.UTF8);
                m_UDPReader = new BinaryReader(m_TCPStream, Encoding.UTF8);
                m_UDPFormatter = new BinaryFormatter();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public void TCPRun()
        {
            ClientForm = new CNA__Tut5.MainWindow(this);

            Thread thread1 = new Thread(new ThreadStart(this.TCPProcessServerResponse));
            thread1.Start();
            ClientForm.ShowDialog();

            Thread thread2 = new Thread(new ThreadStart(this.UdpProcessServerResponse));
            thread2.Start();
            Login();

            //string userInput;

            //ProcessServerResponse();

            //while ((userInput = Console.ReadLine()) != null)
            //{
            //    m_Writer.WriteLine(userInput);
            //    m_Writer.Flush();

            //    ProcessServerResponse();

            //    if (userInput.ToLower() == "bye")
            //    {
            //        break;
            //    }
            //}

            m_TcpClient.Close();
            m_UdpClient.Close();
        }

        private void TCPProcessServerResponse()
        {
            try
            {
                while (m_TcpClient.Connected)
                {
                    //Console.Write("Server says: ");
                    //Console.WriteLine(m_Reader.ReadLine());
                    //Console.WriteLine();
                    //ClientForm.UpdateChatBox("Server says: " + m_Reader.ReadLine() + "\n");

                    int numberOfBytes;

                    if ((numberOfBytes = m_TCPReader.ReadInt32()) != -1)
                    {
                        byte[] buffer = m_TCPReader.ReadBytes(numberOfBytes);
                        MemoryStream memStream = new MemoryStream(buffer);
                        Packet packet = m_TCPFormatter.Deserialize(memStream) as Packet;

                        switch (packet.packetType)
                        {
                            case PacketType.Chat_Message:
                                ChatMessagePacket chatPacket = (ChatMessagePacket)packet;
                                ClientForm.UpdateChatBox(chatPacket._message);
                                break;
                            case PacketType.Client_Name:
                                NamePacket namePacket = (NamePacket)packet;
                                ClientForm.UpdateUserList(namePacket._message);
                                break;
                            case PacketType.Terminate:
                                //ClientForm.Close();
                                break;
                        }
                     
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TCP Read Exception: " + e.Message);
            }
        }

        public void TCPSendMessage(Packet message)
        {
            ////m_Writer.WriteLine(name + " says: " + message);
            //m_Writer.WriteLine(message);
            //m_Writer.Flush();
            try
            {
                MemoryStream memStream = new MemoryStream();
                m_TCPFormatter.Serialize(memStream, message);
                byte[] buffer = memStream.GetBuffer();
                m_TCPWriter.Write(buffer.Length);
                m_TCPWriter.Write(buffer);
                m_TCPWriter.Flush();

            }
            catch (Exception e)
            {
                Console.WriteLine("TCP Write Exception: " + e.Message);
            }
        }

        public void Login()
        {
            TCPSendMessage(new LoginPacket((IPEndPoint)m_UdpClient.Client.LocalEndPoint));
        }

        public void UdpSendMessage(Packet packet)
        {
            try
            {
                MemoryStream UDPMemStream = new MemoryStream();
                m_UDPFormatter.Serialize(UDPMemStream, packet);
                byte[] buffer = UDPMemStream.GetBuffer();
                m_UDPWriter.Write(buffer.Length);
                m_UDPWriter.Write(buffer);
                m_UDPWriter.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("UDP Write Exception: " + e.Message);
            }
        }

        private void UdpProcessServerResponse()
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                while(true)
                {
                    byte[] buffer = m_UdpClient.Receive(ref endPoint);
                    MemoryStream UDPMemStream = new MemoryStream(buffer);
                    Packet packet = m_UDPFormatter.Deserialize(UDPMemStream) as Packet;

                    switch (packet.packetType)
                    {
                        case PacketType.Chat_Message:
                            ChatMessagePacket chatPacket = (ChatMessagePacket)packet;
                            ClientForm.UpdateChatBox(chatPacket._message);
                            break;
                        case PacketType.Client_Name:
                            NamePacket namePacket = (NamePacket)packet;
                            ClientForm.UpdateUserList(namePacket._message);
                            break;
                        case PacketType.Login:
                            ChatMessagePacket loginPacket = (ChatMessagePacket)packet;
                            ClientForm.UpdateChatBox(loginPacket._message);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("UDP Read Exception: " + e.Message);
            }
        }
    }
}
