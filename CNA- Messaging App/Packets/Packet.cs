using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Packets
{
    public enum PacketType
    {
        Chat_Message,
        Private_Message,
        Client_Name,
        Login,
        Help,
        Terminate
    }

    [Serializable]
    public class Packet
    {
        public PacketType packetType { get ; protected set; }
    }

    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string _message;

        public ChatMessagePacket(string message)
        {
            _message = message;
            packetType = PacketType.Chat_Message;
        }
    }

    [Serializable]
    public class NamePacket : Packet
    {
        public string _message;

        public NamePacket(string message)
        {
            _message = message;
            packetType = PacketType.Client_Name;

        }
    }

    [Serializable]
    public class LoginPacket : Packet
    {
        public IPEndPoint _endPoint;

        public LoginPacket(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
            packetType = PacketType.Login;

        }
    }

    [Serializable]
    public class HelpPacket : Packet
    {
        public string _message;

        public HelpPacket()
        {
            packetType = PacketType.Help;

        }
    }

    [Serializable]
    public class TerminatePacket : Packet
    {
        public string _message;

        public TerminatePacket()
        {
            packetType = PacketType.Terminate;

        }
    }
}