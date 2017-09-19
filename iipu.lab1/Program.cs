using System;
using System.Collections.Generic;
using System.IO;
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

            try
            {
                using (var searcher = new ManagementObjectSearcher(new ManagementScope(),
                    new SelectQuery("SELECT * from Win32_PnPEntity")))
                {
                    ManagementObjectCollection managers = searcher.Get();

                    Regex pciDeviceRegex = new Regex(@"^PCI\\*");
                    string pciDidVidRegexPattern = @"(_)|(&)";
                    const int vidPos = 2;
                    const int didPos = 6;
                    const string dbFileName = "pci.ids.txt";

                    string vidRegexPattern = @"";
                    string didRegexPattern = "\t";

                    foreach (var manager in managers)
                    {
                        string devId = manager["DeviceID"].ToString();

                        if (!pciDeviceRegex.IsMatch(devId))
                        {
                            continue;
                        }

                        var stringParts = Regex.Split(devId, pciDidVidRegexPattern);

                        using (var fs = File.Open(dbFileName, FileMode.Open))
                        {
                            using (var sr = new StreamReader(fs))
                            {
                                sr.ReadLine();
                            }
                        }

                        Console.WriteLine("VID field: {0}", stringParts[vidPos]);
                        Console.WriteLine("DID field: {0}", stringParts[didPos]);
                        Console.WriteLine("------------------------------------");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
    }
}