using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitParser.Model
{
    public record class ParseContext
    {
        public CivitParserSettings Settings { get; set; } = new CivitParserSettings();
        public CancellationToken CancelToken { get; set; } = default;
        public IWebDriver Driver { get; set; } = null;
        public Autofac.IContainer Container { get; set; } = null;
        public TextWriter Out { get; set; } = Console.Out;
        public TextWriter Error { get; set; } = Console.Error;
        public double ProgressAmount { get; set; } = 0.0;

        public ParseContext UpdateCancelToken(CancellationTokenSource source)
        {
            return new ParseContext() { Settings = this.Settings, Driver = this.Driver, Container=this.Container, CancelToken = source.Token, Out= this.Out, Error = this.Error };
        }

        public void WriteLineOut(string text, Saltworks.Trace.TraceLogger log)
        {
            log.Information(text);
            Out.WriteLine(text);
        }

        public void WriteLineError(string text, Saltworks.Trace.TraceLogger log)
        {
            log.Error(text);
            Error.WriteLine(text);
        }
    }
}
