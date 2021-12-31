using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public enum PacketType
    {
        Chat_Message,
        Private_Message,
        Client_Name
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
}