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
        }
    }
}
