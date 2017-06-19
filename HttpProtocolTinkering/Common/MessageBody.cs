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
		public HttpHeaderValueCollection<TransferCodingHeaderValue> TransferCodings { get; }
		public bool Present => PayloadBody != null && PayloadBody != "";

		public MessageBody(string payloadBody, HttpRequestHeaders requestHeaders, HttpContentHeaders contentHeaders)
			: this(payloadBody, requestHeaders.TransferEncoding)
		{

		}

		public MessageBody(string payloadBody, HttpResponseHeaders responseHeaders, HttpContentHeaders contentHeaders) 
			: this(payloadBody, responseHeaders.TransferEncoding)
		{

		}

		public MessageBody(string payloadBody, HttpHeaderValueCollection<TransferCodingHeaderValue> transferCodings)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.3
			// The message body is
			// identical to the payload body unless a transfer coding has been applied.
			if (transferCodings.Count == 0)
			{
				PayloadBody = payloadBody;
			}
			else throw new NotImplementedException();
			TransferCodings = transferCodings;
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
