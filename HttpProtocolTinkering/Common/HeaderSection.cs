using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using static HttpProtocolTinkering.Common.Constants;
using System.IO;
using System.Linq;

namespace HttpProtocolTinkering.Common
{
    public class HeaderSection
    {
		public List<HeaderField> Fields { get; private set; } = new List<HeaderField>();

		public string ToString(bool endWithTwoCRLF)
		{
			StringBuilder sb = new StringBuilder();
			foreach(var field in Fields)
			{
				sb.Append(field.ToString(endWithCRLF: true));
			}
			if(endWithTwoCRLF)
			{
				sb.Append(CRLF);
			}
			return sb.ToString();
		}
		public override string ToString()
		{
			return ToString(false);
		}
		public static HeaderSection FromString(string headersString)
		{
			headersString = HeaderField.CorrectObsFolding(headersString);

			var hs = new HeaderSection();
			if (headersString.EndsWith(CRLF + CRLF))
			{
				headersString = headersString.TrimEnd(CRLF, StringComparison.Ordinal);
			}

			using (var reader = new StringReader(headersString))
			{
				while (true)
				{
					var field = reader.ReadLine(strictCRLF: true);
					if (field == null)
					{
						break;
					}
					else
					{
						hs.Fields.Add(HeaderField.FromString(field));
					}
				}
				return hs;
			}
		}

		public HttpRequestContentHeaders ToHttpRequestHeaders()
		{
			var message = new HttpRequestMessage()
			{
				Content = new ByteArrayContent(new byte[] { })
			};
			foreach (var field in Fields)
			{
				if (field.Name.StartsWith("Content-", StringComparison.Ordinal))
				{
					message.Content.Headers.TryAddWithoutValidation(field.Name, field.Value);
				}
				else message.Headers.TryAddWithoutValidation(field.Name, field.Value);
			}
			return new HttpRequestContentHeaders
			{
				RequestHeaders = message.Headers,
				ContentHeaders = message.Content.Headers
			};
		}
		public HttpResponseContentHeaders ToHttpResponseHeaders()
		{
			var message = new HttpResponseMessage()
			{
				Content = new ByteArrayContent(new byte[] { })
			};
			foreach (var field in Fields)
			{
				if(field.Name.StartsWith("Content-", StringComparison.Ordinal))
				{
					message.Content.Headers.TryAddWithoutValidation(field.Name, field.Value);
				}
				else message.Headers.TryAddWithoutValidation(field.Name, field.Value);
			}
			return new HttpResponseContentHeaders
			{
				ResponseHeaders = message.Headers,
				ContentHeaders = message.Content.Headers
			};
		}

		public static HeaderSection FromHttpHeaders(HttpHeaders headers)
		{
			var hs = new HeaderSection();
			var message = new HttpRequestMessage();
			foreach (var header in headers)
			{
				hs.Fields.Add(new HeaderField(header.Key, String.Join(",", header.Value)));
			}

			// -- Start [SECTION] Crazy VS2017/.NET Core 1.1 bug ---
			// The following if branch is needed as is to avoid the craziest VS2017/.NET Core 1.1 bug I have ever seen!
			// If this section is not added the Content-Length header will not be set unless...
			// - I put a break point at the start of the function
			// - And I explicitly expand the "headers" variable
			if (headers is HttpContentHeaders)
			{
				if (((HttpContentHeaders)headers).ContentLength != null)
				{
					if (hs.Fields.All(x => x.Name != "Content-Length"))
					{
						hs.Fields.Add(new HeaderField("Content-Length", ((HttpContentHeaders)headers).ContentLength.ToString()));
					}
				}
			}
			// -- End [SECTION] Crazy VS2017/.NET Core 1.1 bug ---

			return hs;
		}
	}
}
