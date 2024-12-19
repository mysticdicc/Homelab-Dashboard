using danklibrary;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace dankservice
{
    public class Worker : BackgroundService
    {
        private Timer? _timer = null;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {

                    //fetch monitored devices

                    /*
                    using HttpClient httpClient = new();
                    httpClient.BaseAddress = new Uri("https://localhost:7124");

                    List<IP>? ips = await httpClient.GetFromJsonAsync<List<IP>>("/monitoring/get/all", CancellationToken.None);

                    if (null != ips)
                    {
                        foreach (var ip in ips.Where(x => x.IsMonitoredICMP))
                        {
                            using Ping ping = new Ping();

                            ip.MonitorState = new MonitorState
                            {
                                SubmitTime = new DateTime(),
                                IcmpResponse = ping.Send(new IPAddress(ip.Address)).Status == IPStatus.Success
                            };
                        }

                        foreach (var ip in ips.Where(x => x.IsMonitoredTCP && null != x.PortsMonitored))
                        {

                            List<PortState> portTests = [];

                            foreach (int port in ip.PortsMonitored)
                            {
                                using var client = new TcpClient();
                                bool status = false;

                                try
                                {
                                    var address = new IPAddress(ip.Address);
                                    client.Connect(address, port);
                                    status = true;
                                }
                                catch
                                {
                                    status = false;
                                }

                                PortState state = new PortState
                                {
                                    Port = port,
                                    Status = status
                                };

                                portTests.Add(state);
                            }
                        }

                        await httpClient.PostAsJsonAsync<List<IP>>("/monitoring.post/newpoll", ips, CancellationToken.None);
                       


                    }
                    //send results to api
                     */
                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex) {
                Environment.Exit(1);
            }
        }

        async private void TimerEvent(object? state)
        {
            using HttpClient httpClient = new();
            httpClient.BaseAddress = new Uri("https://localhost:7124");

            List<IP>? ips = await httpClient.GetFromJsonAsync<List<IP>>("/monitoring/get/all", CancellationToken.None);

            if (null != ips)
            {
                foreach (var ip in ips.Where(x => x.IsMonitoredICMP))
                {
                    using Ping ping = new Ping();

                    ip.MonitorState = new MonitorState
                    {
                        SubmitTime = new DateTime(),
                        IcmpResponse = ping.Send(new IPAddress(ip.Address)).Status == IPStatus.Success
                    };
                }

                foreach (var ip in ips.Where(x => x.IsMonitoredTCP && null != x.PortsMonitored))
                {

                    List<PortState> portTests = [];

                    foreach (int port in ip.PortsMonitored)
                    {
                        using var client = new TcpClient();
                        bool status = false;

                        try
                        {
                            var address = new IPAddress(ip.Address);
                            client.Connect(address, port);
                            status = true;
                        }
                        catch
                        {
                            status = false;
                        }

                        PortState ste= new PortState
                        {
                            Port = port,
                            Status = status
                        };

                        portTests.Add(ste);
                    }
                }

                await httpClient.PostAsJsonAsync<List<IP>>("/monitoring.post/newpoll", ips, CancellationToken.None);



            }
        }

        public Task StartAsync(CancellationToken token)
        {
            _timer = new Timer(TimerEvent, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken token)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
