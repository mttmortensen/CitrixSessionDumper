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
            int sessionId = CitrixInfoDump.ProcessSessionId();

            string username = CitrixInfoDump.GetWTSString(CitrixInfoDump.WTS_INFO_CLASS.WTSUserName, sessionId);
            string domain = CitrixInfoDump.GetWTSString(CitrixInfoDump.WTS_INFO_CLASS.WTSDomainName, sessionId);
            string machine = Environment.MachineName;
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string clientIP = CitrixInfoDump.GetClientIP(sessionId);
            string appName = CitrixInfoDump.DetectPublishedApp(sessionId);


            string logPath = @"C:\Logs\CitrixSesdsionDump.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            using (StreamWriter writer = new StreamWriter(logPath, true))
            {
                writer.WriteLine($"==========={username}'s Info===========");
                writer.WriteLine(CitrixInfoDump.GetSessionInfo(timestamp, username, domain, machine, clientIP));
                writer.WriteLine(CitrixInfoDump.GetProcessAndSystemInfo());

                var gpoGroup = CitrixInfoDump.GetAppliedGPOs();

                writer.WriteLine("==== APPLIED GPOs ====");
                var gpos = CitrixInfoDump.GetAppliedGPOs();
                if (gpos.Count > 0)
                {
                    foreach (var gpo in gpos)
                        writer.WriteLine($"{gpo}");
                }
                else
                {
                    writer.WriteLine("  (none)");
                }

                writer.WriteLine(CitrixInfoDump.GetCitrixLogPaths(username, appName));

                writer.WriteLine("======================");
                writer.WriteLine();

            }

            Console.WriteLine("Session dump Complete");
        }

    }
}
