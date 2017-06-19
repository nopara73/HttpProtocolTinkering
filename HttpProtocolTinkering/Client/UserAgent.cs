using HttpProtocolTinkering.Server;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpProtocolTinkering.Client
{
	public class UserAgent
    {
		public async Task<HttpResponseMessage> SendRequestAsync(OriginServer originServer, HttpRequestMessage request)
		{
			// https://tools.ietf.org/html/rfc7230#section-2.7.1
			// A sender MUST NOT generate an "http" URI with an empty host identifier.
			if (request.RequestUri.DnsSafeHost == "") throw new HttpRequestException("Host identifier is empty");

			Proxy proxy = new Proxy();

			var response = await proxy.SendRequestAsync(originServer, request).ConfigureAwait(false);

			if(response.Version.Major != request.Version.Major)
			{
				throw new HttpRequestException($"Origin server's HTTP major version differs: {nameof(response)}: {response.Version.Major} != {nameof(request)}: {request.Version.Major}");
			}

			return response;
		}
	}
}
