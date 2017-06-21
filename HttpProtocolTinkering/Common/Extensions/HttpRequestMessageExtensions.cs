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
			string startLine = await HttpMessageHelper.ReadStartLineAsync(reader).ConfigureAwait(false);
			position += startLine.Length;

			var requestLine = RequestLine.CreateNew(startLine);
			var request = new HttpRequestMessage(requestLine.Method, requestLine.URI);

			string headers = await HttpMessageHelper.ReadHeadersAsync(reader).ConfigureAwait(false);
			position += headers.Length + 2;

			var headerSection = HeaderSection.CreateNew(headers);
			var headerStruct = headerSection.ToHttpRequestHeaders();
			HttpMessageHelper.CopyHeaders(headerStruct.RequestHeaders, request.Headers);

			var hasMessageBody = reader.Peek() != -1;
			long? contentLength = headerStruct.ContentHeaders?.ContentLength;
			if (headerStruct.RequestHeaders.TransferEncoding.Count == 0 && contentLength == null)
			{
				// https://tools.ietf.org/html/rfc7230#section-3.3
				// The presence of a message body in a request is signaled by a
				// Content - Length or Transfer-Encoding header field. 
				// Request message framing is independent of method semantics, even if the method does
				// not define any use for a message body.
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
						if (contentLength == null) throw new NotImplementedException();
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
					request.Content = HttpMessageHelper.GetDummyOrNullContent(headerStruct.ContentHeaders);
				}

				HttpMessageHelper.CopyHeaders(headerStruct.ContentHeaders, request.Content.Headers);
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
