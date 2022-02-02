using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JExtension
{
    public static class JHelper
    {
        public static bool IsValidEmail(this string emailAddress)
        {
            const string pattern = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

            return Regex.IsMatch(emailAddress, pattern);
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
        public static string GetDate(this string strDate, DateFormat inputFormat = DateFormat.DDMMYY, DateFormat outputFormat = DateFormat.YYMMDD, char seperator = '-', string datePattern = "")
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(strDate))
                {
                    string[] parts = strDate.Split(new char[] { '/', '-', ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
                    double OADate = 0;
                    if (parts.Count() == 1 && double.TryParse(strDate, out OADate))
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
                            DateTime.TryParseExact(strDate, datePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
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
                            strDate = day + seperator + month + seperator + year;
                            break;

                        case DateFormat.MMDDYY:
                            strDate = month + seperator + day + seperator + year;
                            break;

                        case DateFormat.YYMMDD:
                            strDate = year + seperator + month + seperator + day;
                            break;
                    }
                }
            }
            catch
            {
                throw new Exception("Invalid Date Format");
            }
            return strDate;
        }

        public static DateTime ToDate(this string strDate, DateFormat inputFormat = DateFormat.DDMMYY)
        {
            return Convert.ToDateTime(strDate.GetDate(inputFormat: inputFormat, outputFormat: DateFormat.MMDDYY));
        }

        public static int Month(this string monthName)
        {
            return Convert.ToDateTime("01-" + monthName + "-2011").Month;
        }

        public static string Month(this int i)
        {
            if (i > 12 || i < 0)
                throw new Exception("Invalid Month Number");
            return new DateTime(2000, i, 1).ToString("MMM");
        }

        public static string FullMonth(this int i)
        {
            if (i > 12 || i < 0)
                throw new Exception("Invalid Month Number");
            return new DateTime(2000, i, 1).ToString("MMMM");
        }

        public static IEnumerable<DateTime> ParseDate<T>(this List<T> dates)
        {
            var dateTime = new DateTime();
            var x = new List<DateTime>();
            foreach (var date in dates)
            {
                //Use the Parse() method
                try
                {
                    dateTime = DateTime.Parse(date.ToString().CharToRemove());
                    x.Add(dateTime);
                }
                catch (Exception ex)
                {
                    CultureInfo enUS = new CultureInfo("en-US");
                    DateTime dateValue;
                    var formatStrings = new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy hh:mm:ss tt", "dd-MM-yyyy hh:mm:ss tt", "yyyy-MM-dd hh:mm:ss" };
                    if (DateTime.TryParseExact(date.ToString(), formatStrings, enUS, DateTimeStyles.None, out dateValue))
                    {
                        x.Add(dateValue);
                    }
                }
            }
            return x;
        }

        public static DateTime ParseDate<T>(this T date)
        {
            var dateTime = new DateTime();
            string tempDate = ToString(date);
            if (tempDate != "")
            {
                tempDate.CharToRemove();
                //Use the Parse() method
                try
                {
                    double x;
                    if (double.TryParse(tempDate, out x))
                    {
                        return DateTime.FromOADate(x);
                    }
                    return DateTime.Parse(tempDate);
                }
                catch (Exception ex)
                {
                    CultureInfo enUS = new CultureInfo("en-US");
                    var formatStrings = new string[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yyyy hh:mm:ss tt", "dd-MM-yyyy hh:mm:ss tt", "yyyy-MM-dd hh:mm:ss", "ddMMMyy" };
                    if (DateTime.TryParseExact(tempDate, formatStrings, enUS, DateTimeStyles.None, out dateTime))
                        return dateTime;
                    else
                        throw ex;
                }
            }
            return dateTime;
        }

        public static object DbNullIfNullOrEmpty<T>(this T str)
        {
            return !string.IsNullOrEmpty(ToString(str)) ? str : (object)DBNull.Value;
        }

        public static DataTable RemoveDuplicateRows(this DataTable dataTable)
        {
            return dataTable.DefaultView.ToTable( /*distinct*/ true);
        }

        public static DataTable RemoveDuplicateRows(this DataTable dataTable, string columnName)
        {
            Hashtable hTable = new Hashtable();
            ArrayList duplicateList = new ArrayList();

            //Add list of all the unique item value to hashtable, which stores combination of key, value pair.
            //And add duplicate item value in arraylist.
            foreach (DataRow drow in dataTable.Rows)
            {
                if (hTable.Contains(drow[columnName]))
                    duplicateList.Add(drow);
                else
                    hTable.Add(drow[columnName], string.Empty);
            }

            //Removing a list of duplicate items from datatable.
            foreach (DataRow dRow in duplicateList)
                dataTable.Rows.Remove(dRow);

            //Datatable which contains unique records will be return as output.
            return dataTable;
        }

        public static DataTable RemoveDuplicateRows(this DataTable dataTable, string[] columnNames)
        {
            foreach (var column in columnNames)
            {
                dataTable.RemoveDuplicateRows(column);
            }
            return dataTable;
        }

        public static DataTable RemoveDuplicateRows(this DataTable dataTable, string[] columnNames, string selectedColumns)
        {
            return dataTable.RemoveDuplicateRows(columnNames).SelectColumn(selectedColumns);
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static List<T> ToList<T>(this DataTable dataTable) where T : new()
        {
            var dataList = new List<T>();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var objFieldNames = (from PropertyInfo aProp in typeof(T).GetProperties(flags)
                                 select new
                                 {
                                     Name = aProp.Name,
                                     Type = Nullable.GetUnderlyingType(aProp.PropertyType) ??
                         aProp.PropertyType
                                 }).ToList();
            var dataTblFieldNames = (from DataColumn aHeader in dataTable.Columns
                                     select new
                                     {
                                         Name = aHeader.ColumnName,
                                         Type = aHeader.DataType
                                     }).ToList();
            var commonFields = objFieldNames.Intersect(dataTblFieldNames).ToList();

            foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
            {
                var aTSource = new T();
                foreach (var aField in commonFields)
                {
                    PropertyInfo propertyInfos = aTSource.GetType().GetProperty(aField.Name);
                    var value = (dataRow[aField.Name] == DBNull.Value) ?
                    null : dataRow[aField.Name]; //if database field is nullable
                    propertyInfos.SetValue(aTSource, value, null);
                }
                dataList.Add(aTSource);
            }
            return dataList;
        }

        public static void AddColumn<T>(this DataTable dataTable, string ColumnName)
        {
            DataColumn column = new DataColumn(ColumnName, typeof(T));
            dataTable.Columns.Add(column);
            dataTable.AcceptChanges();
        }

        public static void AddColumn<T>(this DataTable dataTable, string ColumnName, T DefaultValue)
        {
            DataColumn column = new DataColumn(ColumnName, typeof(T));
            column.DefaultValue = DefaultValue;
            dataTable.Columns.Add(column);
            dataTable.AcceptChanges();
        }

        public static void AddColumn<T>(this DataTable dataTable, string ColumnName, T DefaultValue, OverWrite type)
        {
            if (type == OverWrite.YES)
                dataTable.Columns.Remove(ColumnName);
            AddColumn(dataTable, ColumnName, DefaultValue);
        }

        public static DataTable Merge(this DataSet ds)
        {
            bool header = true;
            DataTable dataTable = new DataTable();
            foreach (DataTable table in ds.Tables)
            {
                if (table.Rows.Count == 0)
                    continue;
                if (header)
                {
                    dataTable = table;
                    header = false;
                    continue;
                }
                foreach (DataRow dr in table.Rows)
                    dataTable.Rows.Add(dr.ItemArray);
            }
            return dataTable;
        }

        public static DataTable Merge(this IList<DataTable> tables, string primaryKeyColumn)
        {
            if (!tables.Any())
                throw new ArgumentException("Tables must not be empty", "tables");
            if (primaryKeyColumn != null)
                foreach (DataTable t in tables)
                    if (!t.Columns.Contains(primaryKeyColumn))
                        throw new ArgumentException("All tables must have the specified primarykey column " + primaryKeyColumn, "primaryKeyColumn");

            if (tables.Count == 1)
                return tables[0];

            DataTable table = new DataTable("TableUnion");
            table.BeginLoadData(); // Turns off notifications, index maintenance, and constraints while loading data
            foreach (DataTable t in tables)
            {
                //table.Merge(t); // same as table.Merge(t, false, MissingSchemaAction.Add);
                table.Merge(t, true, MissingSchemaAction.Ignore);
            }
            table.EndLoadData();

            if (primaryKeyColumn != null)
            {
                // since we might have no real primary keys defined, the rows now might have repeating fields
                // so now we're going to "join" these rows ...
                var pkGroups = table.AsEnumerable()
                    .GroupBy(r => r[primaryKeyColumn]);
                var dupGroups = pkGroups.Where(g => g.Count() > 1);
                foreach (var grpDup in dupGroups)
                {
                    // use first row and modify it
                    DataRow firstRow = grpDup.First();
                    foreach (DataColumn c in table.Columns)
                    {
                        if (firstRow.IsNull(c))
                        {
                            DataRow firstNotNullRow = grpDup.Skip(1).FirstOrDefault(r => !r.IsNull(c));
                            if (firstNotNullRow != null)
                                firstRow[c] = firstNotNullRow[c];
                        }
                    }
                    // remove all but first row
                    var rowsToRemove = grpDup.Skip(1);
                    foreach (DataRow rowToRemove in rowsToRemove)
                        table.Rows.Remove(rowToRemove);
                }
            }

            return table;
        }

        public static List<string> ValidateColumn(this DataTable dataTable, List<string> columnList, bool mapping = false)
        {
            if (dataTable.Columns.Contains("NoName"))
            {
                dataTable.Columns.Remove("NoName");
            }
            var dataTableColumns = (from DataColumn dc in dataTable.Columns select dc.ColumnName.ToUpper());
            var columns = mapping ? columnList.EvenItems().Select(x => x.ToUpper()) : columnList.Select(x => x.ToUpper());
            var invalidColumns = (from x in columns where !x.In(dataTableColumns.ToArray()) select x);
            return invalidColumns.ToList();
        }

        public static DataTable SelectColumn(this DataTable dataTable, string columnNames)
        {
            DataView view = new DataView(dataTable);
            return view.ToTable(dataTable.TableName, false, columnNames.Split(','));
        }

        public static List<string> ColumnNames(this DataTable dataTable)
        {
            return (from DataColumn dc in dataTable.Columns select dc.ColumnName).ToList();
        }

        public static IEnumerable<T> RowData<T>(this DataRow dr)
        {
            return dr.ItemArray.Cast<T>().ToArray();
        }

        public static IEnumerable RowData(this DataRow dr)
        {
            return dr.ItemArray;
        }

        public static IEnumerable<T> ColumnData<T>(this DataTable dt, string columnName)
        {
            return dt.AsEnumerable().Select(x => x.Field<T>(columnName)).ToArray();
        }

        public static IEnumerable<object> ColumnData(this DataTable dt, string columnName)
        {
            return dt.AsEnumerable().Select(x => x[columnName]).ToArray();
        }

        public static IEnumerable<T> ColumnData<T>(this DataTable dt, int columnIndex)
        {
            return dt.AsEnumerable().Select(x => x.Field<T>(columnIndex)).ToArray();
        }

        public static IEnumerable<object> ColumnData(this DataTable dt, int columnIndex)
        {
            return dt.AsEnumerable().Select(x => x[columnIndex]).ToArray();
        }

        public static void ChangeType<T>(this DataTable dataTable, string ColumnName, Type type)
        {
            foreach (DataRow dr in dataTable.Rows)
                dr[ColumnName] = Convert.ChangeType(dr[ColumnName], type);
        }

        public static void ChangeType<T>(this DataTable dataTable, int ColumnIndex, Type type)
        {
            foreach (DataRow dr in dataTable.Rows)
                dr[ColumnIndex] = Convert.ChangeType(dr[ColumnIndex], type);
        }

        public static List<DateTime> ValidateDate(this DataTable dataTable, string column, DateTime fromDate, DateTime toDate, DateFormat dateFormat)
        {
            try
            {
                var strDates = dataTable.ColumnData(column).Distinct();
                var strTransactionDates = from x in strDates select Convert.ToDateTime(x.ToString().GetDate(dateFormat, DateFormat.MMDDYY));
                return (from t in strTransactionDates where (t.Date < fromDate || t.Date > toDate) select t).ToList();
            }
            catch
            {
                throw new Exception("InvalidDate:TransactionDate has multiple date fromats or it cannot  be blank.");
            }
        }

        public static List<DateTime> ValidateDate(this List<string> strDates, string column, DateTime fromDate, DateTime toDate, DateFormat dateFormat)
        {
            try
            {
                var strTransactionDates = from x in strDates select Convert.ToDateTime(x.GetDate(dateFormat, DateFormat.MMDDYY));
                return (from t in strTransactionDates where (t.Date < fromDate || t.Date > toDate) select t).ToList();
            }
            catch
            {
                throw new Exception("InvalidDate:TransactionDate has multiple date fromats or it cannot  be blank.");
            }
        }

        public static List<string> GetData(string fileName)
        {
            string file = fileName;
            List<string> lines = File.ReadAllLines(file).Select(x => x.Split(',')[0]).ToList();
            lines.RemoveAt(0);
            int rows = lines.Count;
            return lines;
        }

        public static List<string> GetData(string fileName, int colIndex)
        {
            string file = fileName;
            List<string> lines = File.ReadAllLines(file).Select(x => x.Split(',')[4]).ToList();
            lines.RemoveAt(0);
            int rows = lines.Count;
            return lines;
        }

        public static IEnumerable<string> SortByLength(this IEnumerable<string> e)
        {
            // Use LINQ to sort the array received and return a copy.
            var sorted = from s in e
                         orderby s.Length ascending
                         select s;
            return sorted;
        }

        public static IEnumerable<string> OddItems(this List<string> list)
        {
            return list.Where((x, i) => i % 2 != 0);
        }

        public static IEnumerable<string> EvenItems(this List<string> list)
        {
            return list.Where((x, i) => i % 2 == 0);
        }

        public static bool In<T>(this T source, params T[] list)
        {
            return list.Contains(source);
        }

        public static IEnumerable<TSource> Between<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult lowest, TResult highest) where TResult : IComparable<TResult>
        {
            return source.OrderBy(selector).
                SkipWhile(s => selector.Invoke(s).CompareTo(lowest) < 0).
                TakeWhile(s => selector.Invoke(s).CompareTo(highest) <= 0);
        }

        public static bool IsAlphaNumeric(this string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,]{6,}$");
            return rg.IsMatch(strToCheck);
        }

        public static void ShowProgressBar(string message, long processed, long total)
        {
            long percent = (100 * (processed + 1)) / total;
            Console.Write("\r{0}{1}% complete", message, percent);
            if (processed >= total - 1)
            {
                Console.WriteLine(Environment.NewLine);
            }
        }

        public static void WriteFile<T>(this T line, string fileName, bool append = false, bool OverWrite = false)
        {
            if (OverWrite)
            {
                fileName.DeleteFile();
            }
            using (StreamWriter sw = new StreamWriter(fileName, append))
            {
                sw.WriteLine(line);
            }
        }

        public static void WriteFile<T>(this List<T> lines, string fileName, bool append = false, bool Overwrite = false)
        {
            if (Overwrite)
            {
                fileName.DeleteFile();
            }
            using (StreamWriter sw = new StreamWriter(fileName, append))
            {
                foreach (T line in lines)
                {
                    sw.WriteLine(line);
                }
            }
        }

        public static void DeleteFile(this string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public static void DeleteFiles(this string directory, string searchPattern)
        {
            Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly).ToList().ForEach(x => x.DeleteFile());
        }

        public static void DeleteFile(this string file, int DaysBeforeOrAfter)
        {
            if (File.Exists(file))
            {
                //Go to N days Back Date then Delete the Files having File CreatedDate Less than Last N Days Prevoius
                if (File.GetCreationTime(file) < DateTime.Now.AddDays(DaysBeforeOrAfter))
                    File.Delete(file);
            }
        }
    }
}