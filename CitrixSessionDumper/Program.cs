using System;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.ObjectModel;

namespace CitrixSessionDumper
{
    internal class Program
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
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string username = Environment.UserName;
            string domain = Environment.UserDomainName;
            string machine = Environment.MachineName;

            // === Run PoweerShell to get Citrix session info ===
            string clientIP = "Not Found";

            using (PowerShell ps = PowerShell.Create())
            {
                // This command pulls the client IP of the current user session
                ps.AddScript($@"
                    $session = Get-BrokerSession -SessionState Active | Where-Object {{ $_.UserName -like '*{username}' }}
                    if ($session) 
                    {{
                        $session.ClientIPAddress
                    }}
                    else 
                    {{
                        'No active Citrix session found'
                    }}
                ");

                try 
                {
                    Collection<PSObject> results = ps.Invoke();

                    if (results.Count > 0)
                    {
                        clientIP = results[0].ToString();
                    }
                }
                catch (Exception ex) 
                {
                    clientIP = "Error: " + ex.Message;
                }

                // === Output to console and log file ===
                Console.WriteLine("==== SESSION INFO ====");
                Console.WriteLine($"Timestamp: {timestamp}");
                Console.WriteLine($"Username: {username}");
                Console.WriteLine($"Domain: {domain}");
                Console.WriteLine($"Machine: {machine}");
                Console.WriteLine($"Client IP: {clientIP}");
                Console.WriteLine("======================");

                // Log it to file
                string logPath = @"C:\Logs\CitrixSessionDump.txt";
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
        }
    }
}
