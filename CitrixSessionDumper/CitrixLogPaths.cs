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
    }
}
