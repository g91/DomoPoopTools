using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, 8080);
            listener.Start();
            Console.WriteLine("Server started. Waiting for clients to connect...");

            ConcurrentDictionary<string, Client> clients = new ConcurrentDictionary<string, Client>();
            int clientCount = 0;

            Task.Factory.StartNew(() => ReadCommandsAsync(clients));

            while (true)
            {
                TcpClient tcpClient = listener.AcceptTcpClient();
                NetworkStream stream = tcpClient.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string name = Encoding.UTF8.GetString(buffer, 0, bytesRead).Substring(6);

                Client client = new Client
                {
                    Name = name,
                    Stream = stream,
                };
                clients[name] = client;
                clientCount++;

                Console.WriteLine($"Client {name} connected. Number of connected clients: {clientCount}");
                byte[] helloMessage = Encoding.UTF8.GetBytes("Hello " + name);
                stream.Write(helloMessage, 0, helloMessage.Length);

                Task.Factory.StartNew(() => ReadDataAsync(client, clients));
            }
        }

        private static async void ReadCommandsAsync(ConcurrentDictionary<string, Client> clients)
        {
            while (true)
            {
                string command = Console.ReadLine();
                if (command.StartsWith("send "))
                {
                    string[] splitCommand = command.Split(' ');
                    string targetName = splitCommand[1];
                    string sendMessage = command.Substring(splitCommand[0].Length + targetName.Length + 2);
                    if (clients.ContainsKey(targetName))
                    {
                        SendMessage(targetName, "Server: " + sendMessage, clients);
                    }
                    else
                    {
                        Console.WriteLine("Error: Client not found.");
                    }
                }
                else if (command.StartsWith("broadcast "))
                {
                    string broadcastMessage = "Server: " + command.Substring(10);
                    BroadcastMessage(broadcastMessage, clients);
                }
                else if (command == "list")
                {
                    Console.Write("Connected clients: ");
                    foreach (var c in clients)
                    {
                        Console.Write(c.Key + " ");
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Error: Invalid command.");
                }
            }
        }

        private static void SendMessage(string targetName, string message, ConcurrentDictionary<string, Client> clients)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            clients[targetName].Stream.Write(data, 0, data.Length);
        }

        private static void BroadcastMessage(string message, ConcurrentDictionary<string, Client> clients)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (var client in clients.Values)
            {
                client.Stream.Write(data, 0, data.Length);
            }
        }

        private static async void ReadDataAsync(Client client, ConcurrentDictionary<string, Client> clients)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await client.Stream.ReadAsync(buffer, 0, buffer.Length);
                HandlePacket(buffer, bytesRead, client, clients);
            }
        }

        private static void HandlePacket(byte[] buffer, int bytesRead, Client client, ConcurrentDictionary<string, Client> clients)
        {
            int packetId = BitConverter.ToInt32(buffer, 0);
            int size = BitConverter.ToInt32(buffer, 4);
            int type = BitConverter.ToInt32(buffer, 8);
            byte[] payload = new byte[size - 12];
            Array.Copy(buffer, 12, payload, 0, size - 12);

            switch (packetId)
            {
                case 1:
                    HandleHelloPacket(client, payload);
                    break;
                case 2:
                    HandleBroadcastPacket(client, payload, clients);
                    break;
                default:
                    Console.WriteLine($"Error: Invalid packet ID. Packet ID: {packetId}");
                    break;
            }
        }

        private static void HandleHelloPacket(Client client, byte[] payload)
        {
            string name = Encoding.UTF8.GetString(payload);
            Console.WriteLine($"Client {client.Name} sent a hello packet. Name: {name}");
        }

        private static void HandleBroadcastPacket(Client client, byte[] payload, ConcurrentDictionary<string, Client> clients)
        {
            string message = $"{client.Name}: {Encoding.UTF8.GetString(payload)}";
            BroadcastMessage(message, clients);
        }

        class Client
        {
            public string Name { get; set; }
            public NetworkStream Stream { get; set; }
        }
    }
}
