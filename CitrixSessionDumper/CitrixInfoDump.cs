using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CitrixSessionDumper
{
    public static class CitrixInfoDump
    {
        // ==== WTS API IMPORTS ====
        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation
        (
            IntPtr hServer,
            int sessionId,
            WTS_INFO_CLASS wtsInfoClass,
            out IntPtr ppBuffer,
            out int bytesReturned
        );

        [DllImport("Wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        // === Session Information Types ===
        public enum WTS_INFO_CLASS
        {
            WTSUserName = 5,
            WTSDomainName = 7,
            WTSClientAddress = 14,
            WTSClientName = 10,
        }

        // === WTS Structure for IP Address ===
        [StructLayout(LayoutKind.Sequential)]
        struct WTS_CLIENT_ADDRESS
        {
            public int AddressFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Address;
        }

        public static int ProcessSessionId()
        {
            return System.Diagnostics.Process.GetCurrentProcess().SessionId;
        }

        public static string GetWTSString(WTS_INFO_CLASS infoClass, int sessionId)
        {
            IntPtr buffer;
            int bytesReturned;

            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, infoClass, out buffer, out bytesReturned))
            {
                string result = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
                return result;
            }

            return "Error";
        }

        public static string GetClientIP(int sessionId)
        {
            IntPtr buffer;
            int bytesReturned;

            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WTS_INFO_CLASS.WTSClientAddress, out buffer, out bytesReturned))
            {
                WTS_CLIENT_ADDRESS address = (WTS_CLIENT_ADDRESS)Marshal.PtrToStructure(buffer, typeof(WTS_CLIENT_ADDRESS));
                if (address.AddressFamily == 2) // AF_INET (IPv4)
                {
                    string ip = $"{address.Address[2]}.{address.Address[3]}.{address.Address[4]}.{address.Address[5]}";
                    WTSFreeMemory(buffer);
                    return ip;
                }

                WTSFreeMemory(buffer);
                return "Not IPv4";
            }

            return "Error getting IP";
        }

        public static string GetProcessAndSystemInfo()
        {
            var proc = Process.GetCurrentProcess();
            long memory = proc.WorkingSet64;
            TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==== PROCESS & SYSTEM INFO ====");
            sb.AppendLine($"Session PID: {proc.Id}");
            sb.AppendLine($"Memory Usage: {memory / (1024 * 1024)} MB");
            sb.AppendLine($"System Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");

            return sb.ToString();
        }

        public static string GetSessionInfo(string timestamp, string username, string domain, string machine, string clientIP)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==== SESSION INFO ====");
            sb.AppendLine($"Timestamp: {timestamp}");
            sb.AppendLine($"Username: {username}");
            sb.AppendLine($"Domain: {domain}");
            sb.AppendLine($"Machine: {machine}");
            sb.AppendLine($"Client IP: {clientIP}");
            return sb.ToString();
        }

        public static string GetGPODump()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" ==== GPO RESULTS (USER SCOPE) ====");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c gpresult /scope:user /v",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    // Filter only lines related to Citrix GPOs
                    string[] keywords =
                    {
                        "Citrix",
                        "XenApp",
                        "XenDesktop",
                        "Virtual Apps",
                        "FSLogix",
                        "WEM",
                        "Loopback",
                        "Profile",
                        "Printers",
                        "Drives"
                    };

                    foreach (string line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    {
                        foreach (string keyword in keywords)
                        {
                            if (line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                sb.AppendLine(line.Trim());
                                break; // Only add the line once for any keyword match
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error retrieving GPO info: {ex.Message}");
            }

            return sb.ToString();
        }

        public static GPOGroupResult GetGPOGroupResults() 
        {
            GPOGroupResult result = new GPOGroupResult();

            // Step 1: Run gpresult and extract applied GPO names
            string gpresult = RunCommand("gpresult", "/scope:user /v");
            List<string> appliedGpos = new List<string>();
            foreach (var line in gpresult.Split('\n')) 
            {
                if (line.Trim().StartsWith("Applied Group Policy Objects", StringComparison.OrdinalIgnoreCase))
                    continue; 

                if (line.Trim().StartsWith("    "))
                    appliedGpos.Add(line.Trim());
            }

            // Step 2: Get Domain GPOs via PowerShell
            HashSet<string> domainGops = new HashSet<string>();
            using (PowerShell ps = PowerShell.Create()) 
            {
                ps.AddScript("Get-GPO -All | Select-Object -ExpandProperty DisplayName");
                foreach (var gpo in ps.Invoke())
                    domainGops.Add(gpo.ToString());
            }

            // Step 3: Compare and group them 
            foreach (var gpo in appliedGpos) 
            {
                if (domainGops.Contains(gpo))
                    result.ActiveGPOS.Add(gpo);
                else
                    result.GhostedGPOS.Add(gpo);
            }

            return result;
        }

        private static string RunCommand(string filename, string arguments) 
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo 
                {
                    FileName = filename,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false, 
                    CreateNoWindow = true
                }
            };

            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return output;
        }
    }
}
