using HttpProtocolTinkering.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProtocolTinkering
{
    public abstract class StartLine
	{
		public HttpProtocol Protocol { get; set; }
		public abstract override string ToString();
	}
}
