using CommandLine;

namespace caching_proxy
{
    public class Options
    {

        [Option("port", Required = true, HelpText = "--port <PORT>")]
        public string PORT { get; set; } = string.Empty;
        [Option("origin", Required = true, HelpText = "--origin <ORIGIN>")]
        public string ORIGIN { get; set; } = string.Empty;
        [Option("clear-cache", Required = false, HelpText = "--clear-cache")]
        public bool ClearCache { get; set; }
    }
}
