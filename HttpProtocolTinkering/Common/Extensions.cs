using HttpProtocolTinkering.Common.Response;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static HttpProtocolTinkering.Common.Constants;

namespace HttpProtocolTinkering.Common
{
	public static class Extensions
	{
		public static string ToReasonString(this HttpStatusCode me)
		{

			var message = new HttpResponseMessage(me);
			return message.ReasonPhrase;
		}

		public static HttpStatusCode FromReasonString(this HttpStatusCode me, string reason)
		{
			foreach (HttpStatusCode code in Enum.GetValues(typeof(HttpStatusCode)))
			{
				// not sure if case sensitive or not, let's do not case sensitive and if I encounter the opposite some in the specificaiton I'll modify it
				if (code != HttpStatusCode.Unused && code.ToReasonString().Equals(reason, StringComparison.OrdinalIgnoreCase))
				{
					return code;
				}
			}

			throw new NotSupportedException($"{nameof(HttpStatusCode)} reason: {reason} is not supported");
		}

		public static void Validate(this HttpStatusCode me, int codeToValidate)
		{
			var valid = false;
			foreach (var code in Enum.GetValues(typeof(HttpStatusCode)))
			{
				if ((int)code == codeToValidate)
				{
					valid = true;
				}
			}

			if (!valid) throw new NotSupportedException($"{nameof(HttpStatusCode)}: {codeToValidate} is not supported");
		}
		public static string[] Split(this string me, string separator, StringSplitOptions options = StringSplitOptions.None)
		{
			return me.Split(separator.ToCharArray(), options);
		}

		public static HttpResponseMessage FromString(this HttpResponseMessage me, string responseString)
		{
			try
			{
				int statusLineEnd = responseString.IndexOf(CRLF);				
				StatusLine statusLine = StatusLine.FromString(responseString.Substring(0,statusLineEnd));
				responseString = responseString.Remove(0, statusLineEnd + 2);

				// server might only send status line
				var response = new HttpResponseMessage(statusLine.StatusCode);
				if (responseString.Length == 0)
				{
					return response;
				}

				int headersEnd = responseString.IndexOf(CRLF + CRLF);
				var headersString = responseString.Substring(0, headersEnd);

				foreach(var headerLine in headersString.Split(CRLF, StringSplitOptions.RemoveEmptyEntries))
				{
					var headerLineParts = headerLine.Split(":", StringSplitOptions.RemoveEmptyEntries);
					response.Headers.Add(headerLineParts[0].Trim(), headerLineParts[1].Trim());
				}

				responseString = responseString.Remove(0, headersEnd + 4);
				// no need to send body
				if (responseString.Length == 0)
				{
					return response;
				}
				
				response.Content = new StringContent(responseString);
				return response;
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(HttpResponseMessage)}", ex);
			}
		}

		public static async Task<string> ToHttpStringAsync(this HttpResponseMessage me)
		{
			var statusLine = new StatusLine(new HttpProtocol($"HTTP/{me.Version.Major}.{me.Version.Minor}"), me.StatusCode).ToString();
			var headers = me.Headers.ToString();

			var hasHeaders = headers != "";
			var hasContent = me.Content != null;

			string content;
			if (hasContent)
				content = await me.Content.ReadAsStringAsync().ConfigureAwait(false);
			else content = "";

			var ret = statusLine;
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
