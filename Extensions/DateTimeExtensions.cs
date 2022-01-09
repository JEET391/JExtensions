using JExtensions.Enum;
using System;
using System.Globalization;

namespace JExtensions.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Default DateInputFormat DDMMYY and OutputFormat YYMMDD
        /// </summary>
        /// <param name="dateStirng"></param>
        /// <param name="inputFormat"></param>
        /// <param name="outputFormat"></param>
        /// <param name="outputDateSeperator"></param>
        /// <returns></returns>
        public static string GetDate(this string dateStirng, DateFormat inputFormat, DateFormat outputFormat, char outputDateSeperator = '-', string inputFormatPattern = "")
        {
            if (!string.IsNullOrEmpty(dateStirng))
            {
                try
                {
                    string day = string.Empty;
                    string month = string.Empty;
                    string year = string.Empty;
                    var parts = dateStirng.Split(new char[] { '/', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (inputFormat)
                    {
                        case DateFormat.DDMMYY:
                            day = parts[0];
                            month = parts[1];
                            year = parts[2];
                            break;

                        case DateFormat.MMDDYY:
                            month = parts[0];
                            day = parts[1];
                            year = parts[2];
                            break;

                        case DateFormat.YYMMDD:
                            year = parts[0];
                            month = parts[1];
                            day = parts[2];
                            break;

                        case DateFormat.PATTERN:
                            DateTime.TryParseExact(dateStirng, inputFormatPattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);
                            month = date.Month.ToString();
                            day = date.Day.ToString();
                            year = date.Year.ToString();
                            break;
                    }
                    switch (outputFormat)
                    {
                        case DateFormat.DDMMYY:
                            return $"{day}{outputDateSeperator}{month}{outputDateSeperator}{year}";

                        case DateFormat.MMDDYY:
                            return $"{month}{outputDateSeperator}{day}{outputDateSeperator}{year}";

                        case DateFormat.YYMMDD:
                            return $"{year}{outputDateSeperator}{month}{outputDateSeperator}{day}";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }
            }
            return string.Empty;
        }

        public static DateTime GetDateTime(this string dateString, DateFormat input)
        {
            return Convert.ToDateTime(GetDate(dateString, input, DateFormat.MMDDYY));
        }

        public static bool IsBetween(DateTime date, DateTime fromDate, DateTime toDate = default) => date >= fromDate && date <= toDate;

        public static bool IsGreaterThan(DateTime date, DateTime fromDate) => date <= fromDate;

        public static bool IsLessFromToday(DateTime date) => date < DateTime.Now;

        public static bool IsLessThan(DateTime date, DateTime fromDate) => date >= fromDate;
    }
}