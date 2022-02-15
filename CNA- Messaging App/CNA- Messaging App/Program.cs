using System;
using System.Net;
using System.Net.Sockets;

namespace ServerProj
{
    class Program
    {
        static void Main()
        {
            ServerClass server = new ServerClass("127.0.0.1", 4444);
            server.TCPStart();
            server.Stop();
        }
    }
}
