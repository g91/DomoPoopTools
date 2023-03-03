using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bobs_poop_Toolsx.Packet
{
    public class TcpProxyServer
    {
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly IPAddress _destinationAddress;
        private readonly int _destinationPort;
        private TcpListener _listener;
        private bool _isRunning;

        public TcpProxyServer(string listenAddress, int listenPort, string destinationAddress, int destinationPort)
        {
            _listenAddress = IPAddress.Parse(listenAddress);
            _listenPort = listenPort;
            _destinationAddress = IPAddress.Parse(destinationAddress);
            _destinationPort = destinationPort;
        }

        public void Start()
        {
            _listener = new TcpListener(_listenAddress, _listenPort);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine("Listening on {0}:{1}", _listenAddress, _listenPort);

            while (_isRunning)
            {
                TcpClient client = _listener.AcceptTcpClient();

                Thread t = new Thread(() =>
                {
                    try
                    {
                        HandleClient(client, _destinationAddress, _destinationPort);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error handling client: {0}", ex.Message);
                    }
                });
                t.Start();
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }

        private void HandleClient(TcpClient client, IPAddress destinationAddress, int destinationPort)
        {
            TcpClient destination = new TcpClient(destinationAddress.ToString(), destinationPort);

            using (NetworkStream clientStream = client.GetStream())
            using (NetworkStream destinationStream = destination.GetStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = clientStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Log the request data
                    string requestData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //Console.WriteLine("Request:\n{0}", requestData);

                    // Forward the request to the destination server
                    destinationStream.Write(buffer, 0, bytesRead);

                    // Receive the response from the destination server
                    int bytesReceived;
                    byte[] responseBuffer = new byte[4096];
                    do
                    {
                        bytesReceived = destinationStream.Read(responseBuffer, 0, responseBuffer.Length);
                        string responseData = Encoding.ASCII.GetString(responseBuffer, 0, bytesReceived);
                        //Console.WriteLine("Response:\n{0}", responseData);
                        clientStream.Write(responseBuffer, 0, bytesReceived);
                    } while (bytesReceived == responseBuffer.Length);
                }
            }

            destination.Close();
            client.Close();
        }
    }
}
