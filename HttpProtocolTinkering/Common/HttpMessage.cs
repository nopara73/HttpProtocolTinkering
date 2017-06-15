using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Linq;

namespace HttpProtocolTinkering.Common
{
	// https://tools.ietf.org/html/rfc7230#section-3
	// All HTTP/ 1.1 messages consist of a start - line followed by a sequence
	// of octets in a format similar to the Internet Message Format
	// [RFC5322]: zero or more header fields(collectively referred to as
	// the "headers" or the "header section"), an empty line indicating the
	// end of the header section, and an optional message body.
	// HTTP - message = start - line
	//					* (header - field CRLF )
	//					CRLF
	//					[message - body]
	public class HttpMessage
    {
		public string StartLine { get; set; }
		public string Headers { get; set; }
		public string Body { get; set; }

		public HttpMessage(string startLine, string headers = "", string body = "")
		{
			if (startLine == null || startLine == "") throw new FormatException($"{nameof(startLine)} cannot be null or empty");

			StartLine = startLine;
			if (!startLine.EndsWith(CRLF))
			{
				StartLine += CRLF;
			}

			Headers = headers;
			if (headers == null || headers == "")
			{
				Headers = "";
			}
			else if (!headers.EndsWith(CRLF))
			{
				Headers += CRLF;
			}

			Body = body;
			if (body == null || body == "")
			{
				Body = "";
			}
		}

		public override string ToString()
		{
			return StartLine + Headers + CRLF + Body;
		}

		public static HttpMessage FromString(string message)
		{
			int startLineEnd = message.IndexOf(CRLF);
			int headersStart = startLineEnd + 2;
			string startLine = message.Substring(0, headersStart); // the CRLF is part of the startLine

			if (message.Length == headersStart)
			{
				return new HttpMessage(startLine);
			}
			else
			{
				// https://tools.ietf.org/html/rfc7230#section-3
				// A recipient that receives whitespace between the
				// start - line and the first header field MUST either reject the message
				// as invalid or
				if (Char.IsWhiteSpace(message[headersStart]))
				{
					throw new FormatException($"Invalid {nameof(HttpMessage)}: Cannot be whitespace between the start line and the headers");
				}
			}

			int headersEnd = message.IndexOf(CRLF + CRLF, startIndex: headersStart);
			string headers = message.Substring(headersStart, (headersEnd - startLineEnd) + 2); // the second CRLF not part of the headers

			int bodyStart = headersEnd + 4;
			if (bodyStart == message.Length) return new HttpMessage(startLine, headers);
			string body = message.Substring(bodyStart, message.Length - bodyStart); // neither the body

			return new HttpMessage(startLine, headers, body);
		}
	}
}
