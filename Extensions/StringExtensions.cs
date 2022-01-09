using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace JExtensions.Extensions
{
    public enum Month
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }

    public static class StringExtensions
    {
        public static int GetMonthNumber(this string monthName)
        {
            return Convert.ToDateTime("01-" + monthName + "-2011").Month;
        }

        public static int GetMonthNumber(this Month month)
        {
            return (int)month;
        }

        public static int IndexOfNth(this string str, char c, int n)
        {
            int s = -1;

            for (int i = 0; i < n; i++)
            {
                s = str.IndexOf(c, s + 1);

                if (s == -1) break;
            }
            return s;
        }

        public static bool IsNullOrDefault<T>(T value)
        {
            return Equals(value, default(T));
        }

        public static string GetMonthName(this int monthNumber) =>
           new DateTime(1990, monthNumber, 1).ToString("MMMM");

        public static string RemoveAndReplace(this string strLine, int pos1, int pos2)
        {
            int start = IndexOfNth(strLine, '\"', pos1);
            int end = IndexOfNth(strLine, '\"', pos2);
            string target = strLine.Substring(start, end - start);
            string desination = Regex.Replace(strLine.Substring(start, end - start), @",", string.Empty);
            strLine = strLine.Replace(target, desination);
            return strLine;
        }

        public static string ByteSize(this long byteLength)
        {
            string[] suf = { "Byte", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteLength == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteLength);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteLength) * num).ToString() + suf[place];
        }

        public static decimal ToDecimal(this object value)
        {
            switch (value)
            {
                case "":
                case " ":
                case "undefined":
                case "UNDEFINED":
                case "null":
                case "NULL":
                case null:
                    return default;

                default:
                    if (value == DBNull.Value)
                    {
                        return default;
                    }
                    else if (value is int || value is double || value is float)
                    {
                        return (decimal)value;
                    }
                    else if (value is string)
                    {
                        var output = RemoveInvalidChar(value.ToString());
                        decimal.TryParse(output, out decimal result);
                        return result;
                    }
                    else
                    {
                        _ = 0;
                    }
                    break;
            }
            return default;
        }

        public static string GetLastString(this string strLine, char c)
        {
            int start = strLine.LastIndexOf(c);
            int end = strLine.Length;
            return strLine.Substring(start, end - start);
        }

        public static string AppendDateTimeStamp<T>(this string value)
        {
            return value == ""
                ? string.Format("{0}{1:_yyyy_MM_dd_HH_mm_ss_fff_tt}", "", DateTime.Now)
                : string.Format("{0}{1:_yyyy_MM_dd_HH_mm_ss_fff_tt}", value, DateTime.Now);
        }

        public static string GetSplitLastString(this string strLine)
        {
            char[] delimiters = new char[] { '/', '\\' };
            string[] parts = strLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            int lastIndex = parts.Length - 1;
            return parts[lastIndex];
        }

        public static string RemoveInvalidChar(this string value)
        {
            return Regex.Replace(value.ToString(), @"\(|\)|\,", string.Empty).Trim();
        }

        public static object DbNullIfNullOrEmpty<T>(this T str)
        {
            return !string.IsNullOrEmpty(ToString(str)) ? str : (object)DBNull.Value;
        }

        public static int IndexOf(this string value, char c, int nth)
        {
            int index = -1;
            for (int i = 0; i < nth; i++)
            {
                index = value.IndexOf(c, index + 1);
                if (index == -1) break;
            }
            return index;
        }

        public static string Left(this string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Length > maxLength ? value.Substring(0, maxLength) : value;
            }
            return value;
        }

        public static string Right(this string value, int maxLength)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.Length > maxLength ? value.Substring(value.Length - maxLength, maxLength) : value;
            }
            return value;
        }

        public static string Split(this string value, int index)
        {
            return value.Split(new char[] { '/', '-', ' ', '\\', ',' }, StringSplitOptions.RemoveEmptyEntries)[index];
        }

        public static string Split(this string value, int index, string delimiters)
        {
            return value.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[index];
        }

        public static string[] Split(this string value, string delimiters)
        {
            return value.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ToCapitalise(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string ToString(this object value)
        {
            if (value != null && value != DBNull.Value)
            {
                string x = value.ToString();
                return x == "null"
                          || x == "undefined"
                    ? string.Empty
                    : x;
            }
            return string.Empty;
        }
    }
}