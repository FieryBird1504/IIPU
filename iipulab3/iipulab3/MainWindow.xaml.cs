using iipulab3.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;


namespace SiperMegaApplication
{
    public partial class MainWindow : Window
    {
        long prevTime = -1;

        public MainWindow()
        {
            InitializeComponent();

            Closing += whenCloseWindow;

            aTimer = new System.Windows.Threading.DispatcherTimer();
            aTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            aTimer.Interval = new TimeSpan(0, 0, 1);
            aTimer.Start();
        }

        public static bool isRunningOnBattery { get; set; }
        private System.Windows.Threading.DispatcherTimer aTimer;

        private void whenCloseWindow(object source, EventArgs e)
        {
            if (prevTime >= 0)
            {
                SetCurrDisplayTurnOffValue(prevTime);
                prevTime = -1;
            }
        }

        private void dispatcherTimer_Tick(object source, EventArgs e)
        {
            {
                PowerStatus status = SystemInformation.PowerStatus;
                ChargeStatus.Content = status.BatteryChargeStatus.ToString();

                FullLifetime.Content = status.BatteryFullLifetime.ToString();
                if (status.BatteryFullLifetime == -1)
                    FullLifetime.Content = "Unknown";

                Charge.Content = status.BatteryLifePercent.ToString("P0");

                LifeRemaining.Content = status.BatteryLifeRemaining.ToString();
                if (status.BatteryLifeRemaining == -1)
                    LifeRemaining.Content = "Unknown";

                LineStatus.Content = status.PowerLineStatus.ToString();

                isRunningOnBattery =
                    (System.Windows.Forms.SystemInformation.PowerStatus.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Offline);

                RunningOn.Content = isRunningOnBattery ? "Battery" : "Power";

                if ((status.BatteryChargeStatus == BatteryChargeStatus.Low || status.BatteryChargeStatus == BatteryChargeStatus.High 
                    || status.BatteryChargeStatus == BatteryChargeStatus.Critical) && prevTime < 0 && status.BatteryLifePercent < 30)
                {
                    prevTime = ReadCurrDisplayTurnOffValue();
                    SetCurrDisplayTurnOffValue(1);
                }
                else if ((status.BatteryChargeStatus == BatteryChargeStatus.Charging || status.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery
                    || status.BatteryChargeStatus == BatteryChargeStatus.Unknown) && prevTime >= 0)
                {
                    SetCurrDisplayTurnOffValue(prevTime);
                    prevTime = -1;
                }
            }
        }
        static void SetBrightness(byte targetBrightness)
        {
            ManagementScope scope = new ManagementScope("root\\WMI");
            SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
                {
                    foreach (ManagementObject mObj in objectCollection)
                    {
                        mObj.InvokeMethod("WmiSetBrightness",
                            new Object[] { UInt32.MaxValue, targetBrightness });
                        break;
                    }
                }
            }
        }

        static string RunScript(string scriptText)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace();

            runspace.Open();

            Pipeline pipeline = runspace.CreatePipeline();

            pipeline.Commands.AddScript(scriptText);

            pipeline.Commands.Add("Out-String");

            Collection<PSObject> results = pipeline.Invoke();

            runspace.Close();

            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            return stringBuilder.ToString();
        }

        static long ReadCurrDisplayTurnOffValue()
        {
            var res = RunScript(
                @"$plan = Get-WmiObject -Class win32_powerplan -Namespace root\cimv2\power -Filter ""isActive='true'"";$regex = [regex]""{(.*?)}$"";$planGuid = $regex.Match($plan.instanceID.Tostring()).groups[1].value;powercfg -query $planGuid");

            var toFind0 = "GUID Alias: SUB_VIDEO";
            var toFind = "Current DC Power Setting Index:";

            var lines = res.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            lines.RemoveRange(0, lines.FindIndex(s => s.Contains(toFind0)));

            var snum = (from s in lines
                        where s.Contains(toFind)
                        select s.Replace(toFind, string.Empty).Replace(" ", string.Empty)).First();

            return long.Parse(snum.Replace("0x", string.Empty), System.Globalization.NumberStyles.HexNumber) / 60;
        }

        static void SetCurrDisplayTurnOffValue(long timeInMin)
        {
            RunScript($"powercfg -change -monitor-timeout-dc {timeInMin}");
        }
    }
}
