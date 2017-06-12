using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProtocolTinkering.Common.Response
{
    public enum StatusCode
    {
		OK = 200,
		BadRequest = 400,
		HTTPVersionNotSupported = 505
	}
	public static class StatusCodeExtensions
	{
		public static string ToReasonString(this StatusCode me)
		{
			switch (me)
			{
				case StatusCode.OK:
					return "OK";
				case StatusCode.BadRequest:
					return "Bad Request";
				case StatusCode.HTTPVersionNotSupported:
					return "HTTP Version Not Supported";
				default:
					throw new NotImplementedException();
			}
		}
	}
}
