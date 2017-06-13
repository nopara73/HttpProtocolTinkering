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

		public static StatusCode FromReasonString(this StatusCode me, string reason)
		{
			foreach (StatusCode code in Enum.GetValues(typeof(StatusCode)))
			{
				// not sure if case sensitive or not, let's do not case sensitive and if I encounter the opposite some in the specificaiton I'll modify it
				if (code.ToReasonString().Equals(reason, StringComparison.OrdinalIgnoreCase))
				{
					return code;
				}
			}

			throw new NotSupportedException($"{nameof(StatusCode)} reason: {reason} is not supported");
		}

		public static void Validate(this StatusCode me, int codeToValidate)
		{
			var valid = false;
			foreach(var code in Enum.GetValues(typeof(StatusCode)))
			{
				if((int)code == codeToValidate)
				{
					valid = true;
				}
			}

			if(!valid) throw new NotSupportedException($"{nameof(StatusCode)}: {codeToValidate} is not supported");
		}
	}
}
