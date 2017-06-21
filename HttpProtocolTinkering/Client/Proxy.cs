﻿using HttpProtocolTinkering.Common;
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
				var response = await new HttpResponseMessage().CreateNewAsync(responseStream).ConfigureAwait(false);

				ValidateResponse(request, response);

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

		private static void ValidateResponse(HttpRequestMessage request, HttpResponseMessage response)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The presence of a message body in a response depends on both the
			// request method to which it is responding and the response status code
			// (Section 3.1.2).  Responses to the HEAD request method(Section 4.3.2
			// of[RFC7231]) never include a message body because the associated
			// response header fields(e.g., Transfer - Encoding, Content - Length,
			// etc.), if present, indicate only what their values would have been if
			// the request method had been GET(Section 4.3.1 of[RFC7231]). 2xx
			// (Successful) responses to a CONNECT request method(Section 4.3.6 of
			// [RFC7231]) switch to tunnel mode instead of having a message body.
			if (response.Content != null && response.Content.ReadAsStringAsync().Result != "")
			{
				if (request.Method == HttpMethod.Head)
				{
					throw new HttpRequestException("Response to HEAD method cannot include message body");
				}
				if (request.Method == new HttpMethod("CONNECT"))
				{
					if (HttpStatusCodeHelper.IsSuccessful(response.StatusCode))
					{
						throw new HttpRequestException("Response to CONNECT method with 2xx status code cannot include message body");
					}
				}
			}

			// https://tools.ietf.org/html/rfc7230#section-3.3.1
			// A server MUST NOT send a Transfer - Encoding header field in
			// any 2xx(Successful) response to a CONNECT request(Section 4.3.6 of
			// [RFC7231]).
			if (request.Method == new HttpMethod("CONNECT"))
			{
				if (response.Headers.Contains("Transfer-Encoding"))
				{
					if (HttpStatusCodeHelper.IsSuccessful(response.StatusCode))
					{
						throw new HttpRequestException("A server MUST NOT send a Transfer - Encoding header field in any 2xx(Successful) response to a CONNECT request");
					}
				}
			}
		}
	}
}
