using OpenQA.Selenium;
using Saltworks.EventHandling;
using Saltworks.Trace;
using Serilog;
using System.Reflection.Metadata;

namespace CivitParser.Model
{
    public class BaseCivitPageParser
    {
        private static TraceLogger _log = TraceManager.Logger<BaseCivitPageParser>();

        protected void UpdateProgress(ParseContext context, double new_progress_amount)
        {
            _log.Trace("In UpdateProgress");
            context.ProgressAmount = context.ProgressAmount + new_progress_amount;
            OnProgressUpdated(new ParseProgressEventArgs() { CurrentValue = context.ProgressAmount , MaxValue = 400});
            _log.Trace("Out UpdateProgress");
        }

        protected virtual void OnProgressUpdated(ParseProgressEventArgs e)
        {
            ProgressUpdated?.Invoke(this, e);
        }

        public event EventHandler<ParseProgressEventArgs>? ProgressUpdated;

        protected virtual void OnTaskChanged(string new_task)
        {
            TaskChanged?.Invoke(this, new GenericEventArgs<string>(new_task));
        }

        public event EventHandler<GenericEventArgs<string>>? TaskChanged;


        internal void BrowseTo(Uri uri, double delay_multiplier, ParseContext ctxt)
        {
            _log.Information("BrowseTo: {uri}", uri);
            ctxt.Driver.Navigate().GoToUrl(uri.ToString());
            System.Threading.Thread.Sleep((int)(ctxt.Settings.DefaultPageDelay * delay_multiplier));
        }

        internal void c(ParseContext ctxt)
        {
            try
            {
                ctxt.CancelToken.ThrowIfCancellationRequested();
            } catch (Exception)
            {
                _log.Warning("Cancel requested.");
                throw;
            }
        }
    }

    public class ParseProgressEventArgs : EventArgs
    {
        public double MaxValue { get; set; } = 0;
        public double CurrentValue { get; set; } = 0;
    }
}