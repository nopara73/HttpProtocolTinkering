namespace System
{
    public static class StringExtensions
    {
		public static string[] Split(this string me, string separator, StringSplitOptions options = StringSplitOptions.None)
		{
			return me.Split(separator.ToCharArray(), options);
		}

		/// <summary>
		/// Removes all leading and trailing occurences of the specified string
		/// </summary>
		public static string Trim(this string me, string trimString, StringComparison comparisonType)
		{
			return me.TrimStart(trimString, comparisonType).TrimEnd(trimString, comparisonType);
		}
		/// <summary>
		/// Removes all leading occurences of the specified string
		/// </summary>
		public static string TrimStart(this string me, string trimString, StringComparison comparisonType)
		{
			if (me.StartsWith(trimString, comparisonType))
			{
				return me.Substring(trimString.Length);
			}
			return me;
		}
		/// <summary>
		/// Removes all trailing occurences of the specified string
		/// </summary>
		public static string TrimEnd(this string me, string trimString, StringComparison comparisonType)
		{
			if (me.EndsWith(trimString, comparisonType))
			{
				return me.Substring(0, me.Length - trimString.Length);
			}
			return me;
		}
	}
}
