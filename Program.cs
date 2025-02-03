// See https://aka.ms/new-console-template for more information
using caching_proxy;
using CommandLine;


Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
{
    Task.Run(async () =>
    {
        var proxy = new Proxy();

        if (o.ClearCache)
        {
            proxy.ClearCache();
        }
        else if (!string.IsNullOrEmpty(o.ORIGIN) && !string.IsNullOrEmpty(o.PORT))
        {
            Console.WriteLine($"Server on localhost:{o.PORT}");
            Console.WriteLine($"Origin:{o.ORIGIN}");

            await proxy.Start(o.PORT, o.ORIGIN);
        }
        else
        {
            Console.WriteLine("Options are missing. Be sure to include --port and --origin.");
        }
    }).GetAwaiter().GetResult();

}).WithNotParsed(errors =>
{
    Console.WriteLine("Error in arguments supplied.");
});







