using System;
using System.Collections.Generic;
using System.Text;
using HttpProtocolTinkering.Common;
using static System.Console;
using HttpProtocolTinkering.Common.Response;
using HttpProtocolTinkering.Common.Request;

namespace HttpProtocolTinkering.Server
{
	public class OriginServer
	{
		public string AcceptRequest(string requestString)
		{
			var request = RequestMessage.FromString(requestString);
			var protocolVersion = ProtocolVersion.HTTP11;

			if (request.RequestLine.ProtocolVersion.MajorVersion != protocolVersion.MajorVersion)
			{
				return new ResponseMessage(new StatusLine(protocolVersion, StatusCode.HTTPVersionNotSupported)).ToString();
			}

			var response = new ResponseMessage(new StatusLine(protocolVersion, StatusCode.BadRequest));
			var http11OkStatusLine = new StatusLine(protocolVersion, StatusCode.OK);

			if (request.RequestLine.Type == RequestType.GET)
			{
				if (request.RequestLine.URI == "foo")
				{
					response = new ResponseMessage(http11OkStatusLine, body: "bar");
				}
			}

			WriteLine();
			WriteLine("Sending response:");
			WriteLine(response);
			return response.ToString();
		}
	}
}
