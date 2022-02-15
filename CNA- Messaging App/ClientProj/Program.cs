using System;

namespace ClientProj
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Client client = new Client();
            if (client.TCPConnect("127.0.0.1", 4444))
            {
                client.TCPRun();
            }
            else
            {
                Console.WriteLine("Failed to connect to the server.");
            }
        }
    }
}
