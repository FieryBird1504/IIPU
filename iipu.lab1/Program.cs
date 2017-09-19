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
                foreach (var manager in managers)
                {
                    string devID = manager["DeviceID"].ToString();
                    
                    if (!pciDeviceRegex.IsMatch(devID))
                    {
                        continue;
                    }
                    
                    Console.WriteLine("Device ID field: {0}", devID);
                }
            }
        }
    }
}