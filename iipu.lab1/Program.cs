using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace iipu.lab1
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("PCI devices:");
            using (var searcher = new ManagementObjectSearcher(new ManagementScope(), new SelectQuery("SELECT * from Win32_PnPEntity")))
            {
                ManagementObjectCollection managers = searcher.Get();

                Regex pciDeviceRegex = new Regex(@"^PCI\\*");
                string pciDidVidRegexPattern = @"(_)|(&)";
                const int vidPos = 2;
                const int didPos = 6;
                
                foreach (var manager in managers)
                {
                    string devId = manager["DeviceID"].ToString();
                    
                    if (!pciDeviceRegex.IsMatch(devId))
                    {
                        continue;
                    }

                    var stringParts = Regex.Split(devId, pciDidVidRegexPattern);
                    Console.WriteLine("VID field: {0}", stringParts[vidPos]);
                    Console.WriteLine("DID field: {0}", stringParts[didPos]);
                    Console.WriteLine("------------------------------------");
                }
            }
        }
    }
}