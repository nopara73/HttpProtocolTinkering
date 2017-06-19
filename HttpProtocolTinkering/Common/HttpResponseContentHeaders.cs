using System.Net.Http.Headers;

namespace HttpProtocolTinkering.Common
{
    public struct HttpResponseContentHeaders
	{
		public HttpResponseHeaders ResponseHeaders { get; set; }
		public HttpContentHeaders ContentHeaders { get; set; }
	}
}
