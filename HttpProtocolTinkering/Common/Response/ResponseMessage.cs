using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;

namespace HttpProtocolTinkering.Common.Response
{
    public class ResponseMessage
    {
		public StatusLine StatusLine { get; set; }
		public Header Header { get; set; }
		public string Body { get; set; }

		public ResponseMessage(StatusLine statusLine, Header header = null, string body = "")
		{
			StatusLine = statusLine;
			Header = header;
			Body = body;
		}

		public override string ToString()
		{
			var ret = StatusLine.ToString();
			if (Header != null)
			{
				ret += Header.ToString();
			}
			else
			{
				ret += CRLF;
			}

			ret += Body;

			return ret;
		}

		public static ResponseMessage FromString(string requestString)
		{
			try
			{
				var lines = new List<string>(requestString.Split(CRLF.ToCharArray(), StringSplitOptions.None));
				// request line is the first line
				var statusLineString = lines[0];
				lines.RemoveAt(0);
				StatusLine statusLine = StatusLine.FromString(statusLineString);

				// server might only send status line
				if (lines.Count == 0)
				{
					return new ResponseMessage(statusLine);
				}

				// header ends with an empty line
				var endOfheader = lines.FindIndex(x => x == "");
				var headerLines = lines.GetRange(0, endOfheader); // don't include the empty line
				lines.RemoveRange(0, endOfheader + 1); // include the empty line
				var header = Header.FromString(string.Join(CRLF, headerLines)); // Will be reparsed, it can be optimized
				
				// no need to send body
				if (lines.Count == 0)
				{
					return new ResponseMessage(statusLine, header);
				}

				var body = string.Join(CRLF, lines); // body doesn't have to conform with CRLF ending, to restore the original body
				return new ResponseMessage(statusLine, header, body);
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(ResponseMessage)}", ex);
			}
		}
	}
}
