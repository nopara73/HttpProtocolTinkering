﻿using HttpProtocolTinkering.Common;
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

			HttpMessageHelper.AssertValidResponse(headerStruct.RequestHeaders, headerStruct.ContentHeaders);
			request.Content = HttpMessageHelper.GetContent(reader, position, headerStruct);

			if (request.Content != null)
			{
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

			return startLine + headers + CRLF + messageBody;
		}
	}
}
