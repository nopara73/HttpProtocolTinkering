using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http
{
    public static class HttpResponseMessageHelper
    {
		public static void AssertValidResponse(string messageBody, HttpResponseHeaders responseHeaders, HttpStatusCode status, out bool present)
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
				present = true;
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
			else present = false;
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
	}
}
