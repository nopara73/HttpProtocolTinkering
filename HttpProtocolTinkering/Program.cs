using HttpProtocolTinkering.Client;
using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Server;
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
			
			var uri = new HttpRequestUriBuilder(UriScheme.http, "foo").Uri;
			var requestLine = new RequestLine(HttpMethod.Get, uri, HttpProtocol.HTTP11);
			var request = new HttpRequestMessage(requestLine.Method, requestLine.URI);
			userAgent.SendRequestAsync(originServer, request).Wait();

			var uri2 = new HttpRequestUriBuilder(UriScheme.http, "foo2").Uri;
			var requestLine2 = new RequestLine(HttpMethod.Get, uri2, HttpProtocol.HTTP11);
			var wrongRequest = new HttpRequestMessage(requestLine2.Method, requestLine2.URI);

			userAgent.SendRequestAsync(originServer, wrongRequest).Wait();
			
			WriteLine();
			WriteLine("Press a key to exit...");
			ReadKey();
        }
    }
}