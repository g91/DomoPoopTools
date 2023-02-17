using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Renci.SshNet;

namespace Bobs_poop_Toolsx 
{ 
   
    public class SshServer
    {
        public string Id { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SshHelper
    {
        private Dictionary<string, SshServer> _servers;

        public SshHelper()
        {
            _servers = new Dictionary<string, SshServer>();
        }

        public void AddServer(string id, string host, string username, string password)
        {
            _servers[id] = new SshServer
            {
                Id = id,
                Host = host,
                Username = username,
                Password = password
            };
        }

        public string ExecuteCommand(string serverId, string command)
        {
            if (!_servers.ContainsKey(serverId))
            {
                return $"Error: server with ID '{serverId}' was not found.";
            }

            var server = _servers[serverId];

            using (var client = new SshClient(server.Host, server.Username, server.Password))
            {
                client.Connect();
                using (var cmd = client.CreateCommand(command))
                {
                    var result = cmd.Execute();
                    client.Disconnect();
                    return result;
                }
            }
        }
    }
}
