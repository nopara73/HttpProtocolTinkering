using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using static HttpProtocolTinkering.Common.Constants;
using System.IO;

namespace HttpProtocolTinkering.Common
{
    public class HeaderSection
    {
		public List<HeaderField> Fields { get; private set; } = new List<HeaderField>();

		public string ToString(bool endWithCRLF)
		{
			StringBuilder sb = new StringBuilder();
			foreach(var field in Fields)
			{
				sb.Append(field.ToString(true));
			}
			if(endWithCRLF)
			{
				sb.Append(CRLF);
			}
			return sb.ToString();
		}
		public override string ToString()
		{
			return ToString(true);
		}
		public static HeaderSection FromString(string headersString)
		{
			var hs = new HeaderSection();
			if (headersString.EndsWith(CRLF + CRLF))
			{
				headersString = headersString.TrimEnd(CRLF, StringComparison.Ordinal);
			}

			using (var reader = new StringReader(headersString))
			{
				var field = "";
				while (field != null)
				{
					field = reader.ReadLine(strictCRLF: true);
					hs.Fields.Add(HeaderField.FromString(field));
				}
				return hs;
			}
		}

		public HttpRequestHeaders ToHttpRequestHeaders()
		{
			var message = new HttpRequestMessage();
			foreach(var field in Fields)
			{
				message.Headers.Add(field.Name, field.Value);
			}
			return message.Headers;
		}
		public HttpResponseHeaders ToHttpResponseHeaders()
		{
			var message = new HttpResponseMessage();
			foreach (var field in Fields)
			{
				message.Headers.Add(field.Name, field.Value);
			}
			return message.Headers;
		}
		public HttpContentHeaders ToHttpContentHeaders()
		{
			var message = new HttpRequestMessage();
			foreach (var field in Fields)
			{
				message.Content.Headers.Add(field.Name, field.Value);
			}
			return message.Content.Headers;
		}

		public static HeaderSection FromHttpHeaders(HttpHeaders headers)
		{
			var hs = new HeaderSection();
			var message = new HttpRequestMessage();
			foreach (var header in headers)
			{
				hs.Fields.Add(new HeaderField(header.Key, header.Value.ToString()));
			}
			return hs;
		}
	}
}
