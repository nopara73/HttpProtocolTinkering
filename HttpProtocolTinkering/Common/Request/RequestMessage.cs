using System;
using System.Collections.Generic;
using System.Linq;
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

		public static RequestMessage FromString(string requestString)
		{
			try
			{
				var lines = new List<string>(requestString.Split(CRLF.ToCharArray(), StringSplitOptions.None));
				// request line is the first line
				var requestLineString = lines[0];
				var requestLine = RequestLine.FromString(requestLineString);
				lines.RemoveAt(0);
				// header ends with an empty line
				var endOfheader = lines.FindIndex(x => x == "");
				var headerLines = lines.GetRange(0, endOfheader); // don't include the empty line
				var header = Header.FromString(string.Join(CRLF, headerLines)); // Will be reparsed, it can be optimized
				lines.RemoveRange(0, endOfheader + 1); // include the empty line
				
				// no need to send body
				if (lines.Count == 0)
				{
					return new RequestMessage(requestLine, header);
				}

				var body = string.Join(CRLF, lines); // body doesn't have to conform with CRLF ending, to restore the original body
				return new RequestMessage(requestLine, header, body);
			}
			catch(Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(RequestMessage)}", ex);
			}
		}
	}
}
