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
        const string vidRegexPattern = @"^";
        const string didRegexPattern = "^\t";
        public static void WorkWithFile(string vid, string did)
        {
            using (var fs = File.Open(dbFileName, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        var findRegex = new Regex(vidRegexPattern + vid.ToLower());
                        var line = sr.ReadLine();
                        if (findRegex.IsMatch(line))
                        {
                            Console.WriteLine("VID: {0}", line.Substring(vidRegexPattern.Length - 1));
                            break;
                        }
                    }
                    
                    while (!sr.EndOfStream)
                    {
                        var findRegex = new Regex(didRegexPattern + did.ToLower());
                        var line = sr.ReadLine();
                        if (findRegex.IsMatch(line))
                        {
                            Console.WriteLine("DID: {0}", line.Substring(didRegexPattern.Length - 1));
                            break;
                        }
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