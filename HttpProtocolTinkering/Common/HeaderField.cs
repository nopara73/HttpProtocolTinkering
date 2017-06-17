using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static HttpProtocolTinkering.Common.Constants;

namespace HttpProtocolTinkering.Common
{
    public class HeaderField
    {
		public string Name { get; private set; }
		public string Value { get; private set; }

		public HeaderField(string name, string value)
		{
			Name = name;
			Value = value;
		}

		// https://tools.ietf.org/html/rfc7230#section-3.2
		// Each header field consists of a case-insensitive field name followed
		// by a colon(":"), optional leading whitespace, the field value, and
		// optional trailing whitespace.
		// header-field   = field-name ":" OWS field-value OWS
		public string ToString(bool endWithCRLF)
		{
			var ret = Name + ":" + Value;
			if (endWithCRLF)
			{
				ret += CRLF;
			}
			return ret;
		}

		public override string ToString()
		{
			return ToString(true);
		}

		public static HeaderField FromString(string fieldString)
		{
			fieldString = fieldString.TrimEnd(CRLF, StringComparison.Ordinal);

			using(var reader = new StringReader(fieldString))
			{
				var name = reader.ReadPart(':');
				if(name == null || name.Trim() == "") throw new FormatException($"Wrong {nameof(HeaderField)}: {fieldString}");

				var value = reader.ReadToEnd();

				// if there's no value
				if (value == null) value = "";
				// if it starts with more than one whitespace
				if (value.Length > 1 && Char.IsWhiteSpace(value[0]) && Char.IsWhiteSpace(value[1])) throw new FormatException($"Wrong {nameof(HeaderField)}: {fieldString}");
				// if it ends with more than one whitespace
				if (value.Length > 1 && Char.IsWhiteSpace(value[value.Length - 1]) && Char.IsWhiteSpace(value[value.Length - 2])) throw new FormatException($"Wrong {nameof(HeaderField)}: {fieldString}");

				return new HeaderField(name, value);
			}
		}
	}
}
