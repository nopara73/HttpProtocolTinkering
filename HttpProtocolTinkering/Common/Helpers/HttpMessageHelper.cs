using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static HttpProtocolTinkering.Common.Constants;

namespace System.Net.Http
{
	public static class HttpMessageHelper
    {
		public static async Task<string> ReadStartLineAsync(StreamReader reader)
		{
			// https://tools.ietf.org/html/rfc7230#section-3
			// A recipient MUST parse an HTTP message as a sequence of octets in an
			// encoding that is a superset of US-ASCII[USASCII].

			// Read until the first CRLF
			// the CRLF is part of the startLine
			var startLine = await reader.ReadLineAsync(strictCRLF: true).ConfigureAwait(false) + CRLF;
			if (startLine == null || startLine == "") throw new FormatException($"{nameof(startLine)} cannot be null or empty");
			return startLine;
		}

		public static async Task<string> ReadHeadersAsync(StreamReader reader)
		{
			var headers = "";
			var firstRead = true;
			while (true)
			{
				var header = await reader.ReadLineAsync(strictCRLF: true).ConfigureAwait(false);
				if (header == null) throw new FormatException($"Malformed HTTP message: End of headers must be CRLF");
				if (header == "")
				{
					// 2 CRLF was read in row so it's the end of the headers
					break;
				}

				if (firstRead)
				{
					// https://tools.ietf.org/html/rfc7230#section-3
					// A recipient that receives whitespace between the
					// start - line and the first header field MUST either reject the message
					// as invalid or consume each whitespace-preceded line without further
					// processing of it(i.e., ignore the entire line, along with any				 
					// subsequent lines preceded by whitespace, until a properly formed				 
					// header field is received or the header section is terminated).
					if (Char.IsWhiteSpace(header[0]))
					{
						throw new FormatException($"Invalid HTTP message: Cannot be whitespace between the start line and the headers");
					}
					firstRead = false;
				}

				headers += header + CRLF; // CRLF is part of the headerstring
			}
			if (headers == null || headers == "")
			{
				headers = "";
			}

			return headers;
		}

		public static void AssertValidResponse(bool hasMessageBody, HttpResponseHeaders responseHeaders, HttpContentHeaders contentHeaders, HttpStatusCode statusCode, HttpMethod requestMethod)
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
			if (hasMessageBody)
			{
				if (HttpStatusCodeHelper.IsInformational(statusCode))
				{
					throw new HttpRequestException("Response with 1xx status code cannot include message body");
				}
				if (statusCode == HttpStatusCode.NoContent)
				{
					throw new HttpRequestException("Response with 204 status code cannot include message body");
				}
				if (statusCode == HttpStatusCode.NotModified)
				{
					throw new HttpRequestException("Response with 304 status code cannot include message body");
				}

				if (requestMethod == HttpMethod.Head)
				{
					throw new HttpRequestException("Response to HEAD method cannot include message body");
				}
				if (requestMethod == new HttpMethod("CONNECT"))
				{
					if (HttpStatusCodeHelper.IsSuccessful(statusCode))
					{
						throw new HttpRequestException("Response to CONNECT method with 2xx status code cannot include message body");
					}
				}
			}

			// https://tools.ietf.org/html/rfc7230#section-3.3.1
			// A server MUST NOT send a Transfer-Encoding header field in any
			// response with a status code of 1xx(Informational) or 204(No Content).
			// https://tools.ietf.org/html/rfc7230#section-3.3.2
			// A server MUST NOT send a Content-Length header field in any
			// response with a status code of 1xx(Informational) or 204(No Content).
			if (responseHeaders.Contains("Transfer-Encoding") || contentHeaders.Contains("Content-Length"))
			{
				if (HttpStatusCodeHelper.IsInformational(statusCode))
				{
					throw new HttpRequestException("A server MUST NOT send a Transfer-Encoding or Content-Length header fields in any response with a status code of 1xx(Informational)");
				}
				if (statusCode == HttpStatusCode.NoContent)
				{
					throw new HttpRequestException("A server MUST NOT send a Transfer-Encoding or Content-Length header fields in any response with a status code of 204(No Content)");
				}
			}

			// https://tools.ietf.org/html/rfc7230#section-3.3.1
			// A server MUST NOT send a Transfer-Encoding header field in any 2xx
			// (Successful) response to a CONNECT request(Section 4.3.6 of
			// [RFC7231]).
			// https://tools.ietf.org/html/rfc7230#section-3.3.2
			// A server MUST NOT send a Content-Length header field in any 2xx
			// (Successful) response to a CONNECT request(Section 4.3.6 of
			// [RFC7231]).
			if (requestMethod == new HttpMethod("CONNECT"))
			{
				if (responseHeaders.Contains("Transfer-Encoding") || contentHeaders.Contains("Content-Length"))
				{
					if (HttpStatusCodeHelper.IsSuccessful(statusCode))
					{
						throw new HttpRequestException("A server MUST NOT send a Transfer-Encoding or Content-Length header fields in any 2xx(Successful) response to a CONNECT request");
					}
				}
			}

			// Any Content-Length field value greater than or equal to zero is valid.
			if (contentHeaders.Contains("Content-Length"))
			{
				if (contentHeaders.ContentLength < 0)
					throw new HttpRequestException("Content-Length MUST be bigger than zero");
			}
		}

		public static HttpContent GetDummyOrNullContent(HttpContentHeaders contentHeaders)
		{
			if (contentHeaders != null && contentHeaders.Count() != 0)
			{
				return new ByteArrayContent(new byte[] { }); // dummy empty content
			}
			else
			{
				return null;
			}
		}

		public static void CopyHeaders(HttpHeaders source, HttpHeaders destination)
		{
			if (source != null && source.Count() != 0)
			{
				foreach (var header in source)
				{
					destination.TryAddWithoutValidation(header.Key, header.Value);
				}
			}
		}
	}
}
