using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProtocolTinkering.Common
{
    public enum ProtocolVersion
    {
		HTTP11
    }
	public static class ProtocolVersionExtensions
	{
		public static string ToCorrectString(this ProtocolVersion me)
		{
			switch (me)
			{
				case ProtocolVersion.HTTP11:
					return "HTTP/1.1";
				default:
					throw new NotImplementedException();
			}
		}

		public static ProtocolVersion FromString(this ProtocolVersion me, string protocolVersionString)
		{
			// HTTP is case sensitive
			if(protocolVersionString == ProtocolVersion.HTTP11.ToCorrectString())
			{
				return ProtocolVersion.HTTP11;
			}
			else throw new NotSupportedException($"{nameof(ProtocolVersion)}: {protocolVersionString} is not supported");
		}
	}
}
