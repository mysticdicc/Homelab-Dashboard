using danklibrary;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Options;
using danklibrary.DankAPI;

namespace web
{
    public class Monitor : BackgroundService
    {
        private Timer? _timer;
        private readonly HttpClient _httpClient;
        public int _delay;
        private ILogger<Monitor> _logger;
        CancellationTokenSource _cancellationToken;
        Monitoring _monitoringApi;

        public Monitor(IOptions<MonitorSettings> options, ILogger<Monitor> logger, HttpClient httpClient, Monitoring monitoringApi)
        {
            _delay = options.Value.MonitorDelay;
            _httpClient = httpClient;
            _timer = null;
            _logger = logger;
            _cancellationToken = new();
            _monitoringApi = monitoringApi;
        }

        private async Task RunServiceAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _logger.LogInformation($"Entered execution action, task delay is {_delay}ms");
                    await Task.Delay(1500);

                    DateTime submit = DateTime.Now;

                    //fetch monitored devices
                    var handler = new HttpClientHandler();
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) =>
                        {
                            return true;
                        };

                    List<IP>? ips = [];

                    ips = await _httpClient.GetFromJsonAsync<List<IP>>("/monitoring/get/allmonitored", CancellationToken.None);

                    if (null != ips)
                    {
                        _logger.LogInformation($"Fetched {ips.Count()} from API");

                        foreach (IP ip in ips)
                        {
                            if (null == ip.MonitorStateList)
                            {
                                List<MonitorState> states = [];
                                ip.MonitorStateList = states;
                            }
                        }

                        foreach (var ip in ips.Where(x => x.IsMonitoredICMP))
                        {
                            _logger.LogInformation($"Ping testing {IP.ConvertToString(ip.Address)}");

                            using Ping ping = new Ping();

                            var monitorState = new MonitorState
                            {
                                SubmitTime = submit,
                                IcmpResponse = ping.Send(new IPAddress(ip.Address)).Status == IPStatus.Success,
                                IP_ID = ip.ID
                            };

                            if (null != ip.MonitorStateList)
                            {
                                ip.MonitorStateList = ip.MonitorStateList.Append(monitorState);
                            }
                        }

                        foreach (var ip in ips.Where(x => x.IsMonitoredTCP && null != x.PortsMonitored))
                        {
                            _logger.LogInformation($"TCP testing {IP.ConvertToString(ip.Address)}");

                            List<PortState> portStates = [];

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

                                PortState state = new()
                                {
                                    Port = port,
                                    Status = status
                                };

                                portStates.Add(state);
                            }

                            MonitorState monitorState = new()
                            {
                                SubmitTime = submit,
                                PortState = portStates,
                                IP_ID = ip.ID
                            };

                            if (null != ip.MonitorStateList)
                            {
                                ip.MonitorStateList = ip.MonitorStateList.Append(monitorState);
                            }
                        }

                        _logger.LogInformation(JsonConvert.SerializeObject(ips, Formatting.Indented));

                        try
                        {
                            await _httpClient.PostAsJsonAsync<List<IP>>("/monitoring/post/newpoll", ips, CancellationToken.None);
                            _logger.LogInformation("Ips submitted to api endpoint");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                    }
                    else
                    {
                        _logger.LogError("Could not fetch IP list from api endpoint");
                    }

                    await Task.Delay(_delay, token);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("User intiated service end");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                await Task.Delay(15000, token);
            }
        }

        async public void Restart()
        {
            _logger.LogInformation("Monitoring service restart initiated");
            _delay = await _monitoringApi.GetTimer();
            _cancellationToken.Cancel();
            _cancellationToken = new();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service action has intiated");

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunServiceAsync(_cancellationToken.Token);
            }
        }

        override public void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
