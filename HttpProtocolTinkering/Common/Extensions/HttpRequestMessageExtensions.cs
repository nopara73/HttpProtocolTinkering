using HttpProtocolTinkering.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static HttpProtocolTinkering.Common.Constants;

namespace System.Net.Http
{
    public static class HttpRequestMessageExtensions
    {
		public static async Task<HttpRequestMessage> CreateNewAsync(this HttpRequestMessage me, Stream requestStream)
		{
			// https://tools.ietf.org/html/rfc7230#section-3
			// The normal procedure for parsing an HTTP message is to read the
			// start - line into a structure, read each header field into a hash table
			// by field name until the empty line, and then use the parsed data to
			// determine if a message body is expected.If a message body has been
			// indicated, then it is read as a stream until an amount of octets
			// equal to the message body length is read or the connection is closed.

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
			var reader = new StreamReader(stream: requestStream); // todo: dispose StreamReader, but leave open the requestStream
			var position = 0;
			// https://tools.ietf.org/html/rfc7230#section-3
			// A recipient MUST parse an HTTP message as a sequence of octets in an
			// encoding that is a superset of US-ASCII[USASCII].

			// Read until the first CRLF
			// the CRLF is part of the startLine
			var startLine = await reader.ReadLineAsync(strictCRLF: true).ConfigureAwait(false) + CRLF;
			position += startLine.Length + 2;
			if (startLine == null || startLine == "") throw new FormatException($"{nameof(startLine)} cannot be null or empty");

			var requestLine = RequestLine.CreateNew(startLine);
			var request = new HttpRequestMessage(requestLine.Method, requestLine.URI);

			var headers = "";
			var firstRead = true;
			while (true)
			{
				var header = await reader.ReadLineAsync(strictCRLF: true).ConfigureAwait(false);
				position += header.Length + 2;
				if (header == null) throw new FormatException($"Malformed {nameof(HttpMessage)}: End of headers must be CRLF");
				if (header == "")
				{
					position -= 2;
					// 2 CRLF was read in row so it's the end of the headers
					break;
				}

				if (firstRead)
				{
					// https://tools.ietf.org/html/rfc7230#section-3
					// A recipient that receives whitespace between the
					// start - line and the first header field MUST either reject the message
					// as invalid or consume each whitespace-preceded line without further
					// processing of it(i.e., ignore the entire line, along with any				 
					// subsequent lines preceded by whitespace, until a properly formed				 
					// header field is received or the header section is terminated).
					if (Char.IsWhiteSpace(header[0]))
					{
						throw new FormatException($"Invalid {nameof(HttpMessage)}: Cannot be whitespace between the start line and the headers");
					}
					firstRead = false;
				}

				headers += header + CRLF; // CRLF is part of the headerstring
			}
			if (headers == null || headers == "")
			{
				headers = "";
			}
			var headerSection = HeaderSection.CreateNew(headers);
			var headerStruct = headerSection.ToHttpRequestHeaders();
			if (headerStruct.RequestHeaders != null)
			{
				foreach (var header in headerStruct.RequestHeaders)
				{
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}

			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The presence of a message body in a request is signaled by a
			// Content - Length or Transfer-Encoding header field. 
			// Request message framing is independent of method semantics, even if the method does
			// not define any use for a message body.
			var hasMessageBody = reader.Peek() != -1;
			long? contentLength = headerStruct.ContentHeaders?.ContentLength;
			if (headerStruct.RequestHeaders.TransferEncoding.Count == 0 && contentLength == null)
			{
				if (hasMessageBody)
				{
					throw new HttpRequestException("Message body is not indicated in the headers, yet it's present");
				}
				request.Content = null;
			}
			else
			{
				if (hasMessageBody)
				{
					// https://tools.ietf.org/html/rfc7230#section-3.3
					// The message body is
					// identical to the payload body unless a transfer coding has been applied.
					if (headerStruct.RequestHeaders.TransferEncoding.Count == 0)
					{
						request.Content = new StreamContent(new SubStream(reader.BaseStream, position, (int)contentLength));
					}
					else
					{
						// https://tools.ietf.org/html/rfc7230#section-3.3.2
						// A sender MUST NOT send a Content - Length header field in any message
						// that contains a Transfer-Encoding header field.
						if (contentLength == null)
						{
							throw new HttpRequestException("A sender MUST NOT send a Content-Length header field in any message that contains a Transfer-Encoding header field.");
						}

						throw new NotImplementedException();
					}
				}
				else
				{
					if (headerStruct.ContentHeaders != null)
					{
						request.Content = new ByteArrayContent(new byte[] { }); // dummy empty content
					}
					else
					{
						request.Content = null;
					}
				}

				if (headerStruct.ContentHeaders != null)
				{
					foreach (var header in headerStruct.ContentHeaders)
					{
						request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
					}
				}
			}

			return request;

		}

		public static async Task<string> ToHttpStringAsync(this HttpRequestMessage me)
		{
			var startLine = new RequestLine(me.Method, me.RequestUri, new HttpProtocol($"HTTP/{me.Version.Major}.{me.Version.Minor}")).ToString();

			string headers = "";
			if (me.Headers != null && me.Headers.Count() != 0)
			{
				var headerSection = HeaderSection.CreateNew(me.Headers);
				headers += headerSection.ToString(endWithTwoCRLF: false);
			}		

			string messageBody = "";
			if (me.Content != null)
			{
				if (me.Content.Headers != null && me.Content.Headers.Count() != 0)
				{
					var headerSection = HeaderSection.CreateNew(me.Content.Headers);
					headers += headerSection.ToString(endWithTwoCRLF: false);
				}

				messageBody = await me.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			return new HttpMessage(startLine, headers, messageBody).ToString();
		}
	}
}
