using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Markus.Feedback.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();
            var contentRoot = config["contentRoot"] ?? Directory.GetCurrentDirectory();
            if (!Path.IsPathRooted(contentRoot))
            {
                contentRoot = Path.Combine(Directory.GetCurrentDirectory(), contentRoot);
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(contentRoot)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
