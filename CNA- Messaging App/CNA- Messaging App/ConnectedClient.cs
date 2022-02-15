using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Packets;

namespace ServerProj
{
    class ConnectedClient
    {
        private Socket m_Socket;
        private NetworkStream m_Stream;
        private BinaryReader m_Reader;
        private BinaryWriter m_Writer;
        private BinaryFormatter formatter;
        private object m_ReadLock;
        private object m_WriteLock;
        public IPEndPoint endPoint;

        public ConnectedClient(Socket socket)
        {
            m_WriteLock = new object();
            m_ReadLock = new object();
            m_Socket = socket;
            m_Stream = new NetworkStream(m_Socket, true);
            m_Reader = new BinaryReader(m_Stream, Encoding.UTF8);
            m_Writer = new BinaryWriter(m_Stream, Encoding.UTF8);
            formatter = new BinaryFormatter();
        }

        public void Close()
        {
            m_Stream.Close();
            m_Writer.Close();
            m_Reader.Close();
            m_Socket.Close();
        }

        public Packet Read()
        {
            try
            {
                lock (m_ReadLock)
                {
                    int numberOfBytes;

                    if((numberOfBytes = m_Reader.ReadInt32()) != -1)
                    {
                        byte[] buffer = m_Reader.ReadBytes(numberOfBytes);
                        MemoryStream memStream = new MemoryStream(buffer);
                        return formatter.Deserialize(memStream) as Packet;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return null;
            }
        }

        public void Send(Packet message)
        {
            //try
            //{
            //    lock (m_WriteLock)
            //    {
            //        m_Writer.WriteLine(message);
            //        m_Writer.Flush();
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Exception: " + e.Message);
            //}
            try
            {
                lock (m_WriteLock)
                {
                    MemoryStream memStream = new MemoryStream();
                    formatter.Serialize(memStream, message);
                    byte[] buffer = memStream.GetBuffer();
                    m_Writer.Write(buffer.Length);
                    m_Writer.Write(buffer);
                    m_Writer.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}
