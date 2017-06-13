using HttpProtocolTinkering.Client;
using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Common.Request;
using HttpProtocolTinkering.Server;
using System;
using System.Net;
using System.Net.Http;
using static System.Console;

namespace HttpProtocolTinkering
{
    class Program
    {
        static void Main(string[] args)
        {
			var originServer = new OriginServer();
			var userAgent = new UserAgent();
			
			var requestLine = new RequestLine(HttpMethod.Get, new UriBuilder("http", "foo").Uri, HttpProtocol.HTTP11);
			var request = new RequestMessage(requestLine, new Header());
			var requestLine2 = new RequestLine(HttpMethod.Get, new UriBuilder("http", "foo2").Uri, HttpProtocol.HTTP11);
			var wrongRequest = new RequestMessage(requestLine2, new Header());

			userAgent.SendRequestAsync(originServer, request).Wait();
			userAgent.SendRequestAsync(originServer, wrongRequest).Wait();

			WriteLine();
			WriteLine("Press a key to exit...");
			ReadKey();
        }
    }
}