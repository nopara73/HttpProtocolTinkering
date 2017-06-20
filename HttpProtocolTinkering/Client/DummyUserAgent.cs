using HttpProtocolTinkering.Server;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpProtocolTinkering.Client
{
	public class DummyUserAgent
    {
		public async Task<HttpResponseMessage> SendRequestAsync(OriginServer originServer, HttpRequestMessage request)
		{
			var response = await new Proxy().SendRequestAsync(originServer, request).ConfigureAwait(false);

			return response;
		}
	}
}
