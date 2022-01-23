namespace JExtensions.Constants
{
    public struct DateFormats
    {
        public static char DateSeperator = '-';
        public static string Date = $"yyyy{DateSeperator}MM{DateSeperator}dd";
        public static string DateTime = $"{Date} {Time}";
        public static string DateTime24 = $"{Date} {Time24}";
        public static string DateTimeStamp = $"{Date} {Time} ffftt";
        public static string DateTimeStamp24 = $"{Date} {Time24} ffftt";
        public static string Time = "hh:mm:ss";
        public static string Time24 = "HH:mm:ss";
    }
}