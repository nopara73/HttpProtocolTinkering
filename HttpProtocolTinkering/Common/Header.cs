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

		public static Header FromString(string headerString)
		{
			try
			{
				var lines = new List<string>(headerString.Split(CRLF.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)); // if user included the empty line that ends the header we remove it
				// todo: do header validation here				
				return new Header();
			}
			catch (Exception ex)
			{
				throw new NotSupportedException($"Invalid {nameof(Header)}", ex);
			}
		}
	}
}
