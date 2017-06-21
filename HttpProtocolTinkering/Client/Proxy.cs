using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Server;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static HttpProtocolTinkering.Common.Constants;
using static System.Console;

namespace HttpProtocolTinkering.Client
{
	// https://tools.ietf.org/html/rfc7230#section-3.2.1
	// A proxy MUST forward unrecognized header fields unless the field-name
	// is listed in the Connection header field(Section 6.1) or the proxy
	// is specifically configured to block, or otherwise transform, such
	// fields.Other recipients SHOULD ignore unrecognized header fields.
	public class Proxy
    {
		public readonly HttpProtocol Protocol = HttpProtocol.HTTP11;

		public async Task<HttpResponseMessage> SendRequestAsync(OriginServer originServer, HttpRequestMessage request)
		{
			// https://tools.ietf.org/html/rfc7230#section-2.7.1
			// A sender MUST NOT generate an "http" URI with an empty host identifier.
			if (request.RequestUri.DnsSafeHost == "") throw new HttpRequestException("Host identifier is empty");

			// https://tools.ietf.org/html/rfc7230#section-2.6
			// Intermediaries that process HTTP messages (i.e., all intermediaries
			// other than those acting as tunnels) MUST send their own HTTP - version
			// in forwarded messages.
			request.Version = Protocol.Version;

			// https://tools.ietf.org/html/rfc7230#section-3.3.2
			// A user agent SHOULD send a Content - Length in a request message when
			// no Transfer-Encoding is sent and the request method defines a meaning
			// for an enclosed payload body.For example, a Content - Length header
			// field is normally sent in a POST request even when the value is 0
			// (indicating an empty payload body).A user agent SHOULD NOT send a
			// Content - Length header field when the request message does not contain
			// a payload body and the method semantics do not anticipate such a
			// body.
			// TODO implement it fully (altough probably .NET already ensures it)
			if(request.Method == HttpMethod.Post)
			{
				if (request.Headers.TransferEncoding.Count == 0)
				{
					if (request.Content == null)
					{
						request.Content = new ByteArrayContent(new byte[] { }); // dummy empty content
						request.Content.Headers.ContentLength = 0;
					}
					else
					{
						if (request.Content.Headers.ContentLength == null)
						{
							request.Content.Headers.ContentLength = (await request.Content.ReadAsStringAsync().ConfigureAwait(false)).Length;
						}
					}
				}
			}

			var requestString = await request.ToHttpStringAsync().ConfigureAwait(false);

			WriteLine("SENDING REQUEST...");
			Write(requestString);
			if (request.Content != null && await request.Content.ReadAsStringAsync().ConfigureAwait(false) != "")
			{
				WriteLine();
			}
			WriteLine("-------------------");

			using (var responseStream = await originServer.AcceptRequestAsync(requestString).ConfigureAwait(false))
			{
				var response = await new HttpResponseMessage().CreateNewAsync(responseStream, request.Method).ConfigureAwait(false);
				
				WriteLine("ARRIVING RESPONSE...");
				Write(await response.ToHttpStringAsync().ConfigureAwait(false));
				if (response.Content != null && await response.Content.ReadAsStringAsync().ConfigureAwait(false) != "")
				{
					WriteLine();
				}
				WriteLine("-------------------");

				if (response.Version.Major != request.Version.Major)
				{
					throw new HttpRequestException($"Origin server's HTTP major version differs: {nameof(response)}: {response.Version.Major} != {nameof(request)}: {request.Version.Major}");
				}

				return response;
			}
		}
	}
}
