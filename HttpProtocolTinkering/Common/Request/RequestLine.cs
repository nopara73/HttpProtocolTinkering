using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;

namespace HttpProtocolTinkering.Common.Request
{
    public class RequestLine
    {
		public RequestType Type { get; set; }
		public string URI { get; set; }
		public ProtocolVersion ProtocolVersion { get; set; }

		public RequestLine(RequestType type, string uri, ProtocolVersion protocolVersion)
		{
			Type = type;
			URI = uri;
			ProtocolVersion = ProtocolVersion;
		}

		public override string ToString()
		{
			return Type + SP + URI + SP + ProtocolVersion.ToCorrectString() + CRLF;
		}
	}
}
