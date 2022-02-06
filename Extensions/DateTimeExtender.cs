using JExtensions.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static JExtensions.Extensions.Enum.DateTimeExtender;

namespace JExtensions.Extensions
{
    public static partial class DateTimeExtender
    {
        public static string ToString(this DateTime value, Formats formats)
        {
            string dateformats;
            switch (formats)
            {
                case Formats.Date:
                    dateformats = DateFormats.Date;
                    break;

                case Formats.DateTime:
                    dateformats = DateFormats.DateTime;
                    break;

                case Formats.DateTime24:
                    dateformats = DateFormats.DateTime24;
                    break;

                case Formats.DateTimeStamp:
                    dateformats = DateFormats.DateTimeStamp;
                    break;

                case Formats.DateTimeStamp24:
                    dateformats = DateFormats.DateTimeStamp24;
                    break;

                case Formats.Month:
                    dateformats = DateFormats.Month;
                    break;

                case Formats.Time:
                    dateformats = DateFormats.Time;
                    break;

                case Formats.Time24:
                    dateformats = DateFormats.Time24;
                    break;

                case Formats.ddmmyyyy:
                    dateformats = DateFormats.ddmmyyyy;
                    break;

                case Formats.mmddyyy:
                    dateformats = DateFormats.mmddyyyy;
                    break;

                case Formats.yyyymmdd:
                    dateformats = DateFormats.yyyymmdd;
                    break;

                default:
                    dateformats = DateFormats.Date;
                    break;
            }
            return value.ToString(dateformats);
        }

        public static string GetMonthName(this DateTime dateTime)
        {
            return dateTime.ToString(DateFormats.Month);
        }

        /// <summary>
        /// Convert DateTime to string
        /// </summary>
        /// <param name="datetTime"></param>
        /// <param name="excludeHoursAndMinutes">if true it will execlude time from datetime string. Default is false</param>
        /// <returns></returns>
        public static string GetDate(this DateTime datetTime, bool excludeHoursAndMinutes = false)
        {
            if (datetTime != DateTime.MinValue)
            {
                if (excludeHoursAndMinutes)
                    return datetTime.ToString("yyyy-MM-dd");
                return datetTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            return null;
        }

        /// <summary>
        /// Default DateInputFormat DDMMYY and OutputFormat YYMMDD
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="inputFormat"></param>
        /// <param name="outputFormat"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string GetDate(this string strDate, DateFormat inputFormat = DateFormat.DDMMYY,
            DateFormat outputFormat = DateFormat.YYMMDD, char outputDateSeperator = '-', string inputDatePattern = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strDate))
                {
                    return default;
                }
                string[] parts = strDate.Split(new char[] { '/', '-', ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Count() == 1 && double.TryParse(strDate, out double OADate))
                    return DateTime.FromOADate(double.Parse(strDate)).ToShortDateString().GetDate(DateFormat.MMDDYY, outputFormat);

                string day = string.Empty;
                string month = string.Empty;
                string year = string.Empty;

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
                        DateTime date;
                        DateTime.TryParseExact(strDate, inputDatePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                        month = date.Month.ToString();
                        day = date.Day.ToString();
                        year = date.Year.ToString();
                        break;
                }
                if (year.Length == 2)
                {
                    year = DateTime.Today.Year.ToString().Left(2) + year;
                }
                switch (outputFormat)
                {
                    case DateFormat.DDMMYY:
                        strDate = $"{day}{outputDateSeperator}{month}{outputDateSeperator}{year}";
                        break;

                    case DateFormat.MMDDYY:
                        strDate = $"{month}{outputDateSeperator}{day}{outputDateSeperator}{year}";
                        break;

                    case DateFormat.YYMMDD:
                        strDate = $"{year}{outputDateSeperator}{month}{outputDateSeperator}{day}";
                        break;
                }
                return strDate;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        public static DateTime GetDateTime(this string dateString, DateFormat input)
        {
            return Convert.ToDateTime(GetDate(dateString, input, DateFormat.MMDDYY));
        }

        public static bool IsBetween(DateTime date, DateTime fromDate, DateTime toDate = default) => date >= fromDate && date <= toDate;

        public static bool IsGreaterThan(DateTime date, DateTime fromDate) => date <= fromDate;

        public static bool IsLessFromToday(DateTime date) => date < DateTime.Now;

        public static bool IsLessThan(DateTime date, DateTime fromDate) => date >= fromDate;

        public static string Month(this int i, bool fullName = false)
        {
            return i <= 12 && i >= 0
                ? new DateTime(2000, i, 1).ToString(fullName ? "MMMM" : "MMM")
                : throw new Exception("Invalid Month Number");
        }

        public static int Month(this string monthName)
        {
            return Convert.ToDateTime("01-" + monthName + "-2011").Month;
        }

        public static IEnumerable<DateTime> ParseDate<T>(this List<T> dates)
        {
            var dateTimes = new List<DateTime>();
            foreach (T date in dates)
            {
                try
                {
                    var dateTime = DateTime.Parse(date.ToString().CharToRemove());
                    dateTimes.Add(dateTime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    CultureInfo enUS = new CultureInfo("en-US");
                    var formatStrings = new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy hh:mm:ss tt", "dd-MM-yyyy hh:mm:ss tt", "yyyy-MM-dd hh:mm:ss" };
                    if (DateTime.TryParseExact(date.ToString(), formatStrings, enUS, DateTimeStyles.None, out DateTime dateValue))
                    {
                        dateTimes.Add(dateValue);
                    }
                }
            }
            return dateTimes;
        }

        public static DateTime ParseDate<T>(this T date)
        {
            string tempDate = StringExtensions.ToString(date);
            if (tempDate == "")
            {
                return default;
            }
            tempDate.CharToRemove();
            try
            {
                return double.TryParse(tempDate, out double dateSerialNumber)
                    ? DateTime.FromOADate(dateSerialNumber) : DateTime.Parse(tempDate);
            }
            catch (Exception ex)
            {
                CultureInfo enUS = new CultureInfo("en-US");
                var formatStrings = new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy hh:mm:ss tt", "dd-MM-yyyy hh:mm:ss tt", "yyyy-MM-dd hh:mm:ss", "ddMMMyy" };
                return DateTime.TryParseExact(tempDate, formatStrings, enUS, DateTimeStyles.None, out DateTime dateTime)
                    ? dateTime : throw ex;
            }
        }

        public static DateTime ToDate(this string strDate, DateFormat inputFormat = DateFormat.DDMMYY)
        {
            return Convert.ToDateTime(strDate.GetDate(inputFormat: inputFormat, outputFormat: DateFormat.MMDDYY));
        }
    }
}