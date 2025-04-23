using danklibrary;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

namespace web.Services
{
    public class DiscoveryService(ILogger<DiscoveryService> logger)
    {
        private readonly ILogger<DiscoveryService> _logger = logger;

        public async Task<Subnet> StartDiscovery(Subnet subnet)
        {
            List<Task<IP>> tasks = new();

            foreach (var ip in subnet.List)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                    {
                        return DiscoveryTask(ip);
                    }));
            }

            await Task.WhenAll(tasks);

            subnet.List = tasks.Select(x => x.Result).ToList();

            return subnet;
        }

        public IP DiscoveryTask(IP ip)
        {
            using var ping = new Ping();

            _logger.LogInformation($"Scanning {IP.ConvertToString(ip.Address)}");

            var ipAddress = new IPAddress(ip.Address);

            if (ping.Send(ipAddress).Status == IPStatus.Success)
            {
                _logger.LogInformation($"{(IP.ConvertToString(ip.Address))} responded to ping");
                ip.IsMonitoredICMP = true;

                try
                {
                    ip.Hostname = Dns.GetHostEntry(ipAddress).HostName;
                    _logger.LogInformation($"{IP.ConvertToString(ip.Address)} resolved to {ip.Hostname}");
                }
                catch (SocketException)
                {
                    ip.Hostname = ip.Hostname;
                }
            }

            return ip;
        }
    }
}
