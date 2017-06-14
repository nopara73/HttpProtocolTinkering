using System;
using System.Net.Http;

namespace HttpProtocolTinkering.Common
{
	public static class Parsers
    {
		public static HttpMethod ToHttpMethod(string methodString)
		{
			// not sure if case sensitive or not, let's do not case sensitive and if I encounter the opposite some in the specificaiton I'll modify it

			var method = HttpMethod.Delete;
			const StringComparison stringComparision = StringComparison.OrdinalIgnoreCase;
			if (method.Method.Equals(methodString, stringComparision))
			{
				return method;
			}
			method = HttpMethod.Get;
			if (method.Method.Equals(methodString, stringComparision))
			{
				return method;
			}
			method = HttpMethod.Head;
			if (method.Method.Equals(methodString, stringComparision))
			{
				return method;
			}
			method = HttpMethod.Options;
			if (method.Method.Equals(methodString, stringComparision))
			{
				return method;
			}
			method = HttpMethod.Post;
			if (method.Method.Equals(methodString, stringComparision))
			{
				return method;
			}
			method = HttpMethod.Put;
			if (method.Method.Equals(methodString, stringComparision))
			{
				return method;
			}
			method = HttpMethod.Trace;
			if (method.Method.Equals(methodString, stringComparision))
			{
				return method;
			}

			throw new NotSupportedException($"{nameof(HttpMethod)}: {methodString} is not supported");
		}
	}
}
