using danklibrary;
using System.Net.Sockets;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace dankweb.DankAPI
{
    public class Dash(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<DashboardItem>> GetAll()
        {
            List<DashboardItem>? items = await _httpClient.GetFromJsonAsync<List<DashboardItem>>("/dashboard/get/all");

            if (null != items )
            {
                return items;
            } 
            else
            {
                throw new Exception("List empty");
            }
        }

        public async Task DeleteItem(DashboardItem item)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = JsonContent.Create(item),
                Method = HttpMethod.Delete,
                RequestUri = new Uri("https://localhost:7124/dashboard/delete/byitem")
            };

            try
            {
                await _httpClient.SendAsync(request);
            } 
            catch
            {
                throw;
            }
        }

        public async Task AddItem(DashboardItem item)
        {
            try
            {
                await _httpClient.PostAsJsonAsync<DashboardItem>("/dashboard/post/new", item);
            } 
            catch
            {
                throw;
            }
        }
    }
}
