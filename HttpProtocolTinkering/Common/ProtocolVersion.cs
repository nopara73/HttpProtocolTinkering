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
	}
}
