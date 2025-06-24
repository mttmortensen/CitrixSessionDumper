using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitrixSessionDumper
{
    // Provides methods to enumerate Citrix-related log paths on both client and VDA sides.
    public static class CitrixLogPaths
    {
        // Enumerates common client-side Citrix log directories and their contents.
        public static string GetClientSideLogs() 
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("-- Client-Side Citrix Log Directories --");

            // Common local app-data paths for Citrix Recieve/Workspace logs
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var clientDirs = new[] 
            {
                Path.Combine(localAppData, "Citrix", "Receiver", "SelfServicePlugin", "Logs"),
                Path.Combine(localAppData, "Citrix", "Workspace", "logs")
            };

            foreach (var dir in clientDirs)
            {
                sb.Append(dir);
                if (Directory.Exists(dir)) 
                {
                    sb.AppendLine(" (found) ");
                    foreach (var f in Directory.GetFiles(dir))
                        sb.AppendLine($"   • {Path.GetFileName(f)}");
                }
                else 
                {
                    sb.AppendLine(" (not found) ");
                }
            }

            return sb.ToString();
        }

        // Enumerates common VDA-side Citrix log directories and their contents.
        public static string GetVDASideLogs() 
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- VDA-side log directories (C:\\ProgramData\\Citrix\\*) --");

            // ProgramData path typically holds service-specific log folders
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string citrixPd = Path.Combine(programData, "Citrix");
            if (Directory.Exists(citrixPd))
            {
                foreach (var sub in Directory.GetDirectories(citrixPd))
                {
                    var logs = Path.Combine(sub, "Logs");
                    sb.Append(logs);
                    if (Directory.Exists(logs))
                    {
                        sb.AppendLine(" (found)");
                        foreach (var f in Directory.GetFiles(logs))
                            sb.AppendLine($"   • {Path.GetFileName(f)}");
                    }
                    else
                    {
                        sb.AppendLine(" (not found)");
                    }
                }
            }
            else
            {
                sb.AppendLine($"{citrixPd} (not found)");
            }

            sb.AppendLine();
            sb.AppendLine("-- VDA-side temp logs (C:\\Windows\\Temp\\Citrix) --");

            // Windows Temp Citrix folder for temmporary session logs
            var tempCitrix = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp", "Citrix");
            sb.Append(tempCitrix);
            if(Directory.Exists(tempCitrix)) 
            {
                sb.AppendLine(" (found)");
                foreach (var f in Directory.GetFiles(tempCitrix))
                    sb.AppendLine($"   • {Path.GetFileName(f)}");
            }
            else 
            {
                sb.AppendLine(" (not found)");
            }

            return sb.ToString();
        }
    }
}
