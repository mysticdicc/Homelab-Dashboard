using dankweb.API;
using Microsoft.AspNetCore.Mvc;
using danklibrary;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace web.Controllers
{
    [ApiController]
    public class SubnetsController : Controller
    {
        [HttpPost]
        [Route("[controller]/subnet/post/new")]
        public async Task<Results<BadRequest<string>, Created<Subnet>>> AddSubnet(string CIDR)
        {
            using var context = new danknetContext();

            Subnet subnet = new Subnet(CIDR);
            context.Subnets.Add(subnet);

            await context.SaveChangesAsync();
            return TypedResults.Created(subnet.ID.ToString(), subnet);
        }

        [HttpGet]
        [Route("[controller]/subnet/get/all")]
        public string GetAllSubnets()
        {
            using var context = new danknetContext();

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
            using var context = new danknetContext();

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
            using var context = new danknetContext();

            //validate
            bool validObject = true;

            if (validObject)
            {
                try
                {
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
            using var context = new danknetContext();
            return JsonConvert.SerializeObject(context.IPs);
        }

        [HttpPut]
        [Route("[controller]/ip/put/update")]
        public async Task<Results<BadRequest<string>, Ok<IP>>> UpdateIP(IP ip)
        {
            using var context = new danknetContext();

            var updateItem = context.IPs.Find(ip.ID);

            updateItem.Hostname = ip.Hostname;
            updateItem.IsMonitoredICMP = ip.IsMonitoredICMP;
            updateItem.IsMonitoredTCP = ip.IsMonitoredTCP;
            updateItem.PortsMonitored = ip.PortsMonitored;
            context.IPs.Update(updateItem);

            await context.SaveChangesAsync();

            return TypedResults.Ok(updateItem);
        }
    }
}
