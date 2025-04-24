using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace danklibrary.DankAPI
{
    public class Monitoring(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<MonitorState>> GetAllPolls()
        {
            List<MonitorState>? monitorStates = await _httpClient.GetFromJsonAsync<List<MonitorState>>("/monitoring/get/allpolls");

            if (null != monitorStates)
            {
                return monitorStates;
            } 
            else
            {
                throw new Exception();
            }
            
        }

        public async Task<List<IP>> GetMonitoredIPs()
        {
            List<IP> ips = await _httpClient.GetFromJsonAsync<List<IP>>("/monitoring/get/allmonitored");
            return ips;
        }

        public async Task<List<MonitorState>> GetByDeviceId(int ID)
        {
            List<MonitorState> monitorStates = await _httpClient.GetFromJsonAsync<List<MonitorState>>($"/monitoring/get/bydeviceid?id={ID.ToString()}");
            return monitorStates;
        }

        public async Task UpdateTimer(int monitorDelay)
        {
            await _httpClient.PostAsJsonAsync<int>("/monitoring/post/newtimer", monitorDelay);
        }

        public async Task<int> GetTimer()
        {
            var timer = await _httpClient.GetFromJsonAsync<int>("/monitoring/get/currenttimer");

            if (null != timer)
            {
                return timer;
            }
            else
            {
                throw new Exception("Unable to fetch current timer");
            }
        }

        public async Task RestartService()
        {
            await _httpClient.GetAsync("/monitoring/get/restart");
        }
    }
}
