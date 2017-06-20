using HttpProtocolTinkering.Client;
using HttpProtocolTinkering.Common;
using HttpProtocolTinkering.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using static HttpProtocolTinkering.Common.Constants;
using static System.Console;

namespace HttpProtocolTinkering
{
	class Program
    {
        static void Main(string[] args)
		{
			var originServer = new OriginServer();
			var userAgent = new DummyUserAgent();

			var uriBuilder = new HttpRequestUriBuilder(UriScheme.http, "foo.com");

			var requestLine = new RequestLine(HttpMethod.Get, uriBuilder.BuildUri("foo"), HttpProtocol.HTTP11);
			var request = new HttpRequestMessage(requestLine.Method, requestLine.URI);

			request.Headers.TryAddWithoutValidation("moo", $"mee{CRLF} obsfolded");
			request.Headers.TryAddWithoutValidation("boo", "");
			request.Headers.TryAddWithoutValidation("soo", new string[] { "oo","koo"});
			request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.5) Gecko/20091102 Firefox/3.5.5 (.NET CLR 3.5.30729) ");
			request.Headers.TryAddWithoutValidation("Accept", " text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

			userAgent.SendRequestAsync(originServer, request).Wait();

			var requestLine2 = new RequestLine(HttpMethod.Get, uriBuilder.BuildUri("foo2"), HttpProtocol.HTTP11);
			var wrongRequest = new HttpRequestMessage(requestLine2.Method, requestLine2.URI);

			userAgent.SendRequestAsync(originServer, wrongRequest).Wait();

			var postRequestWithContent = new HttpRequestMessage(HttpMethod.Post, uriBuilder.BuildUri("boo"))
			{
				Content = new StringContent("bee")
			};
			userAgent.SendRequestAsync(originServer, postRequestWithContent).Wait();

			var headRequest = new HttpRequestMessage(HttpMethod.Head, uriBuilder.BuildUri("foo"));
			userAgent.SendRequestAsync(originServer, headRequest).Wait();

			var postRequestWithOut = new HttpRequestMessage(HttpMethod.Post, uriBuilder.BuildUri("boo"))
			{
				Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("foo", "bar") })
			};
			userAgent.SendRequestAsync(originServer, postRequestWithContent).Wait();

			ReadKey();
        }
    }
}