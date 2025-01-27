// See https://aka.ms/new-console-template for more information
using caching_proxy;
using System.Net;
using System.Net.Http.Headers;
using System.Xml.Serialization;

Console.WriteLine("Hello, World!");


bool CorrecArgs = ParseArgs(args);
if (!CorrecArgs) return;
bool Run = true;
string PORT = args[1];
string ORIGIN = args[3];
Dictionary<string, CacheItem> cachedItems = new Dictionary<string, CacheItem>();

HttpListener listener = new HttpListener();

listener.Prefixes.Add($"http://localhost:{PORT}/");
listener.Start();

Console.WriteLine($"Server on port:{PORT}");




while (Run)
{
    var context = await listener.GetContextAsync();

    var request = context.Request;

    var response = context.Response;

    var cacheKey = request.RawUrl ?? "/";


    // If exist cache
    if (cachedItems.TryGetValue(cacheKey, out CacheItem cachedItem))
    {

        WriteResponse(cachedItem.Content, cachedItem.StatusCode, cachedItem.Headers, context, true);
        continue;
    }

    // No cache

    var client = new HttpClient() { BaseAddress = new Uri(ORIGIN) };


    var serverResponse = await client.GetAsync(cacheKey);

    var itemToCached = new CacheItem()
    {
        Content = await serverResponse.Content.ReadAsStringAsync(),
        StatusCode = ((int)serverResponse.StatusCode),
        Headers = serverResponse.Headers
    };



    cachedItems.Add(cacheKey, itemToCached);


    WriteResponse(itemToCached.Content, itemToCached.StatusCode, itemToCached.Headers, context, false);

}



bool ParseArgs(string[] args)
{
    try
    {
        if (args.Length == 0)
        {
            Console.WriteLine("You must set --port and --origin");
            return false;
        }
        if (!args[0].Equals(Options.Port) && !args[0].Equals(Options.abbreviationPort))
        {
            Console.WriteLine("Missing --port or -p");
            return false;
        }
        if (!args[2].Equals(Options.Origin) && !args[2].Equals(Options.abbreviationOrigin))
        {
            Console.WriteLine("Missing --origin or -o");
            return false;
        }
        if (string.IsNullOrEmpty(args[1]))
        {
            Console.WriteLine("Missing port value");
            return false;
        }
        if (string.IsNullOrEmpty(args[3]))
        {
            Console.WriteLine("Missing origin value");
            return false;
        }

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return false;
    }
}

void WriteResponse(string content, int statusCode, HttpResponseHeaders headers, HttpListenerContext context, bool cached)
{

    var response = context.Response;

    response.StatusCode = statusCode;

    foreach (var header in headers)
    {
        response.Headers.Add(header.Key, string.Join(",", header.Value));
    }

    if (cached)
    {
        response.Headers.Add("X-Cache", "HIT");
    }
    else
    {
        response.Headers.Add("X-Cache", "MISS");
    }

    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("<HTML><BODY> " + content + "</BODY></HTML>");
    // Get a response stream and write the response to it.
    response.ContentLength64 = buffer.Length;
    System.IO.Stream output = response.OutputStream;
    output.Write(buffer, 0, buffer.Length);
    output.Close();

}

void SerializeCachedItems()
{
    XmlSerializer serializerObj = new XmlSerializer(typeof(Dictionary<string, CacheItem>));

    StreamWriter writter = new StreamWriter("cachedItems");

    serializerObj.Serialize(writter, cachedItems);
    writter.Close();

}
class CacheItem
{
    public string Content { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public HttpResponseHeaders Headers { get; set; }
    //public DateTime Expiration { get; set; }

    //public bool IsExpired => DateTime.UtcNow > Expiration;
}
