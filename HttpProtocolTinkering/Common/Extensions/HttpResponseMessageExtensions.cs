using HttpProtocolTinkering.Common;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HttpProtocolTinkering.Common.Constants;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
		public static async Task<HttpResponseMessage> CreateNewAsync(this HttpResponseMessage me, Stream responseStream, HttpMethod requestMethod)
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
			var reader = new StreamReader(stream: responseStream); // todo: dispose StreamReader, but leave open the requestStream
			var position = 0;
			string startLine = await HttpMessageHelper.ReadStartLineAsync(reader).ConfigureAwait(false);
			position += startLine.Length;

			var statusLine = StatusLine.CreateNew(startLine);
			var response = new HttpResponseMessage(statusLine.StatusCode);

			string headers = await HttpMessageHelper.ReadHeadersAsync(reader).ConfigureAwait(false);
			position += headers.Length + 2;
			
			var headerSection = HeaderSection.CreateNew(headers);
			var headerStruct = headerSection.ToHttpResponseHeaders();
			HttpMessageHelper.CopyHeaders(headerStruct.ResponseHeaders, response.Headers);

			var hasMessageBody = reader.Peek() != -1;
			HttpMessageHelper.AssertValidResponse(hasMessageBody, headerStruct.ResponseHeaders, statusLine.StatusCode, requestMethod);
			long? contentLength = headerStruct.ContentHeaders?.ContentLength;
			
			if (hasMessageBody)
			{
				// https://tools.ietf.org/html/rfc7230#section-3.3
				// The message body is
				// identical to the payload body unless a transfer coding has been applied.
				if (headerStruct.ResponseHeaders.TransferEncoding.Count == 0)
				{
					if (contentLength == null) throw new NotImplementedException();
					response.Content = new StreamContent(new SubStream(reader.BaseStream, position, (int)contentLength));
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
				response.Content = HttpMessageHelper.GetDummyOrNullContent(headerStruct.ContentHeaders);
			}

			if (response.Content != null)
			{
				HttpMessageHelper.CopyHeaders(headerStruct.ContentHeaders, response.Content.Headers);
			}
			return response;
		}

		public static async Task<Stream> ToStreamAsync(this HttpResponseMessage me)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(await me.ToHttpStringAsync().ConfigureAwait(false)));
		}
		public static async Task<string> ToHttpStringAsync(this HttpResponseMessage me)
		{
			var startLine = new StatusLine(new HttpProtocol($"HTTP/{me.Version.Major}.{me.Version.Minor}"), me.StatusCode).ToString();

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
