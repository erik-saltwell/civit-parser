using Autofac;
using Autofac.Core;
using CommandLine;
using Polly.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Extensions.Autofac.DependencyInjection;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create your builder.
            ContainerBuilder builder = new ContainerBuilder();
            LoggerConfiguration log_configuration = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console();
            builder.RegisterSerilog(log_configuration);

            IContainer service = builder.Build();


            Parser.Default.ParseArguments<CommandLineArgurments>(args).WithParsed<CommandLineArgurments>(arguments =>
            {
                Console.WriteLine("ASDAASDASDASDASDASD");
            });
        }
    }
}
