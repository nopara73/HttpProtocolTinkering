using System;

namespace HttpProtocolTinkering.Common
{
	public class HttpRequestUriBuilder:UriBuilder
    {
		public HttpRequestUriBuilder(UriScheme uriScheme, string hostName)
			:base(uriScheme.ToString(), hostName)
		{
			// https://tools.ietf.org/html/rfc7230#section-2.7.1
			// A sender MUST NOT generate an "http" URI with an empty host identifier.
			if (hostName == "") throw new FormatException("Host identifier is empty");

			if (uriScheme == UriScheme.http)
			{
				// https://tools.ietf.org/html/rfc7230#section-2.7.1
				// [http] If the port subcomponent is empty or not given, TCP port 80(the reserved port
				// for WWW services) is the default.
				Port = 80;
			}
			else if(uriScheme == UriScheme.https)
			{
				// https://tools.ietf.org/html/rfc7230#section-2.7.2
				// [https] TCP port 443 is the default if the port subcomponent is empty or not given
				Port = 443;
			}
		}
    }
}
