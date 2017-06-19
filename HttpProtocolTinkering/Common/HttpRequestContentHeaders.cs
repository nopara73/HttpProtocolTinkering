using System.Net.Http.Headers;

namespace HttpProtocolTinkering.Common
{
	public struct HttpRequestContentHeaders
	{
		public HttpRequestHeaders RequestHeaders { get; set; }
		public HttpContentHeaders ContentHeaders { get; set; }
	}
}
