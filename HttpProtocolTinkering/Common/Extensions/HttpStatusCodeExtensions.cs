using System.Net.Http;

namespace System.Net
{
	public static class HttpStatusCodeExtensions
	{
		public static string ToReasonString(this HttpStatusCode me)
		{
			var message = new HttpResponseMessage(me);
			return message.ReasonPhrase;
		}

		public static HttpStatusCode FromReasonString(this HttpStatusCode me, string reason)
		{
			foreach (HttpStatusCode code in Enum.GetValues(typeof(HttpStatusCode)))
			{				
				// not sure if case sensitive or not, let's do not case sensitive and if I encounter the opposite some in the specificaiton I'll modify it
				string r = code.ToReasonString();
				if (code != HttpStatusCode.Unused && r.Equals(reason, StringComparison.OrdinalIgnoreCase))
				{
					return code;
				}
			}

			throw new NotSupportedException($"{nameof(HttpStatusCode)} reason: {reason} is not supported");
		}

		public static void Validate(this HttpStatusCode me, int codeToValidate)
		{
			var valid = false;
			foreach (var code in Enum.GetValues(typeof(HttpStatusCode)))
			{
				if ((int)code == codeToValidate)
				{
					valid = true;
				}
			}

			if (!valid) throw new NotSupportedException($"{nameof(HttpStatusCode)}: {codeToValidate} is not supported");
		}
	}
}