using HttpProtocolTinkering.Common;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;

namespace HttpProtocolTinkering.Server
{
	public class OriginServer
	{
		// https://tools.ietf.org/html/rfc7230#section-3.1.1
		// Various ad hoc limitations on request-line length are found in
		// practice.It is RECOMMENDED that all HTTP senders and recipients
		// support, at a minimum, request-line lengths of 8000 octets.
		public int UriLength = 8190; // This is the default at Apache, let's use this

		public async Task<Stream> AcceptRequestAsync(string requestString)
		{
			var responseBadRequest = new HttpResponseMessage(HttpStatusCode.BadRequest);
			try
			{
				using (var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(requestString)))
				{
					var request = await new HttpRequestMessage().CreateNewAsync(requestStream).ConfigureAwait(false);
					// https://tools.ietf.org/html/rfc7230#section-2.6
					// A server SHOULD send a response version equal to the highest version
					// to which the server is conformant that has a major version less than
					// or equal to the one received in the request.
					var protocol = HttpProtocol.HTTP11;

					// https://tools.ietf.org/html/rfc7230#section-2.6
					// A server MUST NOT send a version to which it is not conformant.
					// A server can send a 505 (HTTP Version Not Supported) response if it wishes, for any reason,
					// to refuse service of the client's major protocol version.			
					if (request.Version.Major != protocol.Version.Major)
					{
						return await new HttpResponseMessage(HttpStatusCode.HttpVersionNotSupported).ToStreamAsync().ConfigureAwait(false);
					}

					// https://tools.ietf.org/html/rfc7230#section-2.7.1
					// A recipient that processes such a URI reference [without hostname] MUST reject it as invalid.
					if (request.RequestUri.DnsSafeHost == "")
					{
						return await responseBadRequest.ToStreamAsync().ConfigureAwait(false);
					}

					// https://tools.ietf.org/html/rfc7230#section-3.1.1
					// A server that receives a request-target longer than any URI it wishes to parse MUST respond
					// with a 414(URI Too Long) status code
					if (request.RequestUri.ToString().Length > UriLength)
					{
						return await responseBadRequest.ToStreamAsync().ConfigureAwait(false);
					}

					var response = new HttpResponseMessage(HttpStatusCode.OK);

					var uriBuilder = new HttpRequestUriBuilder(UriScheme.http, "foo.com");
					if (request.Method == HttpMethod.Get)
					{
						if (request.RequestUri == uriBuilder.BuildUri("foo"))
						{
							response.Content = new StringContent("bar");
							return await response.ToStreamAsync().ConfigureAwait(false);
						}
					}

					if (request.Method == HttpMethod.Post)
					{
						if (request.RequestUri == uriBuilder.BuildUri("boo"))
						{
							response.Content = request.Content;
							return await response.ToStreamAsync().ConfigureAwait(false);
						}
					}

					if (request.Method == HttpMethod.Head)
					{
						if (request.RequestUri == uriBuilder.BuildUri("foo"))
						{
							// https://tools.ietf.org/html/rfc7230#section-3.3.2
							// A server MAY send a Content-Length header field in a response to a
							// HEAD request(Section 4.3.2 of[RFC7231]);
							// a server MUST NOT send
							// Content - Length in such a response unless its field-value equals the
							// decimal number of octets that would have been sent in the payload
							// body of a response if the same request had used the GET method.
							response.Content = new ByteArrayContent(new byte[] { }); // dummy empty content
							response.Content.Headers.ContentLength = new StringContent("bar").Headers.ContentLength;
							return await response.ToStreamAsync().ConfigureAwait(false);
						}
					}

					return await responseBadRequest.ToStreamAsync().ConfigureAwait(false);
				}
			}
			catch(Exception ex)
			{
				return await responseBadRequest.ToStreamAsync().ConfigureAwait(false);
			}
		}
	}
}
