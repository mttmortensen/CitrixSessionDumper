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
        static void Main(string[] args)
        {
            int sessionId = ProcessSessionId();

            string username = GetWTSString(WTS_INFO_CLASS.WTSUserName, sessionId);
            string domain = GetWTSString(WTS_INFO_CLASS.WTSDomainName, sessionId);
            string machine = Environment.MachineName;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string clientIP = GetClientIP(sessionId);

            string logPath = @"C:\Logs\CitrixSesdsionDump.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine($"==========={username}'s Info===========");
                writer.WriteLine(GetSessionInfo(timestamp, username, domain, machine, clientIP));
                writer.WriteLine(GetProcessAndSystemInfo());
                writer.WriteLine(GetGPODump());
                writer.WriteLine("======================");
                writer.WriteLine();
            }

            Console.WriteLine("Session dump Complete");
        }

    }
}
