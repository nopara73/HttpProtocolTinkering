using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;
using System.Net;
using System.Net.Http;

namespace HttpProtocolTinkering.Common.Request
{
    public class RequestLine
    {
		public HttpMethod Method { get; set; }
		public Uri URI { get; set; }
		public HttpProtocol Protocol { get; set; }

		public RequestLine(HttpMethod method, Uri uri, HttpProtocol protocol)
		{
			Method = method;
			URI = uri;
			Protocol = protocol;
		}

		public override string ToString()
		{
			return Method.Method + SP + URI + SP + Protocol.ToString() + CRLF;
		}

		public static RequestLine FromString(string requestLineString)
		{
			try
			{
				requestLineString = requestLineString.TrimEnd(CRLF.ToCharArray()); // if there's CRLF at the end remove it

				var parts = new List<string>(requestLineString.Split(SP.ToCharArray(), StringSplitOptions.None));
				var methodString = parts[0];
				var uri = new Uri(parts[1]);
				var protocolString = parts[2];
				
				var method = Parsers.ToHttpMethod(methodString);
				var protocol = new HttpProtocol(protocolString);

				return new RequestLine(method, uri, protocol);
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(RequestLine)}", ex);
			}
		}
	}
}
