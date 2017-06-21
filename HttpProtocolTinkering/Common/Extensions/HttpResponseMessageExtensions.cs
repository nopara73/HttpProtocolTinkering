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
		public static async Task<HttpResponseMessage> CreateNewAsync(this HttpResponseMessage me, Stream responseString)
		{
			// https://tools.ietf.org/html/rfc7230#section-3
			// The normal procedure for parsing an HTTP message is to read the
			// start - line into a structure, read each header field into a hash table
			// by field name until the empty line, and then use the parsed data to
			// determine if a message body is expected.If a message body has been
			// indicated, then it is read as a stream until an amount of octets
			// equal to the message body length is read or the connection is closed.
			var message = await HttpMessage.CreateNewAsync(responseString).ConfigureAwait(false);
			var statusLine = StatusLine.CreateNew(message.StartLine);
				
			var response = new HttpResponseMessage(statusLine.StatusCode);

			var headerSection = HeaderSection.CreateNew(message.Headers);
			var headerStruct = headerSection.ToHttpResponseHeaders();
			if (headerStruct.ResponseHeaders != null)
			{
				foreach (var header in headerStruct.ResponseHeaders)
				{
					response.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}

			var messageBody = new MessageBody(message.MessageBody, response.Headers, headerStruct.ContentHeaders, statusLine.StatusCode);
			response.Content = messageBody.ToHttpContent();

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
