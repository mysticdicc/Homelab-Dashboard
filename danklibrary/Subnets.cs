using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NetTools;

namespace danklibrary
{
    public class IP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        required public byte[] Address { get; set; }

        public string? Hostname { get; set; }

        public int SubnetID { get; set; }

        [JsonIgnore]
        public Subnet? Subnet { get; }

        [JsonIgnore]
        public MonitorState? MonitorState { get; set; }
        public IEnumerable<MonitorState>? MonitorStateList { get; set; }

        public bool IsMonitoredICMP { get; set; }
        public bool IsMonitoredTCP { get; set; }
        public List<int>? PortsMonitored { get; set; }

        public bool IsValid(IP ip)
        {
            //address is 4 bytes
            //address is a valid ip
            bool addressValid;

            if (ip.Address.Length != 4)
            {
                return false;
            }

            //if (ip.Add)

            //hostname <= 100
            bool hostValid;

            //subnetid is a valid int
            bool subIdValid;

            return true;
        }

        static public byte[] ConvertToByte(IPAddress ip)
        {
            return IPAddress.Parse(ip.ToString()).GetAddressBytes();
        } 

        static public byte[] ConvertToByte(string ip)
        {
            return IPAddress.Parse(ip).GetAddressBytes();
        }

        static public byte[] GetMaskFromCidr(int cidr)
        {
            var mask = (cidr == 0) ? 0 : uint.MaxValue << (32 - cidr);
            return BitConverter.GetBytes(mask).Reverse().ToArray();
        }

        static public string ConvertToString(byte[] ip)
        {
            var temp = new IPAddress(ip);
            return temp.ToString();
        }
    }

    public class IpRowState
    {
        public bool EditHidden { get; set; }
        public string? PortNumbers { get; set; }

    }

    public class Subnet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public byte[] Address { get; set; }

        public byte[] SubnetMask { get; set; }

        public byte[] StartAddress { get; set; }

        public byte[] EndAddress { get; set; }

        public IList<IP> List { get; set; }

        public Subnet() { }

        public Subnet(string CIDR)
        {
            var subnet = IPAddressRange.Parse(CIDR);

            StartAddress = IP.ConvertToByte(subnet.Begin);
            EndAddress = IP.ConvertToByte(subnet.End);
            
            var split = CIDR.Split('/');

            SubnetMask = IP.GetMaskFromCidr(Int32.Parse(split[1]));
            Address = IP.ConvertToByte(split[0]);

            var temp = new List<IP>();

            foreach (IPAddress ip in subnet) {
                temp.Add(
                    new IP
                    {
                        Address = IP.ConvertToByte(ip)
                    }
                );
            }

            List = temp;
        }
    }

    public class SubnetRowState
    {
        public bool Hidden { get; set; }
        public string? SearchTerm { get; set; }
        public bool FilterRowHidden { get; set; }
        public bool IcmpFilterEnabled { get; set; }
        public bool TcpFilterEnabled { get; set; }
        public enum FilterByOption { And, Or }
        public FilterByOption FilterBy { get; set; }
    }

    public class MonitorState
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int IP_ID { get; set; }
        [JsonIgnore]
        public IP? IP { get; set; }

        public required DateTime SubmitTime { get; set; }
        public PingState? PingState { get; set; }
        public List<PortState>? PortState { get; set; }
    }

    public class PortState
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [ForeignKey("MonitorState")]
        public int MonitorID { get; set; }
        [JsonIgnore]
        public MonitorState? MonitorState { get; set; }
        required public int Port { get; set; }
        required public bool Status { get; set; }
    }

    public class PingState
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [ForeignKey("MonitorState")]
        public int MonitorID { get; set; }
        required public bool Response { get; set; }
        [JsonIgnore]
        public MonitorState? MonitorState { get; set; }
    }
}
