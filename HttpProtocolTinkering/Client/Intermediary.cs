using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Server;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Console;

namespace HttpProtocolTinkering.Client
{
	public class Intermediary
    {
		public readonly HttpProtocol Protocol = HttpProtocol.HTTP11;

		public async Task<HttpResponseMessage> SendRequestAsync(OriginServer originServer, HttpRequestMessage request)
		{
			// https://tools.ietf.org/html/rfc7230#section-2.6
			// Intermediaries that process HTTP messages (i.e., all intermediaries
			// other than those acting as tunnels) MUST send their own HTTP - version
			// in forwarded messages.
			request.Version = Protocol.Version;

			var requestString = await request.ToHttpStringAsync().ConfigureAwait(false);
			
			WriteLine();
			WriteLine("Sending request:");
			WriteLine(requestString);

			var responseString = await originServer.AcceptRequestAsync(requestString).ConfigureAwait(false);

			WriteLine();
			WriteLine("Response arrived:");
			WriteLine(responseString);

			var response = await new HttpResponseMessage().FromStringAsync(responseString).ConfigureAwait(false);

			if (response.Version.Major != request.Version.Major)
			{
				throw new HttpRequestException($"Origin server's HTTP major version differs: {nameof(response)}: {response.Version.Major} != {nameof(request)}: {request.Version.Major}");
			}

			return response;
		}
	}
}
