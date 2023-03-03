using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bobs_poop_Toolsx.Packet
{
    internal class ProxyPacketLogger
    {
        private TcpListener listener;
        private bool isLogging;
        private readonly string logFilePath;
        private readonly IPAddress proxyAddress;
        private int packetIndex;

        public ProxyPacketLogger(string logFilePath, IPAddress proxyAddress)
        {
            this.logFilePath = logFilePath;
            this.proxyAddress = proxyAddress;
            packetIndex = 0;
        }

        public void Start()
        {
            isLogging = true;
            listener = new TcpListener(IPAddress.Any, 80);
            listener.Start();
            Console.WriteLine("Packet logger started.");

            Task.Run(() =>
            {
                while (isLogging)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Task.Run(() => LogPacket(client));
                }
            });
        }

        private void LogPacket(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();
            byte[] clientBuffer = new byte[client.ReceiveBufferSize];
            int bytesRead = clientStream.Read(clientBuffer, 0, client.ReceiveBufferSize);

            using (FileStream fileStream = new FileStream($"logs\\client_packet_{packetIndex}.bin", FileMode.Create))
            {
                fileStream.Write(clientBuffer, 0, bytesRead);
            }

            packetIndex++;

            using (TcpClient proxyClient = new TcpClient())
            {
                proxyClient.Connect(proxyAddress, 80);

                NetworkStream proxyStream = proxyClient.GetStream();
                byte[] proxyBuffer = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(clientBuffer, 0, bytesRead));
                proxyStream.Write(proxyBuffer, 0, proxyBuffer.Length);

                byte[] responseBuffer = new byte[proxyClient.ReceiveBufferSize];
                int responseBytesRead = proxyStream.Read(responseBuffer, 0, proxyClient.ReceiveBufferSize);

                using (FileStream responseFileStream = new FileStream($"logs\\server_packet_{packetIndex}.bin", FileMode.Create))
                {
                    responseFileStream.Write(responseBuffer, 0, responseBytesRead);
                }

                packetIndex++;

                string responseData = Encoding.UTF8.GetString(responseBuffer, 0, responseBytesRead);

                proxyClient.Close();

                if (!string.IsNullOrWhiteSpace(responseData))
                {
                    string logMessage = $"[{DateTime.Now}] {responseData}{Environment.NewLine}";
                    Console.Write(logMessage);

                    using (var writer = new StreamWriter(logFilePath, true))
                    {
                        writer.Write(logMessage);
                    }
                }
            }

            client.Close();
        }

        public void Stop()
        {
            isLogging = false;
            listener.Stop();
            Console.WriteLine("Packet logger stopped.");
        }
    }
}
