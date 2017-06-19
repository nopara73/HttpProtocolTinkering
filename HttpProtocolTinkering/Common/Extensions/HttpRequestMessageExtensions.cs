using HttpProtocolTinkering.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using static HttpProtocolTinkering.Common.Constants;

namespace System.Net.Http
{
    public static class HttpRequestMessageExtensions
    {
		public static async Task<HttpRequestMessage> FromStringAsync(this HttpRequestMessage me, string requestString)
		{
			// https://tools.ietf.org/html/rfc7230#section-3
			// The normal procedure for parsing an HTTP message is to read the
			// start - line into a structure, read each header field into a hash table
			// by field name until the empty line, and then use the parsed data to
			// determine if a message body is expected.If a message body has been
			// indicated, then it is read as a stream until an amount of octets
			// equal to the message body length is read or the connection is closed.
			var message = await HttpMessage.FromStringAsync(requestString).ConfigureAwait(false);
			var requestLine = RequestLine.FromString(message.StartLine);

			var request = new HttpRequestMessage(requestLine.Method, requestLine.URI);

			var headerSection = HeaderSection.FromString(message.Headers);
			var headerStruct = headerSection.ToHttpRequestHeaders();
			if (headerStruct.RequestHeaders != null)
			{
				foreach (var header in headerStruct.RequestHeaders)
				{
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}

			var messageBody = new MessageBody(message.MessageBody, request.Headers, headerStruct.ContentHeaders);
			if (messageBody.Present)
			{
				request.Content = messageBody.ToHttpContent();
			}

			return request;
		}

		public static async Task<string> ToHttpStringAsync(this HttpRequestMessage me)
		{
			var startLine = new RequestLine(me.Method, me.RequestUri, new HttpProtocol($"HTTP/{me.Version.Major}.{me.Version.Minor}")).ToString();

			string headers = "";
			if (me.Headers != null && me.Headers.Count() != 0)
			{
				var headerSection = HeaderSection.FromHttpHeaders(me.Headers);
				headers += headerSection.ToString(endWithTwoCRLF: false);
			}		

			string messageBody = "";
			if (me.Content != null)
			{
				if (me.Content.Headers != null && me.Content.Headers.Count() != 0)
				{
					var headerSection = HeaderSection.FromHttpHeaders(me.Content.Headers);
					headers += headerSection.ToString(endWithTwoCRLF: false);
				}

				messageBody = await me.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			return new HttpMessage(startLine, headers, messageBody).ToString();
		}
	}
}
