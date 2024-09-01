namespace CivitParser.Model
{
    public class CivitParserSettings
    {
        public int DefaultPageDelay { get; set; } = 2000;
        public double ImageCollectionZoom { get; set; } = 7;
        public int LogonDelaySeconds { get; set; } = 25;
    }
}
