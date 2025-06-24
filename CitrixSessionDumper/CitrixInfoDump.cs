using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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

        public static List<string> GetAppliedGPOs()
        {
            var applied = new List<string>();

            // Run gpresult and split into lines
            string gpResult = RunCommand("gpresult", "/scope:user /v");
            var lines = gpResult.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            bool inSection = false;
            foreach (var raw in lines)
            {
                // Find start of the Applied GPOs section
                if (!inSection)
                {
                    if (raw.Trim().StartsWith(
                        "Applied Group Policy Objects",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        inSection = true;
                    }
                    continue;
                }

                // Section ends when we hit a blank line or non-indented line
                if (string.IsNullOrWhiteSpace(raw) || !char.IsWhiteSpace(raw, 0))
                    break;

                var name = raw.Trim();
                if (name.Length > 0)
                    applied.Add(name);
            }

            return applied;
        }

        public static string GetCitrixLogPaths(string username, string applicationName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("==== CITRIX PROFILE PATHS ====");
            sb.Append(CitrixLogPaths.GetProfilePaths(username));
            sb.AppendLine();
            sb.AppendLine("==== CITRIX APP LOGS ====");
            sb.Append(CitrixLogPaths.GetEventViewerLogs(applicationName));
            return sb.ToString();
        }

        public static string DetectPublishedApp(int sessionId)
        {
            try
            {
                // Find all wfica32.exe processes in our session
                var wficaProcs = Process.GetProcessesByName("wfica32")
                                        .Where(p => p.SessionId == sessionId);

                foreach (var wfica in wficaProcs)
                {
                    // Query WMI for processes whose parent is this wfica32
                    string query = $"SELECT Name, ProcessId FROM Win32_Process WHERE ParentProcessId = {wfica.Id}";
                    var searcher = new ManagementObjectSearcher(query);
                    var children = searcher.Get()
                                           .Cast<ManagementObject>()
                                           .Select(mo => new {
                                               Name = mo["Name"]?.ToString(),
                                               Id = Convert.ToInt32(mo["ProcessId"])
                                           })
                                           .ToList();

                    // Pick the most recently-started child process if there are multiple
                    Process best = null;
                    foreach (var child in children)
                    {
                        try
                        {
                            var proc = Process.GetProcessById(child.Id);
                            if (best == null || proc.StartTime > best.StartTime)
                                best = proc;
                        }
                        catch { /* process may have exited */ }
                    }

                    if (best != null)
                        return best.ProcessName;
                }
            }
            catch
            {
                // fall-through to default
            }

            return "UnknownApp";
        }

        public static string RunCommand(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                return string.IsNullOrWhiteSpace(error) ? output : $"ERROR:\n{error}";
            }
        }

    }
}
