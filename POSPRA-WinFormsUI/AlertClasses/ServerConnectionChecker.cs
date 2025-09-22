using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace POSPRA_WinFormsUI.AlertClasses
{
    public class ServerConnectionChecker
    {
        private System.Windows.Forms.Timer internetCheckTimer;
        private bool lastNetworkAvailable = true;
        private bool lastInternetReachable = true;

        // Events you can subscribe to in Form1
        public event Action<string> OnNetworkAvailable;
        public event Action<string> OnNetworkUnavailable;
        public event Action<string> OnInternetConnected;
        public event Action<string> OnInternetDisconnected;
        public event Action<string> OnIpChanged;

        public ServerConnectionChecker()
        {
            // Hook into system events
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;
            NetworkChange.NetworkAddressChanged += NetworkAddressChanged;

            // Timer for Internet reachability (ping test)
            internetCheckTimer = new System.Windows.Forms.Timer();
            internetCheckTimer.Interval = 5000; // every 10s
            internetCheckTimer.Tick += InternetCheckTimer_Tick;
            internetCheckTimer.Start();
        }

        private void NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable && !lastNetworkAvailable)
            {
                lastNetworkAvailable = true;
                OnNetworkAvailable?.Invoke("Network Available");
            }
            else if (!e.IsAvailable && lastNetworkAvailable)
            {
                lastNetworkAvailable = false;
                OnNetworkUnavailable?.Invoke("Network Unavailable");
            }
        }

        private void NetworkAddressChanged(object sender, EventArgs e)
        {
            string ip = GetLocalIPAddress();
            OnIpChanged?.Invoke($"IP Changed: {ip}");
        }

        private void InternetCheckTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send("74.125.68.101", 2000);
                    if (reply.Status == IPStatus.Success)
                    {
                        if (!lastInternetReachable)
                        {
                            lastInternetReachable = true;
                            OnInternetConnected?.Invoke("Internet Connected");
                        }
                    }
                    else
                    {
                        if (lastInternetReachable)
                        {
                            lastInternetReachable = false;
                            OnInternetDisconnected?.Invoke("Internet Disconnected");
                        }
                    }
                }
            }
            catch
            {
                if (lastInternetReachable)
                {
                    lastInternetReachable = false;
                    OnInternetDisconnected?.Invoke("Internet Disconnected (Exception)");
                }
            }
        }

        private string GetLocalIPAddress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            return ip.Address.ToString();
                    }
                }
            }
            return "Unknown";
        }
    }
}
