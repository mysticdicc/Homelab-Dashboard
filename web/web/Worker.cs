using danklibrary;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

namespace web
{
    public class Worker(ILogger<Worker> logger, HttpClient httpClient) : BackgroundService
    {
        private Timer? _timer = null;
        private readonly HttpClient _httpClient = httpClient;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Service action has intiated");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Entered execution action");
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
                        logger.LogInformation($"Fetched {ips.Count()} from API");

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
                            logger.LogInformation($"Ping testing {IP.ConvertToString(ip.Address)}");

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
                            logger.LogInformation($"TCP testing {IP.ConvertToString(ip.Address)}");

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

                        logger.LogInformation(JsonConvert.SerializeObject(ips, Formatting.Indented));

                        try
                        {
                            await _httpClient.PostAsJsonAsync<List<IP>>("/monitoring/post/newpoll", ips, CancellationToken.None);
                            logger.LogInformation("Ips submitted to api endpoint");
                        } 
                        catch (Exception ex)
                        {
                            logger.LogError(ex.Message);
                        }
                    }
                    else
                    {
                        logger.LogError("Could not fetch IP list from api endpoint");
                    }

                    await Task.Delay(60000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("User intiated service end");
            }
            catch (Exception ex) {
                logger.LogCritical(ex.Message);
                await Task.Delay(15000, stoppingToken);
            }
        }

        override public void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
