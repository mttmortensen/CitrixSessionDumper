using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CitrixSessionDumper
{
    // Provides methods to get user profile share paths and event log entries for a specified application.
    public static class CitrixLogPaths
    {
        // Returns UNC paths for the user's profile on old and current Citrix farms.
        public static string GetProfilePaths(string username)
        {
            var sb = new StringBuilder();
            sb.AppendLine("==== PROFILE PATHS ====");

            // Path for the legacy XenApp farm profiles
            var oldFarmPath = $"\\4life.com\\shares\\Profiles\\UserProfiles\\{username}";
            sb.AppendLine($"Old Farm Profile: {oldFarmPath}");

            // Path for the current CVAD farm profiles
            var currentFarmPath = $"\\4life.com\\shares\\UserProfiles$\\{username}";
            sb.AppendLine($"Current Farm Profile: {currentFarmPath}");

            return sb.ToString();
        }

        // Retrieves Error and Warning entries from the Application log for the given source.
        public static string GetEventViewerLogs(string source)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"==== EVENT LOG ({source}) ERRORS & WARNINGS ====");
            try
            {
                using (var log = new EventLog("Application"))
                {
                    var entries = log.Entries.Cast<EventLogEntry>()
                        .Where(e => (e.EntryType == EventLogEntryType.Error || e.EntryType == EventLogEntryType.Warning)
                                    && e.Source.Equals(source, StringComparison.OrdinalIgnoreCase));

                    if (!entries.Any())
                    {
                        sb.AppendLine("No entries found.");
                    }
                    else
                    {
                        foreach (var entry in entries)
                            sb.AppendLine($"[{entry.TimeGenerated}] {entry.EntryType} - {entry.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error querying event log: {ex.Message}");
            }

            sb.AppendLine("======================");
            return sb.ToString();
        }
    }
}
