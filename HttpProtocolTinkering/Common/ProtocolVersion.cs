using System;
using System.Collections.Generic;
using System.Text;

namespace HttpProtocolTinkering.Common
{
	public class ProtocolVersion : IEquatable<ProtocolVersion>, IEquatable<string>
	{
		public string Value { get; }
		public int MajorVersion => GetMajor(Value);
		public int MinorVersion => GetMinor(Value);
		public static ProtocolVersion HTTP11 = new ProtocolVersion("HTTP/1.1");

		public ProtocolVersion(string protocolVersionString)
		{
			try
			{
				if (GetProtocol(protocolVersionString) != "HTTP")
				{
					throw new NotSupportedException($"Wrong protocol {nameof(ProtocolVersion)}: {protocolVersionString}");
				}

				// To assert it complies
				GetMajor(protocolVersionString);
				GetMinor(protocolVersionString);

				Value = protocolVersionString;
			}
			catch(Exception ex)
			{
				throw new FormatException($"Wrong {nameof(ProtocolVersion)} format: {protocolVersionString}", ex);
			}			
		}

		private static string GetProtocol(string protocolVersionString)
		{
			return protocolVersionString.Trim().Split(new char[] { '/' })[0];
		}
		private static int GetMajor(string protocolVersionString)
		{
			var versions = protocolVersionString.Trim().Split(new char[] { '/' })[1].Split(new char[] { '.' });
			return int.Parse(versions[0]);
		}
		private static int GetMinor(string protocolVersionString)
		{
			var versions = protocolVersionString.Trim().Split(new char[] { '/' })[1].Split(new char[] { '.' });
			return int.Parse(versions[1]);
		}

		public override string ToString()
		{
			return Value;
		}

		#region EqualityAndComparison

		public override bool Equals(object obj) => obj is ProtocolVersion && this == (ProtocolVersion)obj;
		public bool Equals(ProtocolVersion other) => this == other;
		public override int GetHashCode() => Value.GetHashCode();
		public static bool operator ==(ProtocolVersion x, ProtocolVersion y) => x.Value == y.Value;
		public static bool operator !=(ProtocolVersion x, ProtocolVersion y) => !(x == y);

		public bool Equals(string other) => Value == other;
		public static bool operator ==(string x, ProtocolVersion y) => x == y.Value;
		public static bool operator ==(ProtocolVersion x, string y) => x.Value == y;
		public static bool operator !=(string x, ProtocolVersion y) => !(x == y);
		public static bool operator !=(ProtocolVersion x, string y) => !(x == y);

		#endregion
	}
}
