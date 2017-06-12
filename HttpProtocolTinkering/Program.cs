using HttpProtocolTinkering.Client;
using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Common.Request;
using HttpProtocolTinkering.Server;
using System;
using static System.Console;

namespace HttpProtocolTinkering
{
    class Program
    {
        static void Main(string[] args)
        {
			var originServer = new OriginServer();
			var userAgent = new UserAgent();

			var requestLine = new RequestLine(RequestType.GET, "foo", ProtocolVersion.HTTP11);
			var request = new RequestMessage(requestLine, new Header());
			var requestLine2 = new RequestLine(RequestType.GET, "foo2", ProtocolVersion.HTTP11);
			var wrongRequest = new RequestMessage(requestLine2, new Header());

			userAgent.SendRequest(originServer, request);
			userAgent.SendRequest(originServer, wrongRequest);

			WriteLine();
			WriteLine("Press a key to exit...");
			ReadKey();
        }
    }
}