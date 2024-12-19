using dankapi.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using danklibrary;

namespace dankapi.Controllers
{
    [ApiController]
    public class MonitoringController : Controller
    {
        [HttpGet]
        [Route("[controller]/get/all")]
        public string GetAll()
        {
            using var context = new danknetContext();
            return JsonConvert.SerializeObject(context.IPs.Where(x => x.IsMonitoredTCP || x.IsMonitoredICMP).ToList());
        }

        [HttpPut]
        [Route("[controller]/put/enable/icmp")]
        public async Task<Results<BadRequest<string>, Ok<int>>> EnableMonitoringICMP(int ID)
        {
            using var context = new danknetContext();
            var update = context.IPs.Find(ID);
            
            if (null != update)
            {
                if (!update.IsMonitoredICMP)
                {
                    update.IsMonitoredICMP = true;
                    await context.SaveChangesAsync();
                    return TypedResults.Ok(update.ID);
                }
                else
                {
                    return TypedResults.Ok(update.ID);
                }
            } 
            else
            {
                return TypedResults.BadRequest("ID doesnt exist");
            }
        }

        [HttpPut]
        [Route("[controller]/put/enable/tcp")]
        public async Task<Results<BadRequest<string>, Ok<int>>> EnableMonitoringTCP(int ID, List<int>Ports)
        {
            using var context = new danknetContext();
            var update = context.IPs.Find(ID);

            if (null != update)
            {
                if (!update.IsMonitoredTCP)
                {
                    update.IsMonitoredTCP = true;
                    update.PortsMonitored = Ports;
                    await context.SaveChangesAsync();

                    return TypedResults.Ok(update.ID);
                }
                else if (update.IsMonitoredTCP && update.PortsMonitored != Ports) 
                {
                    update.PortsMonitored = Ports;
                    await context.SaveChangesAsync();

                    return TypedResults.Ok(update.ID);
                } 
                else 
                {
                    return TypedResults.Ok(update.ID);
                }
            }
            else
            {
                return TypedResults.BadRequest("ID doesnt exist");
            }

        }

        [HttpPut]
        [Route("[controller]/put/disable/icmp")]
        public async Task<Results<BadRequest<string>, Ok<int>>> DisableMonitoringICMP(int ID)
        {
            using var context = new danknetContext();
            var update = context.IPs.Find(ID);

            if (null != update)
            {
                if (!update.IsMonitoredICMP)
                {
                    update.IsMonitoredICMP = false;
                    await context.SaveChangesAsync();
                    return TypedResults.Ok(update.ID);
                }
                else
                {
                    return TypedResults.Ok(update.ID);
                }
            }
            else
            {
                return TypedResults.BadRequest("ID doesnt exist");
            }
        }

        [HttpPut]
        [Route("[controller]/put/disable/tcp")]
        public async Task<Results<BadRequest<string>, Ok<int>>> DisableMonitoringTCP(int ID)
        {
            using var context = new danknetContext();
            var update = context.IPs.Find(ID);

            if (null != update)
            {
                if (!update.IsMonitoredTCP)
                {
                    update.IsMonitoredTCP = false;
                    update.PortsMonitored = null;
                    await context.SaveChangesAsync();

                    return TypedResults.Ok(update.ID);
                }
                else
                {
                    return TypedResults.Ok(update.ID);
                }
            }
            else
            {
                return TypedResults.BadRequest("ID doesnt exist");
            }
        }

        [HttpPost]
        [Route("[controller]/post/newpoll")]
        public async Task<Results<BadRequest<string>, Ok>> NewDevicePoll(List<IP> ips)
        {
            using var context = new danknetContext();

            var monitorStates = ips.SelectMany(x => x.MonitorStateList);

            try
            {
                context.MonitorStates.AddRange(monitorStates);
                await context.SaveChangesAsync();

                return TypedResults.Ok();
            } 
            catch (Exception ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        }
    }
}
