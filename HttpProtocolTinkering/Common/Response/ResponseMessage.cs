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
	}
}
