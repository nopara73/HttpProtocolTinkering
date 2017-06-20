using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HttpProtocolTinkering.Common
{
    public class MessageBody
    {
		public string PayloadBody { get; }
		public bool Present { get; }

		public MessageBody(string messageBody, HttpRequestHeaders requestHeaders, HttpContentHeaders contentHeaders)
			: this(messageBody, requestHeaders.TransferEncoding)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The presence of a message body in a request is signaled by a
			// Content - Length or Transfer-Encoding header field.  Request message
			// framing is independent of method semantics, even if the method does
			// not define any use for a message body.
			Present = requestHeaders.TransferEncoding.Count != 0 || contentHeaders.ContentLength != null;
			if (!Present && messageBody != null && messageBody != "")
			{
				throw new HttpRequestException("Message body is not indicated in the headers, yet it's present");
			}
		}

		public MessageBody(string messageBody, HttpResponseHeaders responseHeaders, HttpContentHeaders contentHeaders, HttpStatusCode status) 
			: this(messageBody, responseHeaders.TransferEncoding)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The presence of a message body in a response depends on both the
			// request method to which it is responding and the response status code
			// (Section 3.1.2).  Responses to the HEAD request method(Section 4.3.2
			// of[RFC7231]) never include a message body because the associated
			// response header fields(e.g., Transfer - Encoding, Content - Length,
			// etc.), if present, indicate only what their values would have been if
			// the request method had been GET(Section 4.3.1 of[RFC7231]). 2xx
			// (Successful) responses to a CONNECT request method(Section 4.3.6 of
			// [RFC7231]) switch to tunnel mode instead of having a message body.
			// All 1xx(Informational), 204(No Content), and 304(Not Modified)
			// responses do not include a message body.  All other responses do
			// include a message body, although the body might be of zero length.
			if (messageBody != null && messageBody != "")
			{
				Present = true;
				if (((int)status).ToString()[0] == '1')
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
		}

		public MessageBody(string messageBody, HttpHeaderValueCollection<TransferCodingHeaderValue> transferCodings)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The message body is
			// identical to the payload body unless a transfer coding has been applied.
			if (transferCodings.Count == 0)
			{
				PayloadBody = messageBody;
			}
			else throw new NotImplementedException();
		}

		public HttpContent ToHttpContent()
		{
			if (Present)
			{
				return new StringContent(PayloadBody);
			}
			else return null;
		}
	}
}
