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
        const string dbFileName = "pci.ids.txt";
        const string vidRegexPattern = @"";
        const string didRegexPattern = "\t";
        public static void WorkWithFile(string vid, string did)
        {
            using (var fs = File.Open(dbFileName, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        //TODO: find VID
                    }
                    
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        //TODO: find DID
                    }

                    if (sr.EndOfStream)
                    {
                        Console.WriteLine("VID in HEX: {0}", vid);
                        Console.WriteLine("DID in HEX: {0}", did);
                    }
                }
            }

            
        }
        
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

                    foreach (var manager in managers)
                    {
                        string devId = manager["DeviceID"].ToString();

                        if (!pciDeviceRegex.IsMatch(devId))
                        {
                            continue;
                        }

                        var stringParts = Regex.Split(devId, pciDidVidRegexPattern);

                        WorkWithFile(stringParts[vidPos], stringParts[didPos]);
                        Console.WriteLine("------------------------------------");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}