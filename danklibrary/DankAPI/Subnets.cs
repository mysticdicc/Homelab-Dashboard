using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace danklibrary.DankAPI
{
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

        public async Task<Subnet> RunDiscoveryTask(Subnet subnet)
        {
            var request = await _httpClient.PostAsJsonAsync<Subnet>("/subnets/startdiscovery", subnet);
            var results = request.Content.ReadFromJsonAsync<Subnet>().Result;

            if (null != results) 
            {
                return results;
            } 
            else
            {
                throw new Exception("No response from discovery service");
            }
        }

        public async Task DiscoveryUpdate(Subnet subnet)
        {
            await _httpClient.PostAsJsonAsync<Subnet>("/subnets/subnet/post/discoveryupdate", subnet);
        }
    }
}
