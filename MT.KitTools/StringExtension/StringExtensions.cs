using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MT.KitTools.StringExtension
{
    public static class StringExtensions
    {
        public static string If(this string self, Func<bool> condition)
        {
            if (condition.Invoke())
            {
                return self;
            }
            return string.Empty;
        }

        public static bool IsEnable(this string self, Func<string, bool> rule = null)
        {
            if (string.IsNullOrEmpty(self))
            {
                return false;
            }
            var b = rule?.Invoke(self);
            if (b.HasValue)
            {
                return b.Value;
            }
            return true;
        }

        public static bool IsNumeric<T>(this string self, out T value) where T : struct
        {
            var match = Regex.IsMatch(self, @"([1-9]\d*\.?\d*)|(0\.\d*[1-9])");
            if (match)
            {
                value = (T)Convert.ChangeType(self, typeof(T));
            }
            else
                value = default;
            return match;
        }
    }
}
