using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Server;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Console;
using HttpProtocolTinkering.Common.Request;
using HttpProtocolTinkering.Common.Response;

namespace HttpProtocolTinkering.Client
{
    public class UserAgent
    {
		public ResponseMessage SendRequest(OriginServer originServer, RequestMessage request)
		{
			WriteLine();
			WriteLine("Sending request:");
			WriteLine(request);

			var requestString = request.ToString();
			var responseString = originServer.AcceptRequest(requestString);
			var response = ResponseMessage.FromString(responseString);

			if(response.StatusLine.ProtocolVersion.MajorVersion != ProtocolVersion.HTTP11.MajorVersion)
			{
				throw new InvalidOperationException("Origin doesn't implement HTTP1 protocol properly");
			}

			return response;
		}
	}
}
