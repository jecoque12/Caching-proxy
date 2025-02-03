using System.Net;
using System.Net.Http.Headers;

namespace caching_proxy
{
    public class Proxy
    {
        private string PORT = string.Empty;
        private string ORIGIN = string.Empty;

        Dictionary<string, CacheItem> cachedItems = new Dictionary<string, CacheItem>();

        public Proxy()
        {

        }

        public async Task Start(string port, string origin)
        {
            PORT = port;
            ORIGIN = origin;

            HttpListener listener = new HttpListener();

            listener.Prefixes.Add($"http://localhost:{PORT}/");
            listener.Start();


            while (true)
            {
                var context = await listener.GetContextAsync();

                var request = context.Request;

                var response = context.Response;

                var cacheKey = request.RawUrl ?? "/";


                Console.WriteLine($"Forwarding request to: {ORIGIN}{cacheKey}");
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

        }

        private void WriteResponse(string content, int statusCode, HttpResponseHeaders headers, HttpListenerContext context, bool cached)
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
        public void ClearCache()
        {
            Console.WriteLine("Clearing cache.....");
            Dictionary<string, CacheItem> cachedItems = new Dictionary<string, CacheItem>();
        }
    }
}
