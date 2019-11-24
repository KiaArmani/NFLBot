using System;
using System.Collections.Generic;
using System.Text;

namespace XurBot.Extensions
{
    /// <summary>
    /// Cuts a string for a given max Length
    /// Code from: https://stackoverflow.com/a/2776689
    /// Author: LBushkin - https://stackoverflow.com/users/91671/lbushkin
    /// Code licensed under cc-by-sa as per https://stackoverflow.com/help/licensing
    /// </summary>
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
