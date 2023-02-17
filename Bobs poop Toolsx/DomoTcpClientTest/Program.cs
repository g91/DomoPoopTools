using System;
using System.Net.Sockets;
using System.Text;

namespace TCPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter client name:");
            string name = Console.ReadLine();

            TcpClient client = new TcpClient("127.0.0.1", 8080);
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            byte[] helloMessage = Encoding.UTF8.GetBytes("Hello " + name);
            stream.Write(helloMessage, 0, helloMessage.Length);
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            Console.WriteLine("Server: " + Encoding.UTF8.GetString(buffer, 0, bytesRead));

            int clientCount = 1;
            Console.WriteLine($"Number of connected clients: {clientCount}");

            Task.Factory.StartNew(() => ReadDataAsync(stream));

            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }
                if (message.StartsWith("send "))
                {
                    byte[] data = Encoding.UTF8.GetBytes("broadcast " + message.Substring(5));
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }

            stream.Close();
            client.Close();
            clientCount--;
            Console.WriteLine($"Number of connected clients: {clientCount}");
        }

        private static async void ReadDataAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                Console.WriteLine("Server: " + Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
        }
    }
}
