using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class DnsServer
{
    static void Main()
    {
        // Specify the IP address to redirect all domains to
        IPAddress ipAddress = IPAddress.Parse("206.53.61.3");

        // Create a UDP socket to listen for incoming DNS requests
        UdpClient udpListener = new UdpClient(53);

        Console.WriteLine("DNS server started. Listening on port 53...");

        while (true)
        {
            // Receive a DNS request packet
            IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] request = udpListener.Receive(ref clientEndpoint);

            Console.WriteLine("Received a DNS request from {0}:{1}.", clientEndpoint.Address, clientEndpoint.Port);

            // Extract the domain name from the request packet
            string domainName = GetDomainName(request);

            Console.WriteLine("Requested domain: {0}", domainName);

            // Build a response packet with the specified IP address for the requested domain
            byte[] response = BuildResponsePacket(request, ipAddress);

            // Send the response packet back to the client
            udpListener.Send(response, response.Length, clientEndpoint);

            Console.WriteLine("Sent a DNS response to {0}:{1}.", clientEndpoint.Address, clientEndpoint.Port);
        }
    }

    // Extracts the domain name from a DNS request packet
    static string GetDomainName(byte[] request)
    {
        int length = request[5];
        StringBuilder domainName = new StringBuilder();

        for (int i = 12; i < 12 + length; i++)
        {
            domainName.Append((char)request[i]);
        }

        return domainName.ToString();
    }

    // Builds a DNS response packet with the specified IP address for the requested domain
    static byte[] BuildResponsePacket(byte[] request, IPAddress ipAddress)
    {
        byte[] response = new byte[request.Length];

        // Copy the request packet header to the response packet
        for (int i = 0; i < 12; i++)
        {
            response[i] = request[i];
        }

        // Set the response packet flags and counts
        response[2] = 0x81; // Response flag and recursion desired flag
        response[3] = 0x80; // Response code
        response[7] = 0x01; // Answer count

        // Build the answer record with the specified IP address
        int length = request[5];
        int offset = 12 + length + 4;
        response[offset++] = 0xc0; // Pointer to the domain name
        response[offset++] = 0x0c;
        response[offset++] = 0x00; // Type (A record)
        response[offset++] = 0x01;
        response[offset++] = 0x00; // Class (IN)
        response[offset++] = 0x01;
        response[offset++] = 0x00; // Time to live (TTL)
        response[offset++] = 0x00;
        response[offset++] = 0x00;
        response[offset++] = 0x00;
        response[offset++] = 0x0a; // Data length
        response[offset++] = (byte)ipAddress.GetAddressBytes()[0];
        response[offset++] = (byte)ipAddress.GetAddressBytes()[1];
        response[offset++] = (byte)ipAddress.GetAddressBytes()[2];
        response[offset++] = (byte)ipAddress.GetAddressBytes()[3];

        return response;
    }
}
