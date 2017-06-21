using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

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
	public class HttpMessageHead
    {
		public string StartLine { get; set; }
		public string Headers { get; set; }

		public HttpMessageHead(string startLine, string headers = "")
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
			if (headers.EndsWith(CRLF + CRLF))
			{
				Headers = Headers.TrimEnd(CRLF, StringComparison.OrdinalIgnoreCase);
			}
		}

		public override string ToString()
		{
			return StartLine + Headers + CRLF;
		}
		
		public static async Task<HttpMessageHead> CreateNewAsync(Stream stream)
		{
			// https://tools.ietf.org/html/rfc7230#section-3
			// A recipient MUST parse an HTTP message as a sequence of octets in an
			// encoding that is a superset of US-ASCII[USASCII].
			using (var reader = new StreamReader(stream))
			{
				// Read until the first CRLF
				// the CRLF is part of the startLine
				var startLine = await reader.ReadLineAsync(strictCRLF: true).ConfigureAwait(false) + CRLF;

				var headers = "";
				var firstRead = true;
				while (true)
				{
					var header = await reader.ReadLineAsync(strictCRLF: true).ConfigureAwait(false);
					if (header == null) throw new FormatException($"Malformed {nameof(HttpMessageHead)}: End of headers must be CRLF");
					if (header == "")
					{
						// 2 CRLF was read in row so it's the end of the headers
						break;
					}

					if (firstRead)
					{
						// https://tools.ietf.org/html/rfc7230#section-3
						// A recipient that receives whitespace between the
						// start - line and the first header field MUST either reject the message
						// as invalid or
						if (Char.IsWhiteSpace(header[0]))
						{
							throw new FormatException($"Invalid {nameof(HttpMessageHead)}: Cannot be whitespace between the start line and the headers");
						}
						firstRead = false;
					}

					headers += header + CRLF; // CRLF is part of the headerstring
				}

				return new HttpMessageHead(startLine, headers);
			}
		}
	}
}
