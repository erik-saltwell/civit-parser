using Autofac;
using Autofac.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Polly.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using System.Xml;
using CivitParser.Model;
using Newtonsoft.Json;
using CivitParser;
using Saltworks.Trace;
using Spectre.Console;
using System.ComponentModel;
using System.Threading.Tasks;
using System;
using OpenQA.Selenium.Support.UI;

namespace ConsoleApp1
{
    internal class Program
    {
        private static TraceLogger _log = TraceManager.Logger<Program>();
        static void Main(string[] args)
        {
            // Create your builder.
            ContainerBuilder builder = new ContainerBuilder();
            TraceManager.AddSink(new DebugTraceSink());
            TraceManager.Enrich(new DateTimeEnricher());
            //            TraceManager.Enrich(new TypeNameEnricher());
            TraceManager.LogLevel = Microsoft.Extensions.Logging.LogLevel.Trace;

            TraceManager.AddArea(typeof(ResultSaver));
            TraceManager.AddArea(typeof(Program));
            TraceManager.AddArea(typeof(ImageCollectionPageParser));
            TraceManager.AddArea(typeof(ImagePageParser));
            TraceManager.AddArea(typeof(BaseCivitPageParser));


            builder.RegisterType<ImageCollectionPageParser>();
            builder.RegisterType<ImagePageParser>();

            Autofac.IContainer container = builder.Build();

            Run(container);
        }

        private static void Run(Autofac.IContainer container)
        {
            using (IWebDriver driver = new ChromeDriver(new ChromeOptions() { BinaryLocation = "d:\\dev\\tools\\chrome\\chrome.exe" }))
            {
                CancellationTokenSource cancel_source = new();
                ParseContext context = new ParseContext() { Driver = driver, CancelToken = cancel_source.Token, Container = container };

                System.IO.DirectoryInfo savedir = new DirectoryInfo("d:\\onboarding\\img\\forai\\");
                HashSet<Uri> urls = new HashSet<Uri>();
                List<ImageData> data = [];

                driver.Navigate().GoToUrl("https://civitai.com");
                WaitForLogon(context).Wait();

                AddNewImagePagesFromUserPages(urls, context, cancel_source);
                ExtractNewImagePagesFromCollectionPages(urls, context, cancel_source);
                data.AddRange(ExtractImageData(urls, context, cancel_source));

                ResultSaver saver = new ResultSaver();
                saver.SaveDirectory = savedir;
                saver.SaveImageData(data, context, cancel_source);
            }
        }


        private static IEnumerable<ImageData> ExtractImageData(HashSet<Uri> urls, ParseContext original_context, CancellationTokenSource cancel_source)
        {
            AnsiConsole.Markup("[underline blue]Processing Images[/]");
            ParseContext ctxt = original_context.UpdateCancelToken(cancel_source);
            List<ImageData> data = new List<ImageData>();
            using (var scope = ctxt.Container.BeginLifetimeScope())
            {
                ImagePageParser parser = scope.Resolve<ImagePageParser>();
                AnsiConsole.Progress()
                    .HideCompleted(false)
                    .Start(progress =>
                    {
                        var task = progress.AddTask("[yellow]Extracting ImageData[/]", true, urls.Count);

                        foreach (Uri uri in urls)
                        {
                            task.Increment(1);
                            ImageData imgData = parser.Parse(uri, ctxt);
                            data.Add(imgData);
                        }
                    });
            }
            return data;
        }

        private static void ExtractNewImagePagesFromCollectionPages(HashSet<Uri> urls, ParseContext original_context, CancellationTokenSource cancel_source)
        {
            AnsiConsole.Markup("[underline blue]Processing collection pages.[/]");
            ParseContext ctxt = original_context.UpdateCancelToken(cancel_source);
            using (var scope = ctxt.Container.BeginLifetimeScope())
            {
                ImageCollectionPageParser parser = scope.Resolve<ImageCollectionPageParser>();
                AnsiConsole.Progress()
                    .HideCompleted(false)
                    .Start(progress =>
                    {

                        // Define tasks
                        var outer_task = progress.AddTask("[yellow]All User Pages[/]", true, urls.Count);
                        var inner_task = progress.AddTask("[green]Current Page[/]", true, 400);

                        parser.ProgressUpdated += (sender, e) =>
                        {
                            inner_task.MaxValue = e.MaxValue;
                            inner_task.Value = e.CurrentValue;
                        };
                        parser.TaskChanged += (sender, e) =>
                        {
                            inner_task.Description = "[green]" + e.EventData + "[/]";
                        };

                        foreach (Uri page in UriSource.GetCollectionPages())
                        {
                            inner_task.Description = "[green]" + page.ToString() + "[/]";
                            foreach (Uri uri in parser.ParseFromCollectionUrl(page, ctxt))
                            {
                                if (!urls.Contains(uri))
                                {
                                    urls.Add(uri);
                                }
                            }
                            outer_task.Increment(1);
                        }
                    });
            }
        }

        private static void AddNewImagePagesFromUserPages(HashSet<Uri> urls, ParseContext original_context, CancellationTokenSource cancel_source)
        {
            AnsiConsole.Markup("[underline blue]Processing user pages.[/]");
            ParseContext ctxt = original_context.UpdateCancelToken(cancel_source);
            using (var scope = ctxt.Container.BeginLifetimeScope())
            {
                ImageCollectionPageParser parser = scope.Resolve<ImageCollectionPageParser>();
                List<Uri> user_pages = UriSource.GetUserPages();
                AnsiConsole.Progress()
                    .HideCompleted(false)
                    .Start(progress =>
                    {   
                        // Define tasks
                        var outer_task = progress.AddTask("[yellow]All User Pages[/]", true, user_pages.Count);
                        var inner_task = progress.AddTask("[green]Current Page[/]", true, 400);
                        parser.ProgressUpdated += (sender, e) =>
                        {
                            inner_task.MaxValue = e.MaxValue;
                            inner_task.Value = e.CurrentValue;
                        };
                        parser.TaskChanged += (sender, e) =>
                        {
                            inner_task.Description = "[green]" + e.EventData + "[/]";
                        };

                        foreach (Uri page in UriSource.GetUserPages())
                        {
                            inner_task.Description = "[green]" + page.ToString() + "[/]";
                            foreach (Uri uri in parser.ParseFromUserUrl(page, ctxt))
                            {
                                if (!urls.Contains(uri))
                                {
                                    urls.Add(uri);
                                }
                            }
                            outer_task.Increment(1);
                        }
                    });
            }
        }

        private static async Task WaitForLogon(ParseContext ctxt)
        {
            AnsiConsole.Markup("[underline blue]Waiting for login.[/]");

            await AnsiConsole.Progress()
                .HideCompleted(false)
                .StartAsync(async progress =>
                {
                    // Define tasks
                    var task1 = progress.AddTask( "[green]Waiting for logon[/]", true, ctxt.Settings.LogonDelaySeconds);
                    
                    while (!progress.IsFinished)
                    {
                        // Simulate some work
                        await Task.Delay(1000);

                        // Increment
                        task1.Increment(1);
                    }
                });
            Console.WriteLine();
        }


    }
}