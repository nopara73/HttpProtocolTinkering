using System;
using System.Collections.Generic;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;

namespace HttpProtocolTinkering.Common
{
    public class Header
    {
		public override string ToString()
		{
			return CRLF + CRLF;
		}
	}
}
