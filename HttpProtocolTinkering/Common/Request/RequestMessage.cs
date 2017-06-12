using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;

namespace HttpProtocolTinkering.Common.Request
{
	public class RequestMessage
	{
		public RequestLine RequestLine { get; set; }
		public Header Header { get; set; }
		public string Body { get; set; }

		public RequestMessage(RequestLine requestLine, Header header, string body = "")
		{
			RequestLine = requestLine;
			Header = header;
			Body = body;
		}

		public override string ToString()
		{
			return RequestLine.ToString() + Header.ToString() + Body;
		}
	}
}
