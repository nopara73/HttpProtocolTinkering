using System;
using static HttpProtocolTinkering.Common.Constants;
using System.Net;
using System.Collections.Generic;

namespace HttpProtocolTinkering.Common
{
	public class StatusLine : StartLine
	{
		public HttpStatusCode StatusCode { get; private set; }

		public StatusLine(HttpProtocol protocol, HttpStatusCode status)
		{
			Protocol = protocol;
			StatusCode = status;

			StartLineString = Protocol.ToString() + SP + (int)StatusCode + SP + StatusCode.ToReasonString() + CRLF;
		}

		public static StatusLine FromString(string statusLineString)
		{
			try
			{
				var parts = GetParts(statusLineString);
				var protocolString = parts[0];
				var codeString = parts[1];
				var reason = parts[2];
				var protocol = new HttpProtocol(protocolString);
				var code = int.Parse(codeString);
				new HttpStatusCode().Validate(code);

				HttpStatusCode statusCode = new HttpStatusCode().FromReasonString(reason);

				return new StatusLine(protocol, statusCode);
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(StatusLine)}", ex);
			}
		}
	}
}
