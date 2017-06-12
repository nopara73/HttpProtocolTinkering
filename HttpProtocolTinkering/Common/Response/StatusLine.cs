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
	}
}
