using System;
using System.Net;
using System.Net.Sockets;
using MySqlConnector;

class DNSServer
{
    static void Main(string[] args)
    {
        int port = 53;
        IPAddress ipAddress = IPAddress.Any;

        TcpListener listener = new TcpListener(ipAddress, port);
        listener.Start();

        Console.WriteLine("DNS server listening on port {0}...", port);

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();

            // Read the query from the client
            byte[] queryData = new byte[512];
            NetworkStream stream = client.GetStream();
            int queryLength = stream.Read(queryData, 0, queryData.Length);

            // Parse the query to get the domain name
            string domainName = ParseDomainName(queryData);

            // Retrieve the IP address from the MySQL database
            string ipAddressString = GetIPAddressFromDatabase(domainName);

            // Send the response back to the client
            byte[] responseData = BuildResponse(queryData, ipAddressString);
            stream.Write(responseData, 0, responseData.Length);

            client.Close();
        }
    }

    static string ParseDomainName(byte[] queryData)
    {
        string domainName = "";
        int length = queryData[5];

        for (int i = 12; i < 12 + length; i++)
        {
            domainName += Convert.ToChar(queryData[i]);
        }

        return domainName;
    }

    static string GetIPAddressFromDatabase(string domainName)
    {
        string connectionString = "Server=127.0.0.1;Database=myDatabase;Uid=myUsername;Pwd=myPassword;";
        MySqlConnection connection = new MySqlConnection(connectionString);

        try
        {
            connection.Open();

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT ip_address FROM dns_records WHERE domain_name = @domainName";
            command.Parameters.AddWithValue("@domainName", domainName);

            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetString(0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            connection.Close();
        }

        return null;
    }

    static byte[] BuildResponse(byte[] queryData, string ipAddressString)
    {
        byte[] response = new byte[queryData.Length + 16];
        Array.Copy(queryData, response, queryData.Length);

        response[2] |= 0x80;
        response[3] = 0x01;
        response[6] = 0x00;
        response[7] = 0x01;
        response[10] = 0x00;
        response[11] = 0x04;

        string[] octets = ipAddressString.Split('.');
        response[queryData.Length] = Convert.ToByte(octets[0]);
        response[queryData.Length + 1] = Convert.ToByte(octets[1]);
        response[queryData.Length + 2] = Convert.ToByte(octets[2]);
        response[queryData.Length + 3] = Convert.ToByte(octets[3]);

        return response;
    }
}
