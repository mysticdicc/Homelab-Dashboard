using System.Net.Sockets;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace danklibrary.DankAPI
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

        public async Task EditItem(DashboardItem item)
        {
            try
            {
                await _httpClient.PutAsJsonAsync<DashboardItem>("/dashboard/put/update", item);
            }
            catch
            {
                throw;
            }
        }


    }

    public class Subnets(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<Subnet>> GetAll()
        {
            List<Subnet>? items = await _httpClient.GetFromJsonAsync<List<Subnet>>("/subnets/subnet/get/all");

            if (null != items)
            {
                return items;
            }
            else
            {
                throw new Exception("List empty");
            }
        }

        public async Task AddSubnet(string CIDR)
        {
            await _httpClient.PostAsync($"/subnets/subnet/post/new?CIDR={CIDR.Replace("/", "%2F")}", new StringContent(String.Empty));
        }

        public async Task EditIP(IP ip)
        {
            await _httpClient.PutAsJsonAsync<IP>("/subnets/ip/put/update", ip);
        }

        public async Task DeleteSubnet(int ID)
        {
            await _httpClient.DeleteAsync($"/subnets/subnet/delete/byid?ID={ID}");
        }
    }
}
