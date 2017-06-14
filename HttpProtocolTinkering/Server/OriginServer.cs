using System;
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
			var protocol = HttpProtocol.HTTP11;

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
