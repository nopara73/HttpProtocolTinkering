using HttpProtocolTinkering.Client;
using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Server;
using System;
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

			var uriBuilder = new HttpRequestUriBuilder(UriScheme.http, "foo.com");

			var requestLine = new RequestLine(HttpMethod.Get, uriBuilder.BuildUri("foo"), HttpProtocol.HTTP11);
			var request = new HttpRequestMessage(requestLine.Method, requestLine.URI);
			userAgent.SendRequestAsync(originServer, request).Wait();
			
			var requestLine2 = new RequestLine(HttpMethod.Get, uriBuilder.BuildUri("foo2"), HttpProtocol.HTTP11);
			var wrongRequest = new HttpRequestMessage(requestLine2.Method, requestLine2.URI);

			userAgent.SendRequestAsync(originServer, wrongRequest).Wait();
			
			WriteLine();
			WriteLine("Press a key to exit...");
			ReadKey();
        }
    }
}