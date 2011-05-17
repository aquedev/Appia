namespace Aqueduct.Appia.Host
{
    using System;
    using Nancy.Hosting.Self;
    using CommandLine;
    using System.Text;
    using Aqueduct.Appia.Core;
    using System.Reflection;
    using System.IO;

    class Program
    {
        static void Main(params string[] args)
        {
#if DEBUG
            Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "Aqueduct.Appia.Razor.dll"));
#endif

            var options = new Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);

            if (string.IsNullOrEmpty(options.ExportPath) == false)
            {
                var exporter = new HtmlExporter(options.ExportPath,
                                                    new Configuration(),
                                                    new Aqueduct.Appia.Core.Bootstrapper()) 
                                                    { Verbose = options.Verbose };
                exporter.Export();
            }
            else
            {
                var nancyHost = new NancyHost(new Uri(String.Format("http://{0}:{1}/", options.Address, options.Port))
                                            , new Aqueduct.Appia.Core.Bootstrapper());
                nancyHost.Start();

                Console.WriteLine(String.Format("Nancy now listening - navigate to http://{0}:{1}/. Press enter to stop", options.Address, options.Port));
                Console.ReadKey();

                nancyHost.Stop();

                Console.WriteLine("Stopped. Good bye!");
            }
        }
    }

    public class Options
    {
        [Option("v", null, HelpText = "Verbose gives you more debug information. Default: false")]
        public bool Verbose = false;

        [Option("a", "address", Required = false, HelpText = "The server's address. Default: localhost")]
        public string Address = "localhost";

        [Option("e", "export", HelpText = "Specify where you want to export the site. When the export path is specified the server will not serve pages")]
        public string ExportPath;

        [Option("p", "port", HelpText = "The server's port. Default: 8888")]
        public int Port = 8888;

        [HelpOption(HelpText = "Dispaly this help screen.")]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("Nancy standalone host");
            return usage.ToString();
        }
    }
}