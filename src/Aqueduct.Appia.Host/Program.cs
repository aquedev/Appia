namespace Aqueduct.Appia.Host
{
    using System;
    using Nancy.Hosting.Self;
    using CommandLine;
    using System.Text;

    class Program
    {
        static void Main(params string[] args)
        {
            var options = new Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);

            var nancyHost = new NancyHost(new Uri(String.Format("http://{0}:{1}/", options.Address, options.Port))
                                            , new Aqueduct.Appia.Core.Bootstrapper());
            nancyHost.Start();

            Console.WriteLine(String.Format("Nancy now listening - navigate to http://{0}:{1}/. Press enter to stop", options.Address, options.Port));
            Console.ReadKey();

            nancyHost.Stop();

            Console.WriteLine("Stopped. Good bye!");
        }
    }

    public class Options
    {
        [Option("a", "address", Required = false, HelpText = "The server's address. Default: localhost")]
        public string Address = "localhost";

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