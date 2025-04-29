using danklibrary;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Options;
using danklibrary.DankAPI;

namespace web.Services
{
    public class MonitorService : BackgroundService
    {
        private Timer? _timer;
        private readonly HttpClient _httpClient;
        public int _delay;
        private ILogger<MonitorService> _logger;
        CancellationTokenSource _cancellationToken;
        Monitoring _monitoringApi;

        public MonitorService(IOptions<MonitorSettings> options, ILogger<MonitorService> logger, HttpClient httpClient, Monitoring monitoringApi)
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

                    DateTime submit = DateTime.UtcNow;

                    //fetch monitored devices
                    var handler = new HttpClientHandler();
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) =>
                        {
                            return true;
                        };

                    List<IP>? ips = [];

                    ips = await _httpClient.GetFromJsonAsync<List<IP>>("/monitoring/get/allmonitoreddevices", CancellationToken.None);

                    if (null != ips)
                    {
                        _logger.LogInformation($"Fetched {ips.Count()} from API");

                        foreach (IP ip in ips)
                        {
                            MonitorState currentMonitorState = new()
                            {
                                SubmitTime = submit,
                                IP_ID = ip.ID
                            };

                            ip.MonitorStateList = [];

                            switch ((ip.IsMonitoredICMP, ip.IsMonitoredTCP))
                            {
                                case (true, true):
                                    currentMonitorState.PortState = TcpTest(ip);
                                    currentMonitorState.PingState = IcmpTest(ip);
                                    break;
                                case (true, false):
                                    currentMonitorState.PingState = IcmpTest(ip);
                                    break;
                                case (false, true):
                                    currentMonitorState.PortState = TcpTest(ip);
                                    break;
                            }

                            ip.MonitorStateList = ip.MonitorStateList.Append(currentMonitorState);

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

        List<PortState> TcpTest(IP ip)
        {
            _logger.LogInformation($"TCP testing {IP.ConvertToString(ip.Address)}");

            List<PortState> portStates = [];
            
            if (null != ip.PortsMonitored)
            {
                foreach (int port in ip.PortsMonitored)
                {
                    _logger.LogInformation($"Testing {port.ToString()} on {IP.ConvertToString(ip.Address)}");

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
            }

            return portStates;
        }

        PingState IcmpTest(IP ip)
        {
            _logger.LogInformation($"Ping testing {IP.ConvertToString(ip.Address)}");

            using Ping ping = new();

            PingState pingState = new()
            {
                Response = ping.Send(new IPAddress(ip.Address)).Status == IPStatus.Success
            };

            return pingState;
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
