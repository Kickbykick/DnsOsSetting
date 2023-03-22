using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using System.Net.NetworkInformation;
using System.Globalization;
using System.Net.Sockets;

namespace DnsSetting
{
    public class Class1
    {
        static void Main(string[] args)
        {
            NetworkInterface? networkInterface = GetActiveEthernetOrWifiNetworkInterface();
            Console.WriteLine(networkInterface?.Name);
            //SetDnsConfig("10.37.12.3,10.66.12.3");
            SetDnsConfig("64.59.176.16,64.59.176.228");
            showInfo();
        }

        public static NetworkInterface? GetActiveEthernetOrWifiNetworkInterface()
        {
            var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));

            return Nic;
        }

        public static void SetDnsConfig(string dsnString)
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            string nic = string.Empty;

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["ipEnabled"])
                {
                    nic = mo["Caption"].ToString();
                    if ((bool)mo["IPEnabled"])
                    {
                        if (mo["Caption"].Equals(nic))
                        {
                            ManagementBaseObject dnsEntry = mo.GetMethodParameters("SetDNSServerSearchOrder");
                            dnsEntry["DNSServerSearchOrder"] = dsnString.Split(',');//Two ip addresses you want to set   
                            ManagementBaseObject dnsMbo = mo.InvokeMethod("SetDNSServerSearchOrder", dnsEntry, null);
                            int returnCode = int.Parse(dnsMbo["returnvalue"].ToString(), CultureInfo.InvariantCulture);//This will give you the return code you can use to evaluate if its not working  

                            break;
                        }
                    }
                }
            }
        }

        internal static void showInfo()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface bendi in interfaces)
            {
                if (bendi.Speed != -1)
                {
                    Console.WriteLine(bendi.Name);

                    IPInterfaceProperties ip = bendi.GetIPProperties();
                    for (int i = 0; i < ip.UnicastAddresses.Count; i++)
                    {
                        if (ip.UnicastAddresses[i].Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (ip.UnicastAddresses[i].Address != null)
                                Console.WriteLine(ip.UnicastAddresses[i].Address.ToString());
                            if (ip.UnicastAddresses[i].IPv4Mask != null)
                                Console.WriteLine(ip.UnicastAddresses[i].IPv4Mask.ToString());
                        }
                    }
                    if (ip.GatewayAddresses.Count > 0)
                        Console.WriteLine(ip.GatewayAddresses[0].Address.ToString());
                    if (ip.DnsAddresses.Count > 0)
                        Console.WriteLine(ip.DnsAddresses[0].ToString());
                    if (ip.DnsAddresses.Count > 1)
                        Console.WriteLine(ip.DnsAddresses[1].ToString());
                    Console.WriteLine();
                }
            }
        }
    }
}