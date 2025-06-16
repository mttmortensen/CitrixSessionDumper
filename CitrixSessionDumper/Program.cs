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

            using(PowerShell ps = PowerShell.Create()) 
            {

            }
        }
    }
}
