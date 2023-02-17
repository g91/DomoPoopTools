using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Packet
{
    public int id;
    public int type;
    public int size;
    public byte[] payload;
}

class PacketHandler
{
    public delegate void Handler(Socket client, Packet packet);

    private readonly ConcurrentDictionary<int, Handler> _handlers = new ConcurrentDictionary<int, Handler>();

    public void Register(int packetId, Handler handler)
    {
        _handlers[packetId] = handler;
    }

    public async Task HandlePacket(Socket client, Packet packet)
    {
        if (!_handlers.TryGetValue(packet.id, out Handler handler))
        {
            Console.WriteLine($"Unknown packet id: {packet.id}");
            return;
        }

        await Task.Run(() => handler(client, packet));
    }
}

class HelloPacket
{
    public int connectionId;
    public int socketId;
    public string name;
}

class MessagePacket
{
    public int recipientId;
    public string message;
}

class Program
{
    static async Task Main(string[] args)
    {
        // Create a TCP/IP socket to listen for incoming connections
        var listener = new TcpListener(IPAddress.Any, 1234);
        listener.Start();
        Console.WriteLine("Server started, listening on port 1234...");

        // Create a packet handler to handle incoming packets
        var packetHandler = new PacketHandler();

        // Register a handler for the "hello" packet
        packetHandler.Register(1, (client, packet) =>
        {
            var helloPacket = new HelloPacket();

            // Read the connection ID, socket ID, and name from the payload
            var buffer = packet.payload;
            helloPacket.connectionId = BitConverter.ToInt32(buffer, 0);
            helloPacket.socketId = BitConverter.ToInt32(buffer, 4);
            helloPacket.name = Encoding.UTF8.GetString(buffer, 8, packet.size - 8);

            Console.WriteLine($"Received hello packet from {helloPacket.name} (connection ID: {helloPacket.connectionId}, socket ID: {helloPacket.socketId})");

            // Send a welcome message to the client
            var welcomeMessage = "Welcome to the server!";
            var welcomePacket = new Packet
            {
                id = 2,
                type = 1,
                size = Encoding.UTF8.GetByteCount(welcomeMessage),
                payload = Encoding.UTF8.GetBytes(welcomeMessage)
            };

            SendPacket(client, welcomePacket);
        });

        // Register a handler for the "message" packet
        packetHandler.Register(3, (client, packet) =>
        {
            var messagePacket = new MessagePacket();

            // Read the recipient ID and message from the payload
            var buffer = packet.payload;
            messagePacket.recipientId = BitConverter.ToInt32(buffer, 0);
            messagePacket.message = Encoding.UTF8.GetString(buffer, 4, packet.size - 4);

            Console.WriteLine($"Received message packet to client ID {messagePacket.recipientId}: {messagePacket.message}");

            // Send the message to the recipient
            SendPacket(GetClientById(messagePacket.recipientId), packet);
        });

        // Start a thread to read server commands from the console and broadcast them to all connected clients
        var commandThread = new Thread(() =>
        {
            while (true)
            {
                var command = Console.ReadLine();

                if (command == "exit")
                {
                    Environment.Exit(0);
                }

                if (command.StartsWith("msg "))
                {
                    var parts = command.Split(' ', 3);
                    if (parts.Length < 3)
                    {
                        Console.WriteLine("Invalid command: msg <client ID> <message>");
                        continue;
                    }

                    if (!int.TryParse(parts[1], out int recipientId))
                    {
                        Console.WriteLine("Invalid recipient ID");
                        continue;
                    }

                    var recipient = GetClientById(recipientId);

                    if (recipient == null)
                    {
                        Console.WriteLine("Invalid recipient ID");
                        continue;
                    }

                    var message = parts[2];
                    var messagePacket = new Packet
                    {
                        id = 3,
                        type = 1,
                        size = sizeof(int) + Encoding.UTF8.GetByteCount(message),
                        payload = new byte[sizeof(int) + Encoding.UTF8.GetByteCount(message)]
                    };
                    BitConverter.GetBytes(recipientId).CopyTo(messagePacket.payload, 0);
                    Encoding.UTF8.GetBytes(message).CopyTo(messagePacket.payload, sizeof(int));

                    SendPacket(recipient, messagePacket);
                }

                if (command.StartsWith("list"))
                {
                    Console.WriteLine("Connected clients:");
                    foreach (var client in clients.Values)
                    {
                        Console.WriteLine($"  - {client.RemoteEndPoint}");
                    }
                }

                if (command.StartsWith("help"))
                {
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("  - msg <client ID> <message>: Send a message to a specific client");
                    Console.WriteLine("  - list: List connected clients");
                    Console.WriteLine("  - help: Show this help message");
                    Console.WriteLine("  - exit: Exit the server");
                }
            }
        });



        // Start a thread to accept new client connections and handle incoming packets from connected clients
        while (true)
        {
            var client = await listener.AcceptSocketAsync();
            Console.WriteLine($"Client connected: {client.RemoteEndPoint}");

            clients.TryAdd(client.RemoteEndPoint.ToString(), client);

            var clientThread = new Thread(() =>
            {
                while (client.Connected)
                {
                    var headerBuffer = new byte[12];
                    var bytesRead = client.Receive(headerBuffer);

                    if (bytesRead == 0)
                    {
                        // Client disconnected
                        break;
                    }

                    var packet = new Packet
                    {
                        id = BitConverter.ToInt32(headerBuffer, 0),
                        type = BitConverter.ToInt32(headerBuffer, 4),
                        size = BitConverter.ToInt32(headerBuffer, 8),
                        payload = new byte[BitConverter.ToInt32(headerBuffer, 8)]
                    };

                    bytesRead = client.Receive(packet.payload);

                    if (bytesRead != packet.size)
                    {
                        Console.WriteLine("Received incomplete packet");
                        break;
                    }

                    packetHandler.HandlePacket(client, packet).Wait();
                }

                clients.TryRemove(client.RemoteEndPoint.ToString(), out _);
                Console.WriteLine($"Client disconnected: {client.RemoteEndPoint}");
            });
            clientThread.Start();
        }
    }

