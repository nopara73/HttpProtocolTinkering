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
			var requestString = await request.ToHttpStringAsync().ConfigureAwait(false);

			WriteLine();
			WriteLine("Sending request:");
			WriteLine(requestString);

			var responseString = await originServer.AcceptRequestAsync(requestString).ConfigureAwait(false);

			WriteLine();
			WriteLine("Response arrived:");
			WriteLine(responseString);

			var response = new HttpResponseMessage().FromString(responseString);

			if(response.Version.Major != HttpProtocol.HTTP11.Version.Major)
			{
				throw new InvalidOperationException("Origin doesn't implement HTTP1 protocol properly");
			}

			return response;
		}
	}
}
