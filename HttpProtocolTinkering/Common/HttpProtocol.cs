using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProtocolTinkering.Common
{
	public class HttpProtocol : IEquatable<HttpProtocol>, IEquatable<string>
	{
		public string Protocol { get; }
		public Version Version { get; }
		public static HttpProtocol HTTP11 = new HttpProtocol("HTTP/1.1");

		public HttpProtocol(string protocolString)
		{
			try
			{
				var parts = protocolString.Trim().Split(new char[] { '/' });
				if(parts.Length != 2) throw new FormatException($"Wrong {nameof(HttpProtocol)} format: {protocolString}");

				if(parts[1].Split(new char[] { '.' }).Length != 2)
				{
					throw new FormatException($"Wrong {nameof(HttpProtocol)} format: {protocolString}");
				}

				Version = new Version(parts[1]);

				string protocol = GetProtocol(protocolString);
				if (protocol != "HTTP")
				{
					throw new NotSupportedException($"Wrong protocol {nameof(HttpProtocol)}: {protocolString}");
				}
				Protocol = protocol;
			}
			catch(Exception ex)
			{
				throw new FormatException($"Wrong {nameof(HttpProtocol)} format: {protocolString}", ex);
			}			
		}

		private static string GetProtocol(string protocolString)
		{
			return protocolString.Trim().Split(new char[] { '/' })[0];
		}

		public override string ToString() => $"{Protocol}/{Version.ToString()}";

		#region EqualityAndComparison

		public override bool Equals(object obj) => obj is HttpProtocol && this == (HttpProtocol)obj;
		public bool Equals(HttpProtocol other) => this == other;
		public override int GetHashCode() => ToString().GetHashCode();
		public static bool operator ==(HttpProtocol x, HttpProtocol y) => x.ToString() == y.ToString();
		public static bool operator !=(HttpProtocol x, HttpProtocol y) => !(x == y);

		public bool Equals(string other) => ToString() == other;
		public static bool operator ==(string x, HttpProtocol y) => x == y.ToString();
		public static bool operator ==(HttpProtocol x, string y) => x.ToString() == y;
		public static bool operator !=(string x, HttpProtocol y) => !(x == y);
		public static bool operator !=(HttpProtocol x, string y) => !(x == y);

		#endregion
	}
}
