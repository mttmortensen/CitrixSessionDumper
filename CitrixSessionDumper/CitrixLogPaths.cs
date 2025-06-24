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
        public static string GetVDASideLogs() { }
    }
}
