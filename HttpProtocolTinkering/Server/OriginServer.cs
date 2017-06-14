﻿using System;
using System.Collections.Generic;
using System.Text;
using HttpProtocolTinkering.Common;
using static System.Console;
using HttpProtocolTinkering.Common.Response;
using HttpProtocolTinkering.Common.Request;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace HttpProtocolTinkering.Server
{
	public class OriginServer
	{
		public async Task<string> AcceptRequestAsync(string requestString)
		{
			var request = new HttpRequestMessage().FromString(requestString);
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
				return await new HttpResponseMessage(HttpStatusCode.HttpVersionNotSupported).ToHttpStringAsync().ConfigureAwait(false);
			}

			var response = new HttpResponseMessage(HttpStatusCode.OK);

			if (request.Method == HttpMethod.Get)
			{
				if (request.RequestUri == new UriBuilder("http", "foo").Uri)
				{
					response.Content = new StringContent("bar");
					return await response.ToHttpStringAsync().ConfigureAwait(false);
				}
			}
			
			return await new HttpResponseMessage(HttpStatusCode.BadRequest).ToHttpStringAsync().ConfigureAwait(false);
		}
	}
}
