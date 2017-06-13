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

		public static RequestLine FromString(string requestLineString)
		{
			try
			{
				requestLineString = requestLineString.TrimEnd(CRLF.ToCharArray()); // if there's CRLF at the end remove it

				var parts = new List<string>(requestLineString.Split(SP.ToCharArray(), StringSplitOptions.None));
				var typeString = parts[0];
				var uri = parts[1];
				var protocolVersionString = parts[2];
				
				var type = new RequestType().FromString(typeString);
				var protocolVersion = new ProtocolVersion().FromString(protocolVersionString);

				return new RequestLine(type, uri, protocolVersion);
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(RequestLine)}", ex);
			}
		}
	}
}
