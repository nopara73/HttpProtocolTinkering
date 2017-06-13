using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProtocolTinkering.Common.Request
{
    public enum RequestType
    {
		GET
    }

	public static class RequestTypeExtensions
	{
		public static RequestType FromString(this RequestType me, string requestTypeString)
		{
			foreach (RequestType type in Enum.GetValues(typeof(RequestType)))
			{
				// not sure if case sensitive or not, let's do not case sensitive and if I encounter the opposite some in the specificaiton I'll modify it
				if (type.ToString().Equals(requestTypeString, StringComparison.OrdinalIgnoreCase))
				{
					return type;
				}
			}

			throw new NotSupportedException($"{nameof(RequestType)}: {requestTypeString} is not supported");
		}
	}
}
