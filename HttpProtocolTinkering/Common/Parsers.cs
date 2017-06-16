using System;
using System.Net.Http;

namespace HttpProtocolTinkering.Common
{
	public static class Parsers
    {
		public static HttpMethod ToHttpMethod(string methodString)
		{
			// https://tools.ietf.org/html/rfc7230#section-3.1.1
			// The request method is case-sensitive.
			var method = HttpMethod.Delete;
			if (method.Method == methodString)
			{
				return method;
			}
			method = HttpMethod.Get;
			if (method.Method == methodString)
			{
				return method;
			}
			method = HttpMethod.Head;
			if (method.Method == methodString)
			{
				return method;
			}
			method = HttpMethod.Options;
			if (method.Method == methodString)
			{
				return method;
			}
			method = HttpMethod.Post;
			if (method.Method == methodString)
			{
				return method;
			}
			method = HttpMethod.Put;
			if (method.Method == methodString)
			{
				return method;
			}
			method = HttpMethod.Trace;
			if (method.Method == methodString)
			{
				return method;
			}

			throw new NotSupportedException($"{nameof(HttpMethod)}: {methodString} is not supported");
		}
	}
}
