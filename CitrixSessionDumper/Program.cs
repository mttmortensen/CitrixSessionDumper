using System;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CitrixSessionDumper
{
    class Program
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
        enum WTS_INFO_CLASS
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

        static void Main(string[] args)
        {
            int sessionId = ProcessSessionId();

            string username = GetWTSString(WTS_INFO_CLASS.WTSUserName, sessionId);
            string domain = GetWTSString(WTS_INFO_CLASS.WTSDomainName, sessionId);
            string machine = Environment.MachineName;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string clientIP = GetClientIP(sessionId);


            Console.WriteLine(GetSessionInfo(timestamp, username, domain, machine, clientIP));
            Console.WriteLine(GetProcessAndSystemInfo());

            string logPath = @"C:\Logs\CitrixSesdsionDump.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine(GetSessionInfo(timestamp, username, domain, machine, clientIP));
                writer.WriteLine(GetProcessAndSystemInfo());
                writer.WriteLine("======================");
                writer.WriteLine();
            }

            Console.WriteLine("Session dump Complete");
        }

        static int ProcessSessionId()
        {
            return System.Diagnostics.Process.GetCurrentProcess().SessionId;
        }

        static string GetWTSString(WTS_INFO_CLASS infoClass, int sessionId)
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

        static string GetClientIP(int sessionId)
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

        static string GetProcessAndSystemInfo() 
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

        static string GetSessionInfo(string timestamp, string username, string domain, string machine, string clientIP)
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

        static string GetGPODump() 
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" ==== GPO RESULTS (USER SCOPE) ====");

            try { }
            catch (Exception ex) { }

            return sb.ToString();
        }
    }
}
