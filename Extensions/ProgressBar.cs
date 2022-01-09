using System;

namespace JExtensions.Extensions
{
    public static class ProgressBar
    {
        public static void ShowProgressBar(this string message, long processedRecords, long totalRecords)
        {
            var percent = 100 * (processedRecords + 1) / totalRecords;
            Console.Write("\r{0}{1}% complete", message, percent);
            if (processedRecords < totalRecords - 1)
            {
                return;
            }
            Console.WriteLine(Environment.NewLine);
        }
    }
}