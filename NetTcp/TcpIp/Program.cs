using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            StartServerAsync();
            Console.ReadKey();
        }

        static void StartServerAsync()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 41125);
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(0);
            //Socket clientSocket = serverSocket.Accept();
            serverSocket.BeginAccept(AcceptCallBack, serverSocket);

        }

        static void AcceptCallBack(IAsyncResult ar)
        {
            Socket serverSocket = ar.AsyncState as Socket;
            Socket clientSocket = serverSocket.EndAccept(ar);
            string msg = "Hello client!你好";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);
            
            clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);

            serverSocket.BeginAccept(AcceptCallBack, serverSocket);

        }

        static byte[] dataBuffer = new byte[1024];
        static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket clientSocket = ar.AsyncState as Socket;
            int count = clientSocket.EndReceive(ar);
            string msg = Encoding.UTF8.GetString(dataBuffer, 0, count);
            Console.WriteLine("从客户端接受到数据"+msg);
            clientSocket.BeginReceive(dataBuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);
        }

        void StartServerSync()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 41125);
            serverSocket.Bind(ipEndPoint);//绑定ip和端口号
            serverSocket.Listen(10);//监听客户端的数量，0表示没有最大限制
            Socket clientSocket = serverSocket.Accept();//接受一个客户端连接

            //向客户端发送消息
            string msg = "Hello client!你好";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);

            //接受客户端的消息
            byte[] dataBuff = new byte[1024];
            int count = clientSocket.Receive(dataBuff);
            string msgReceive = Encoding.UTF8.GetString(dataBuff, 0, count);
            Console.WriteLine(msgReceive);


            Console.ReadKey();
            clientSocket.Close();
            serverSocket.Close();

        }
    }
}
