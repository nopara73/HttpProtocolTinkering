using HttpProtocolTinkering.Common;
using System;
using System.Linq;
using System.Threading.Tasks;
using static HttpProtocolTinkering.Common.Constants;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
		public static async Task<HttpResponseMessage> FromStringAsync(this HttpResponseMessage me, string responseString)
		{
			// https://tools.ietf.org/html/rfc7230#section-3
			// The normal procedure for parsing an HTTP message is to read the
			// start - line into a structure, read each header field into a hash table
			// by field name until the empty line, and then use the parsed data to
			// determine if a message body is expected.If a message body has been
			// indicated, then it is read as a stream until an amount of octets
			// equal to the message body length is read or the connection is closed.
			try
			{
				var message = await HttpMessage.FromStringAsync(responseString).ConfigureAwait(false);
				var statusLine = StatusLine.FromString(message.StartLine);
				
				var response = new HttpResponseMessage(statusLine.StatusCode);

				if (message.Headers != "")
				{
					var headers = HeaderSection.FromString(message.Headers);
					foreach(var header in headers.ToHttpResponseHeaders())
					{
						response.Headers.Add(header.Key, header.Value);
					}
				}

				if (message.Body != "")
				{
					response.Content = new StringContent(message.Body);
				}

				return response;
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(HttpResponseMessage)}", ex);
			}
		}

		public static async Task<string> ToHttpStringAsync(this HttpResponseMessage me)
		{
			var startLine = new StatusLine(new HttpProtocol($"HTTP/{me.Version.Major}.{me.Version.Minor}"), me.StatusCode).ToString();

			string headers = "";
			if(me.Headers != null && me.Headers.Count() != 0)
			{
				var headerSection = HeaderSection.FromHttpHeaders(me.Headers);
				headers = headerSection.ToString(endWithCRLF: true);
			}

			string body = "";
			if (me.Content != null)
			{
				body = await me.Content.ReadAsStringAsync().ConfigureAwait(false);
			}

			return new HttpMessage(startLine, headers, body).ToString();			
		}
	}
}
