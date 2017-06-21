using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
		public string MessageBody { get; set; }

		public HttpMessage(string startLine, string headers = "", string messageBody = "")
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

			MessageBody = messageBody;
			if (messageBody == null || messageBody == "")
			{
				MessageBody = "";
			}
		}

		public override string ToString()
		{
			return StartLine + Headers + CRLF + MessageBody;
		}

		public static async Task<HttpMessage> FromStreamAsync(Stream message)
		{
			using (var reader = new StreamReader(message))
			{
				return await FromTextReaderAsync(reader).ConfigureAwait(false);
			}
		}

		public static async Task<HttpMessage> FromStringAsync(string message)
		{
			using (var reader = new StringReader(message))
			{
				return await FromTextReaderAsync(reader).ConfigureAwait(false);
			}
		}

		private static async Task<HttpMessage> FromTextReaderAsync(TextReader reader)
		{
			// https://tools.ietf.org/html/rfc7230#section-3
			// A recipient MUST parse an HTTP message as a sequence of octets in an
			// encoding that is a superset of US-ASCII[USASCII].

			// Read until the first CRLF
			// the CRLF is part of the startLine
			var startLine = await reader.ReadLineAsync(strictCRLF: true).ConfigureAwait(false) + CRLF;

			var headers = "";
			var firstRead = true;
			while (true)
			{
				var header = await reader.ReadLineAsync(strictCRLF: true).ConfigureAwait(false);
				if (header == null) throw new FormatException($"Malformed {nameof(HttpMessage)}: End of headers must be CRLF");
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
						throw new FormatException($"Invalid {nameof(HttpMessage)}: Cannot be whitespace between the start line and the headers");
					}
					firstRead = false;
				}

				headers += header + CRLF; // CRLF is part of the headerstring
			}

			// the rest is body
			var messageBody = await reader.ReadToEndAsync().ConfigureAwait(false);

			return new HttpMessage(startLine, headers, messageBody);
		}
	}
}
