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

    public class Subnet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public byte[] Address { get; set; }

        public byte[] SubnetMask { get; set; }

        public byte[] StartAddress { get; set; }

        public byte[] EndAddress { get; set; }

        public IEnumerable<IP> List { get; set; }

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
}
