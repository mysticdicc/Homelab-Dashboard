using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace danklibrary
{
    public class MonitorSettings
    {
        public int MonitorDelay { get; set; }

        static public void UpdateMonitorDelay(int monitorDelay)
        {
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                string json = File.ReadAllText(filePath);

                dynamic? obj = JsonConvert.DeserializeObject(json);

                if (null != obj)
                {
                    obj["MonitorSettings"]["MonitorDelay"] = monitorDelay.ToString();
                    string output = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    File.WriteAllText(filePath, output);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        static public int GetMonitorDelay()
        {
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                string json = File.ReadAllText(filePath);

                dynamic? obj = JsonConvert.DeserializeObject(json);

                if (null != obj)
                {
                    return obj["MonitorSettings"]["MonitorDelay"];
                }
                else
                {
                    throw new Exception("Unable to fetch delay");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
