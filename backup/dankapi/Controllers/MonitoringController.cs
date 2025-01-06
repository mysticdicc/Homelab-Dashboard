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
        [Route("[controller]/get/allmonitored")]
        public string GetAll()
        {
            using var context = new danknetContext();
            return JsonConvert.SerializeObject(context.IPs.Where(x => x.IsMonitoredTCP || x.IsMonitoredICMP).ToList());
        }

        [HttpGet]
        [Route("[controller]/get/allpolls")]
        public string GetAllPolls()
        {
            using var context = new danknetContext();
            return JsonConvert.SerializeObject(context.MonitorStates.ToList());
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
