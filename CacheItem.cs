using System.Net.Http.Headers;

namespace caching_proxy
{
    public class CacheItem
    {
        public string Content { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }

    }
}
