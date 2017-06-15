using HttpProtocolTinkering.Common;
using System;
using System.Threading.Tasks;
using static HttpProtocolTinkering.Common.Constants;

namespace System.Net.Http
{
    public static class HttpRequestMessageExtensions
    {
		public static HttpRequestMessage FromString(this HttpRequestMessage me, string requestString)
		{
			try
			{
				int statusLineEnd = requestString.IndexOf(CRLF);
				RequestLine requestLine = RequestLine.FromString(requestString.Substring(0, statusLineEnd));
				requestString = requestString.Remove(0, statusLineEnd + 2);

				// server might only send request line
				var request = new HttpRequestMessage(requestLine.Method, requestLine.URI);
				if (requestString.Length == 0)
				{
					return request;
				}

				int headersEnd = requestString.IndexOf(CRLF + CRLF);
				var headersString = requestString.Substring(0, headersEnd);

				foreach (var headerLine in headersString.Split(CRLF, StringSplitOptions.RemoveEmptyEntries))
				{
					var headerLineParts = headerLine.Split(":", StringSplitOptions.RemoveEmptyEntries);
					request.Headers.Add(headerLineParts[0].Trim(), headerLineParts[1].Trim());
				}

				requestString = requestString.Remove(0, headersEnd + 4);
				// no need to send body
				if (requestString.Length == 0)
				{
					return request;
				}

				request.Content = new StringContent(requestString);
				return request;
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(HttpRequestMessage)}", ex);
			}
		}

		public static async Task<string> ToHttpStringAsync(this HttpRequestMessage me)
		{
			var requestLine = new RequestLine(me.Method, me.RequestUri, new HttpProtocol($"HTTP/{me.Version.Major}.{me.Version.Minor}")).ToString();
			var headers = me.Headers.ToString();

			var hasHeaders = headers != "";
			var hasContent = me.Content != null;

			string content;
			if (hasContent)
				content = await me.Content.ReadAsStringAsync().ConfigureAwait(false);
			else content = "";

			var ret = requestLine;
			if (!hasHeaders && !hasContent)
			{

			}
			else if (hasHeaders)
				ret += headers + CRLF + content;
			else ret += CRLF + CRLF + content;

			return ret;
		}
	}
}
