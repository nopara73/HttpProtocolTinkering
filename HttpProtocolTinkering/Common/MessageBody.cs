﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Linq;

namespace HttpProtocolTinkering.Common
{
    public class MessageBody
    {
		public string PayloadBody { get; }
		public HttpContentHeaders ContentHeaders { get; private set; }
		public bool Present;


		public MessageBody(string messageBody, HttpRequestHeaders requestHeaders, HttpContentHeaders contentHeaders)
			: this(messageBody, requestHeaders.TransferEncoding, contentHeaders)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The presence of a message body in a request is signaled by a
			// Content - Length or Transfer-Encoding header field. 
			// Request message framing is independent of method semantics, even if the method does
			// not define any use for a message body.
			Present = requestHeaders.TransferEncoding.Count != 0 || contentHeaders.ContentLength != null;
			if (!Present && messageBody != null && messageBody != "")
			{
				throw new HttpRequestException("Message body is not indicated in the headers, yet it's present");
			}
		}

		public MessageBody(string messageBody, HttpResponseHeaders responseHeaders, HttpContentHeaders contentHeaders, HttpStatusCode status) 
			: this(messageBody, responseHeaders.TransferEncoding, contentHeaders)
		{
			ValidateResponse(messageBody, responseHeaders, status);
		}

		private void ValidateResponse(string messageBody, HttpResponseHeaders responseHeaders, HttpStatusCode status)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The presence of a message body in a response depends on both the
			// request method to which it is responding and the response status code
			// (Section 3.1.2). 
			// All 1xx(Informational), 204(No Content), and 304(Not Modified)
			// responses do not include a message body.  All other responses do
			// include a message body, although the body might be of zero length.
			if (messageBody != null && messageBody != "")
			{
				Present = true;
				if (HttpStatusCodeHelper.IsInformational(status))
				{
					throw new HttpRequestException("Response with 1xx status code cannot include message body");
				}
				if (status == HttpStatusCode.NoContent)
				{
					throw new HttpRequestException("Response with 204 status code cannot include message body");
				}
				if (status == HttpStatusCode.NotModified)
				{
					throw new HttpRequestException("Response with 304 status code cannot include message body");
				}
			}
			else Present = false;
			// https://tools.ietf.org/html/rfc7230#section-3.3.1
			//  A server MUST NOT send a Transfer - Encoding header field in any
			//  response with a status code of 1xx(Informational) or 204(No Content).
			if (responseHeaders.Contains("Transfer-Encoding"))
			{
				if (HttpStatusCodeHelper.IsInformational(status))
				{
					throw new HttpRequestException("A server MUST NOT send a Transfer - Encoding header field in any response with a status code of 1xx(Informational)");
				}
				if (status == HttpStatusCode.NoContent)
				{
					throw new HttpRequestException("A server MUST NOT send a Transfer - Encoding header field in any response with a status code of 204(No Content)");
				}
			}
		}

		public MessageBody(string messageBody, HttpHeaderValueCollection<TransferCodingHeaderValue> transferCodings, HttpContentHeaders contentHeaders)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The message body is
			// identical to the payload body unless a transfer coding has been applied.
			if (transferCodings.Count == 0)
			{
				PayloadBody = messageBody;
			}
			else
			{
				// https://tools.ietf.org/html/rfc7230#section-3.3.2
				// A sender MUST NOT send a Content - Length header field in any message
				// that contains a Transfer-Encoding header field.
				if(contentHeaders.ContentLength == null)
				{
					throw new FormatException("A sender MUST NOT send a Content - Length header field in any messagethat contains a Transfer-Encoding header field.");
				}
			}

			ContentHeaders = contentHeaders;
		}

		public HttpContent ToHttpContent()
		{
			var content = new ByteArrayContent(new byte[] { }); // dummy empty content
			foreach(var header in ContentHeaders)
			{
				content.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			if (Present)
			{
				return new StringContent(PayloadBody);
			}

			if (content.Headers.Count() == 0)
			{
				return null;
			}
			else return content;
		}
	}
}
