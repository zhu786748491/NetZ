using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace TcpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 41125);
            clientSocket.Connect(serverIp);
            byte[] data = new byte[1024];
            int count = clientSocket.Receive(data);
            string msg = Encoding.UTF8.GetString(data, 0, count);
            Console.WriteLine(msg);


            while (true)
            {
                string s = Console.ReadLine();
                clientSocket.Send(Encoding.UTF8.GetBytes(s));

            }

            Console.ReadKey();
            clientSocket.Close();
        }
    }
}
