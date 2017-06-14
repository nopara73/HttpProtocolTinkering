using System;
using static HttpProtocolTinkering.Common.Constants;
using System.Net;

namespace HttpProtocolTinkering.Common
{
	public class StatusLine
	{
		public HttpProtocol Protocol { get; set; }
		public HttpStatusCode StatusCode { get; set; }

		public StatusLine(HttpProtocol protocol, HttpStatusCode status)
		{
			Protocol = protocol;
			StatusCode = status;
		}

		public override string ToString()
		{
			return Protocol.ToString() + SP + (int)StatusCode + SP + StatusCode.ToReasonString() + CRLF;
		}

		public static StatusLine FromString(string statusLineString)
		{
			try
			{
				var endOfProtocol = statusLineString.IndexOf(SP);
				var protocolString = statusLineString.Substring(0, endOfProtocol);
				var protocol = new HttpProtocol(protocolString);
				statusLineString = statusLineString.Remove(0, endOfProtocol + 1);
				
				var endOfCode = statusLineString.IndexOf(SP);
				var code = int.Parse(statusLineString.Substring(0, endOfCode));
				new HttpStatusCode().Validate(code);
				statusLineString = statusLineString.Remove(0, endOfCode + 1);
				
				var reason = statusLineString;

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
