using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Server;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;
using HttpProtocolTinkering.Common.Request;
using HttpProtocolTinkering.Common.Response;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpProtocolTinkering.Client
{
    public class UserAgent
    {
		public async Task<HttpResponseMessage> SendRequestAsync(OriginServer originServer, HttpRequestMessage request)
		{
			Intermediary intermediary = new Intermediary();

			var response = await intermediary.SendRequestAsync(originServer, request).ConfigureAwait(false);

			if(response.Version.Major != request.Version.Major)
			{
				throw new HttpRequestException($"Origin server's HTTP major version differs: {nameof(response)}: {response.Version.Major} != {nameof(request)}: {request.Version.Major}");
			}

			return response;
		}
	}
}
