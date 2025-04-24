using dankweb.API;
using Microsoft.AspNetCore.Mvc;
using danklibrary;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using web.Services;

namespace web.Controllers
{
    [ApiController]
    public class SubnetsController(IDbContextFactory<danknetContext> dbContext, DiscoveryService discoveryService) : Controller
    {
        private readonly IDbContextFactory<danknetContext> _DbFactory = dbContext;
        private readonly DiscoveryService _discoveryService = discoveryService;

        [HttpPost]
        [Route("[controller]/startdiscovery")]
        public async Task<Results<BadRequest<string>, Ok<Subnet>>> StartSubnetDiscovery(Subnet subnet)
        {
            var _subnet = await _discoveryService.StartDiscovery(subnet);

            if (null != _subnet) 
            {
                return TypedResults.Ok(_subnet);
            } 
            else
            {
                return TypedResults.BadRequest("Null object return");
            }
        }

        [HttpPost]
        [Route("[controller]/subnet/post/new")]
        public async Task<Results<BadRequest<string>, Created<Subnet>>> AddSubnet(string CIDR)
        {
            using var context = _DbFactory.CreateDbContext();

            Subnet subnet = new Subnet(CIDR);
            context.Subnets.Add(subnet);

            await context.SaveChangesAsync();
            return TypedResults.Created(subnet.ID.ToString(), subnet);
        }

        [HttpGet]
        [Route("[controller]/subnet/get/all")]
        public string GetAllSubnets()
        {
            using var context = _DbFactory.CreateDbContext();

            List<Subnet> tempSubnet = context.Subnets.ToList();

            foreach (Subnet subnet in tempSubnet)
            {
                subnet.List = context.IPs.Where(x => x.SubnetID == subnet.ID).ToList();
            }

            return JsonConvert.SerializeObject(tempSubnet, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }
                );
        }

        [HttpDelete]
        [Route("[controller]/subnet/delete/byid")]
        public async Task<Results<BadRequest<string>, Ok<int>>> DeleteSubnetByID(int ID)
        {
            using var context = _DbFactory.CreateDbContext();

            Subnet? deleteItem = context.Subnets.Find(ID);

            if (null != deleteItem)
            {
                try
                {
                    context.Subnets.Remove(deleteItem);
                    await context.SaveChangesAsync();

                    return TypedResults.Ok(deleteItem.ID);
                } 
                catch (Exception ex)
                {
                    return TypedResults.BadRequest(ex.Message);
                }
            } 
            else
            {
                return TypedResults.BadRequest("unable to find ID in db");
            }
        }

        [HttpPost]
        [Route("[controller]/ip/post/new")]
        public async Task<Results<BadRequest<string>, Created<IP>>> AddIP(IP ip)
        {
            using var context = _DbFactory.CreateDbContext();

            //validate
            bool validObject = true;

            if (validObject)
            {
                try
                {
                    if (null != ip.PortsMonitored)
                    {
                        ip.IsMonitoredTCP = true;
                    }

                    context.IPs.Add(ip);
                    await context.SaveChangesAsync();

                    return TypedResults.Created(ip.ID.ToString(), ip);
                }
                catch (Exception ex)
                {
                    return TypedResults.BadRequest(ex.Message);
                }
            }
            else
            {
                return TypedResults.BadRequest("Object invalid.");
            }
        }

        [HttpGet]
        [Route("[controller]/ip/get/all")]
        public string GetAllIP()
        {
            using var context = _DbFactory.CreateDbContext();
            return JsonConvert.SerializeObject(context.IPs);
        }

        [HttpPut]
        [Route("[controller]/ip/put/update")]
        public async Task<Results<BadRequest<string>, Ok<IP>>> UpdateIP(IP ip)
        {
            using var context = _DbFactory.CreateDbContext();

            var updateItem = context.IPs.Find(ip.ID);

            if (updateItem != null) 
            {
                updateItem.Hostname = ip.Hostname;
                updateItem.IsMonitoredICMP = ip.IsMonitoredICMP;

                if (null != ip.PortsMonitored)
                {
                    updateItem.IsMonitoredTCP = true;
                    updateItem.PortsMonitored = ip.PortsMonitored;
                }

                context.IPs.Update(updateItem);

                await context.SaveChangesAsync();
                return TypedResults.Ok(updateItem);
            } 
            else
            {
                return TypedResults.BadRequest($"Unable to find item with {ip.ID}");
            }
        }

        [HttpPost]
        [Route("[controller]/subnet/post/discoveryupdate")]
        public async Task<Results<Ok<List<int>>, Ok<Subnet>>> UpdateSubnet(Subnet subnet)
        {
            using var context = _DbFactory.CreateDbContext();
            List<int> badId = [];

            foreach (var ip in subnet.List)
            {
                var item = context.IPs.Find(ip.ID);
                if (null != item)
                {
                    item.Hostname = ip.Hostname;
                    item.IsMonitoredICMP = ip.IsMonitoredICMP;
                    context.IPs.Update(item);
                } 
                else
                {
                    badId.Add(ip.ID);
                }
            }

            await context.SaveChangesAsync();

            if (badId.Count > 0) 
            {
                return TypedResults.Ok<List<int>>(badId);
            } 
            else
            {
                return TypedResults.Ok<Subnet>(subnet);
            }
        }
    }
}
