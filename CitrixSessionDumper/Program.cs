using System;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.ObjectModel;

namespace CitrixSessionDumper
{
    class Program
    {

        // === Import WTS API 
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
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string machine = Environment.MachineName;

            string clientIP = GetClientIP(sessionId);

            Console.WriteLine("==== SESSION INFO ====");
            Console.WriteLine($"Timestamp: {timestamp}");
            Console.WriteLine($"Username: {username}");
            Console.WriteLine($"Domain: {domain}");
            Console.WriteLine($"Machine: {machine}");
            Console.WriteLine($"Client IP: {clientIP}");
            Console.WriteLine("======================");

            string logPath = @"C:\Logs\CitrixSesdsionDump.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine("==== SESSION INFO ====");
                writer.WriteLine($"Timestamp: {timestamp}");
                writer.WriteLine($"Username: {username}");
                writer.WriteLine($"Domain: {domain}");
                writer.WriteLine($"Machine: {machine}");
                writer.WriteLine($"Client IP: {clientIP}");
                writer.WriteLine("======================");
                writer.WriteLine();
            }
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

    }
}
