using System;
using System.IO;
using System.Management.Automation;
using System.Collections.ObjectModel;
namespace CitrixSessionDumper
{
    internal class Program
    {
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
            }
        }
    }
}
