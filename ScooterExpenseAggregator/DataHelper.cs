using System;

namespace Realmar.ScooterExpenseAggregator
{
    internal static class DataHelper
    {
        internal static (double Value, int EndIndex) Extract(
            string mail,
            string startIdentifier,
            string endIdentifier = " ",
            int begin = 0)
        {
            var indexOf = mail.IndexOf(startIdentifier, begin, StringComparison.Ordinal);
            if (indexOf < 0)
            {
                return (default, -1);
            }

            var start = indexOf + startIdentifier.Length;
            var end = mail.IndexOf(endIdentifier, start, StringComparison.Ordinal);
            var raw = mail.Substring(start, end - start);

            if (double.TryParse(raw, out var value))
            {
                end = begin;
            }

            return (value, end);
        }

        internal static double ParseOrDefault(string number)
        {
            if (double.TryParse(number, out var result))
            {
                return result;
            }
            else
            {
                return default;
            }
        }
    }
}