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

			value = CorrectObsFolding(value);
			Value = value;
		}

		public static string CorrectObsFolding(string text)
		{
			// fix obs line folding
			// https://tools.ietf.org/html/rfc7230#section-3.2.4
			// replace each received obs-fold with one or more SP octets prior to interpreting the field value
			text = text.Replace(CRLF + SP, SP + SP);
			text = text.Replace(CRLF + TAB, SP + TAB);
			return text;
		}

		// https://tools.ietf.org/html/rfc7230#section-3.2
		// Each header field consists of a case-insensitive field name followed
		// by a colon(":"), optional leading whitespace, the field value, and
		// optional trailing whitespace.
		// header-field   = field-name ":" OWS field-value OWS
		// The OWS rule is used where zero or more linear whitespace octets	might appear.
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
				// if empty
				if(name == null || name.Trim() == "") throw new FormatException($"Wrong {nameof(HeaderField)}: {fieldString}");
				// whitespace not allowed
				if (name != name.Trim()) throw new FormatException($"Wrong {nameof(HeaderField)}: {fieldString}");

				var value = reader.ReadToEnd();
				// correction
				if (value == null) value = "";
				value = value.Trim(); // better to use without whitespaces


				return new HeaderField(name, value);
			}
		}
	}
}
