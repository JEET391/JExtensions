using JExtensions.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JExtensions.Extensions
{
    public static class StringExtensions
    {
        public static string AppendDateTimeStamp(this string value, Formats formats)
        {
            var stamp = DateTime.Now.ToString(formats);
            return value != null ? $"{value}_{stamp}" : stamp;
        }

        public static string AddQuote(this string value, string left, string right)
        {
            return $"{left}{value}{right}";
        }

        public static string AddQuote(this string value, Quote quote)
        {
            switch (quote)
            {
                case Quote.CurlyBraket:
                    return "{" + value + "}";

                case Quote.DoubleQuote:
                    return "\"" + value + "\"";

                case Quote.SingleQuote:
                    return "'" + value + "'";

                case Quote.SmallBraket:
                    return "(" + value + ")";

                case Quote.SquareBraket:
                    return "[" + value + "]";

                default:
                    return value;
            }
        }

        public static string AddQuote(this string value, Quote quote, string splitCharacters)
        {
            string[] parts = value.Split(splitCharacters);
            return string.Join(",", parts.Select(x => x.AddQuote(quote)).ToArray());
        }

        public static string AddQuote(this string value, Quote quote, string splitCharacters, char joinWith)
        {
            string[] parts = value.Split(splitCharacters);
            return string.Join(joinWith.ToString(), parts.Select(x => x.AddQuote(quote)).ToArray());
        }

        public static bool Between<T>(this T value, T x, T y)
        {
            return Comparer<T>.Default.Compare(value, x) >= 0 && Comparer<T>.Default.Compare(value, y) <= 0;
        }

        public static IEnumerable<TSource> Between<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult lowest, TResult highest) where TResult : IComparable<TResult>
        {
            return source.OrderBy(selector)
                .SkipWhile(s => selector.Invoke(s).CompareTo(lowest) < 0)
                .TakeWhile(s => selector.Invoke(s).CompareTo(highest) <= 0);
        }

        public static string CharToRemove(this string value)
        {
            return Regex.Replace(value.ToString(), @"\(|\)|\,", string.Empty).Trim();
        }

        public static string CharToRemove(this string value, char c)
        {
            return Regex.Replace(value.ToString(), "@" + c, string.Empty).Trim();
        }

        public static string CharToRemove(this string value, string chars)
        {
            return Regex.Replace(value.ToString(), "@" + chars, string.Empty).Trim();
        }

        public static double Compare(this string x, string y)
        {
            int count = x.Length > y.Length ? x.Length : y.Length;
            int hits = 0;
            int j = 0;
            int i;
            for (i = 0; i <= x.Length - 1; i++)
            {
                if (x[i] == ' ') { i += 1; j = y.IndexOf(' ', j) + 1; hits += 1; }
                while (j < y.Length && y[j] != ' ')
                {
                    if (x[i] == y[j])
                    {
                        hits += 1;
                        j += 1;
                        break;
                    }
                    else
                    {
                        j += 1;
                    }
                }
                if (!(j < y.Length && y[j] != ' '))
                {
                    j -= 1;
                }
            }
            return Math.Round((double)(hits / count), 2);
        }

        public static bool Contains(this string text, string value, StringComparison comp)
        {
            return text != null && value != null && text.IndexOf(value, comp) >= 0;
        }

        public static object DbNullIfNullOrEmpty<T>(this T str)
        {
            return !string.IsNullOrEmpty(ToString(str)) ? str : (object)DBNull.Value;
        }

        public static IEnumerable<T> EvenIndexItems<T>(this IEnumerable<T> list)
        {
            return list.Where((x, i) => i % 2 == 0);
        }

        public static string GetLastString(this string strLine, char c)
        {
            int start = strLine.LastIndexOf(c);
            int end = strLine.Length;
            return strLine.Substring(start, end - start);
        }

        public static string GetSplitLastString(this string strLine)
        {
            char[] delimiters = new char[] { '/', '\\' };
            string[] parts = strLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            int lastIndex = parts.Length - 1;
            return parts[lastIndex];
        }

        public static bool GreaterThen<T>(this T value, T x)
        {
            return Comparer<T>.Default.Compare(value, x) > 0;
        }

        public static bool In<T>(this T source, params T[] list)
        {
            return list.Contains(source);
        }

        public static bool In<T>(this T source, IEnumerable<T> list)
        {
            return list.Contains(source);
        }

        public static bool NotIn<T>(this T source, params T[] list)
        {
            return list.Contains(source) == false;
        }

        public static bool NotIn<T>(this T source, IEnumerable<T> list)
        {
            return list.Contains(source) == false;
        }

        public static bool IsAlphaNumeric(this string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,]{6,}$");
            return rg.IsMatch(strToCheck);
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

        public static bool IsValidEmail(this string emailAddress)
        {
            const string pattern = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

            return Regex.IsMatch(emailAddress, pattern);
        }

        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        public static bool LessThen<T>(this T value, T x)
        {
            return Comparer<T>.Default.Compare(value, x) < 0;
        }

        public static IEnumerable<string> OddIndexItems(this IEnumerable<string> list)
        {
            return list.Where((x, i) => i % 2 != 0);
        }

        public static string RemoveAndReplace(this string strLine, int pos1, int pos2)
        {
            int start = IndexOfNth(strLine, '\"', pos1);
            int end = IndexOfNth(strLine, '\"', pos2);
            string target = strLine.Substring(start, end - start);
            string desination = Regex.Replace(strLine.Substring(start, end - start), @",", string.Empty);
            strLine = strLine.Replace(target, desination);
            return strLine;
        }

        public static string RemoveInvalidChar(this string value)
        {
            return Regex.Replace(value.ToString(), @"\(|\)|\,", string.Empty).Trim();
        }

        public static string RemoveLastChar(this string value)
        {
            return value.Substring(0, value.Length - 1);
        }

        public static string RemoveLastChar(this string value, int numberOfCharactorsToRemove)
        {
            return value.Substring(0, value.Length - numberOfCharactorsToRemove);
        }

        public static string Right(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Length > maxLength ? value.Substring(value.Length - maxLength, maxLength) : value;
        }

        public static IEnumerable<string> SortByLength(this IEnumerable<string> e)
        {
            // Use LINQ to sort the array received and return a copy.
            var sorted = from s in e
                         orderby s.Length ascending
                         select s;
            return sorted;
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

        public static string SplitLastIndexOf(this string strLine, char c)
        {
            int start = strLine.LastIndexOf(c);
            int end = strLine.Length;
            return strLine.Substring(start, end - start);
        }

        public static string SplitLastOrDefault(this string strLine, string chars)
        {
            return strLine.Split(chars).LastOrDefault();
        }

        public static string Strip<T>(this T value)
        {
            var regexSearch = $"{new string(Path.GetInvalidFileNameChars())}{new string(Path.GetInvalidPathChars())}";
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(ToString(value), "");
        }

        public static decimal ToDecimal(this object value)
        {
            return value == DBNull.Value
                || value == null
                || value.ToString() == ""
                || value.ToString() == "null"
                ? 0
                : Convert.ToDecimal(CharToRemove(value.ToString()));
        }

        public static float ToFloat(this object value)
        {
            return value == DBNull.Value
                || value == null
                || value.ToString() == ""
                || value.ToString() == "null"
                ? 0
                : float.Parse(CharToRemove(value.ToString()), CultureInfo.InvariantCulture.NumberFormat);
        }

        public static HumanName ToHumanName(this string humanName)
        {
            return new HumanName(humanName);
        }

        public static string ToMemorySize(this long byteLength)
        {
            string[] suf = { "Byte", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteLength == 0)
            {
                return "0" + suf[0];
            }
            long bytes = Math.Abs(byteLength);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteLength) * num).ToString() + suf[place];
        }

        public static string ToString(this object value)
        {
            switch (value)
            {
                case null:
                    return string.Empty;

                case DateTime _:
                    return DateTime.Now.ToString();

                default:
                    if (value == DBNull.Value)
                    {
                        return string.Empty;
                    }
                    else if (value is string)
                    {
                        string v = value.ToString();
                        if (v == "" || v == "null" || v == "undefined" || v == " ")
                        {
                            return string.Empty;
                        }
                    }

                    break;
            }
            return value.ToString();
        }

        public static string ToTitleCase(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }
    }
}