using JExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JExtensions.Extensions
{
    public static class FileExtensions
    {
        public static void WriteFile(this string line, string fileName, bool append)
        {
            using (StreamWriter sw = new StreamWriter(fileName, append))
            {
                sw.WriteLine(line);
            }
        }

        public static void WriteFile(this string[] lines, string fileName, bool append)
        {
            using (var sw = new StreamWriter(fileName, append))
            {
                foreach (string line in lines)
                {
                    sw.WriteLine(line);
                }
            }
        }

        public static void Delete(this string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            File.Delete(fileName);
        }

        public static void DeleteFiles(this string path, string searchPattern)
        {
            Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly)
                .ToList()
                .ForEach(x => x.Delete());
        }

        /// <summary>
        /// Delete the File Days ex.. for past - and + postive for future
        /// </summary>
        /// <param name="file"></param>
        /// <param name="DaysBeforeOrAfter"></param>
        public static void Delete(this string file, int days)
        {
            var fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
            {
                return;
            }

            if (fileInfo.CreationTime < DateTime.Now.AddDays(days))
            {
                fileInfo.Delete();
            }
        }
    }
}