    private static readonly ConcurrentDictionary<string, Socket> clients = new ConcurrentDictionary<string, Socket>();

    private static void SendPacket(Socket client, Packet packet)
    {
        var headerBuffer = new byte[12];
        BitConverter.GetBytes(packet.id).CopyTo(headerBuffer, 0);
        BitConverter.GetBytes(packet.type).CopyTo(headerBuffer, 4);
        BitConverter.GetBytes(packet.size).CopyTo(headerBuffer, 8);

        client.Send(headerBuffer);
        client.Send(packet.payload);
    }

    private static Socket GetClientById(int id)
    {
        foreach (var client in clients.Values)
        {
            var idPacket = new Packet
            {
                id = 5,
                type = 1,
                size = sizeof(int),
                payload = BitConverter.GetBytes(id)
            };

            SendPacket(client, idPacket);

            var buffer = new byte[12];
            var bytesRead = client.Receive(buffer);

            if (bytesRead == 0)
            {
                // Client disconnected
                continue;
            }

            var packet = new Packet
            {
                id = BitConverter.ToInt32(buffer, 0),
                type = BitConverter.ToInt32(buffer, 4),
                size = BitConverter.ToInt32(buffer, 8),
                payload = new byte[BitConverter.ToInt32(buffer, 8)]
            };

            bytesRead = client.Receive(packet.payload);

            if (bytesRead != packet.size)
            {
                Console.WriteLine("Received incomplete packet");
                continue;
            }

            if (packet.id == 5 && packet.type == 1 && packet.size == sizeof(int) && BitConverter.ToInt32(packet.payload, 0) == id)
            {
                return client;
            }
        }

        return null;
    }


    private static async Task Heartbeat(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Send a heartbeat to all clients to check if they are online
            foreach (var client in clients.Values)
            {
                var heartbeatPacket = new Packet
                {
                    id = 4,
                    type = 1,
                    size = 0,
                    payload = new byte[0]
                };

                SendPacket(client, heartbeatPacket);
            }

            // Wait for a short time before checking again
            await Task.Delay(5000, cancellationToken);
        }
    }

    private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private static void ExitHandler(object sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Exiting...");

        cancellationTokenSource.Cancel();
        args.Cancel = true;
    }


    private static async Task HandleClient(Socket client, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && client.Connected)
        {
            var headerBuffer = new byte[12];
            var bytesRead = await client.ReceiveAsync(headerBuffer, SocketFlags.None, cancellationToken);

            if (bytesRead == 0)
            {
                // Client disconnected
                break;
            }

            var packet = new Packet
            {
                id = BitConverter.ToInt32(headerBuffer, 0),
                type = BitConverter.ToInt32(headerBuffer, 4),
                size = BitConverter.ToInt32(headerBuffer, 8),
                payload = new byte[BitConverter.ToInt32(headerBuffer, 8)]
            };

            bytesRead = await client.ReceiveAsync(packet.payload, SocketFlags.None, cancellationToken);

            if (bytesRead != packet.size)
            {
                Console.WriteLine("Received incomplete packet");
                break;
            }

            await packetHandler.HandlePacket(client, packet);
        }

        clients.TryRemove(client.RemoteEndPoint.ToString(), out _);
        Console.WriteLine($"Client disconnected: {client.RemoteEndPoint}");
    }




}