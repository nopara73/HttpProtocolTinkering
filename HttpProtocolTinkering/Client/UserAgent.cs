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
			return originServer.AcceptRequest(request);
		}
	}
}
