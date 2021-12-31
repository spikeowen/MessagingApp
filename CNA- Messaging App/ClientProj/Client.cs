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
        NetworkStream m_Stream;
        BinaryWriter m_Writer;
        BinaryReader m_Reader;
        BinaryFormatter formatter;
        private CNA__Tut5.MainWindow ClientForm;

    
        public Client()
        {
            m_TcpClient = new TcpClient();
        }

        public bool Connect(string ip, int port)
        {
            try
            {
                m_TcpClient.Connect(ip, port);
                m_Stream = m_TcpClient.GetStream();
                m_Writer = new BinaryWriter(m_Stream, Encoding.UTF8);
                m_Reader = new BinaryReader(m_Stream, Encoding.UTF8);
                formatter = new BinaryFormatter();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public void Run()
        {
            ClientForm = new CNA__Tut5.MainWindow(this);

            Thread thread1 = new Thread(new ThreadStart(this.ProcessServerResponse));
            thread1.Start();
            ClientForm.ShowDialog();

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
        }

        private void ProcessServerResponse()
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

                    if ((numberOfBytes = m_Reader.ReadInt32()) != -1)
                    {
                        byte[] buffer = m_Reader.ReadBytes(numberOfBytes);
                        MemoryStream memStream = new MemoryStream(buffer);
                        Packet packet = formatter.Deserialize(memStream) as Packet;

                        switch (packet.packetType)
                        {
                            case PacketType.Chat_Message:
                                ChatMessagePacket chatPacket = (ChatMessagePacket)packet;
                                ClientForm.UpdateChatBox(chatPacket._message);
                                break;
                            case PacketType.Client_Name:
                                NamePacket namePacket = (NamePacket)packet;
                                ClientForm.UpdateNameChatBox(namePacket._message);
                                break;
                        }
                     
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        public void SendMessage(Packet message)
        {
            ////m_Writer.WriteLine(name + " says: " + message);
            //m_Writer.WriteLine(message);
            //m_Writer.Flush();
            try
            {
                MemoryStream memStream = new MemoryStream();
                formatter.Serialize(memStream, message);
                byte[] buffer = memStream.GetBuffer();
                m_Writer.Write(buffer.Length);
                m_Writer.Write(buffer);
                m_Writer.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}
