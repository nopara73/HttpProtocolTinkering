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