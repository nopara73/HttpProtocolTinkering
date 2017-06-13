using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;

namespace HttpProtocolTinkering.Common.Response
{
    public class StatusLine
	{
		public ProtocolVersion ProtocolVersion { get; set; }
		public StatusCode StatusCode { get; set; }

		public StatusLine(ProtocolVersion protocolVersion, StatusCode status)
		{
			ProtocolVersion = ProtocolVersion;
			StatusCode = status;
		}

		public override string ToString()
		{
			return ProtocolVersion.ToCorrectString() + SP + (int)StatusCode + SP + StatusCode.ToReasonString() + CRLF;
		}

		public static StatusLine FromString(string statusLineString)
		{
			try
			{
				statusLineString = statusLineString.TrimEnd(CRLF.ToCharArray()); // if there's CRLF at the end remove it

				var parts = new List<string>(statusLineString.Split(SP.ToCharArray(), StringSplitOptions.None));
				var protocolVersionString = parts[0];
				var protocolVersion = new ProtocolVersion().FromString(protocolVersionString);
				parts.RemoveAt(0);
				var code = int.Parse(parts[0]);
				new StatusCode().Validate(code);

				parts.RemoveAt(0);
				var reason = string.Join(SP, parts);

				StatusCode statusCode = new StatusCode().FromReasonString(reason);

				return new StatusLine(protocolVersion, statusCode);
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(StatusLine)}", ex);
			}
		}
	}
}
