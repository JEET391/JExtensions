using JExtensions.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JExtensions.Extensions
{
    public static class DataTableExtensions
    {
        public static void AddColumn<T>(this DataTable dataTable, string columnName)
        {
            dataTable.Columns.Add(new DataColumn(columnName, typeof(T)));
            dataTable.AcceptChanges();
        }

        public static void AddColumn<T>(this DataTable dataTable, string columnName, T columnDefaultValue)
        {
            dataTable.Columns.Add(new DataColumn(columnName, typeof(T))
            {
                DefaultValue = columnDefaultValue
            });
            dataTable.AcceptChanges();
        }

        public static void AddColumn<T>(this DataTable dataTable, string columnName, T columnDefaultValue, bool overWrite)
        {
            if (overWrite)
            {
                dataTable.Columns.Remove(columnName);
            }
            AddColumn(dataTable, columnName, columnDefaultValue);
            dataTable.AcceptChanges();
        }

        public static IEnumerable<TSource> Between<TSource, TResult>
            (this IEnumerable<TSource> source, Func<TSource, TResult> selector,
            TResult lowest, TResult highest) where TResult : IComparable<TResult>
        {
            return source.OrderBy(selector).
                SkipWhile(s => selector.Invoke(s).CompareTo(lowest) < 0).
                TakeWhile(s => selector.Invoke(s).CompareTo(highest) <= 0);
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

        public static IEnumerable<T> ColumnData<T>(this DataTable dt, string columnName)
        {
            return dt.AsEnumerable().Select(x => x.Field<T>(columnName)).ToArray();
        }

        public static IEnumerable<T> ColumnData<T>(this DataTable dt, int columnIndex)
        {
            return dt.AsEnumerable().Select(x => x.Field<T>(columnIndex)).ToArray();
        }

        public static IEnumerable<object> ColumnData(this DataTable dt, int columnIndex)
        {
            return dt.AsEnumerable().Select(x => x[columnIndex]).ToArray();
        }

        public static List<string> ColumnNames(this DataTable dataTable)
        {
            return (from DataColumn dc in dataTable.Columns select dc.ColumnName).ToList();
        }

        public static object DbNullIfNullOrEmpty<T>(this T str)
        {
            return !string.IsNullOrEmpty(StringExtensions.ToString(str)) ? str : (object)DBNull.Value;
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
            List<string> lines = File.ReadAllLines(file).Select(x => x.Split(',')[colIndex]).ToList();
            lines.RemoveAt(0);
            int rows = lines.Count;
            return lines;
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

        public static IEnumerable<T> RowData<T>(this DataRow dr)
        {
            return dr.ItemArray.Cast<T>().ToArray();
        }

        public static IEnumerable RowData(this DataRow dr)
        {
            return dr.ItemArray;
        }

        public static DataTable SelectColumn(this DataTable dataTable, string columnNames)
        {
            DataView view = new DataView(dataTable);
            return view.ToTable(dataTable.TableName, false, columnNames.Split(','));
        }

        public static IEnumerable<string> SortByLength(this IEnumerable<string> e)
        {
            // Use LINQ to sort the array received and return a copy.
            var sorted = from s in e
                         orderby s.Length ascending
                         select s;
            return sorted;
        }

        public static DataTable ToDataTable<T>(this IList<T> list)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ??
                    prop.PropertyType);
            }

            foreach (T item in list)
            {
                object[] values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
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

        public static List<T> ToList<T>(this DataTable dataTable)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        public static IEnumerable<string> GetColumnsNotInDataTable(this
            DataTable dataTable, List<string> hasColumns, bool mapping = false)
        {
            if (dataTable.Columns.Contains("NoName"))
            {
                dataTable.Columns.Remove("NoName");
            }

            var dataTableColumns = dataTable.GetColumnNames();

            var columns = mapping
                ? hasColumns.EvenIndexItems().Select(x => x.ToUpper())
                : hasColumns.Select(x => x.ToUpper());
            return from c in columns where c.NotIn(dataTableColumns) select c;
        }

        public static IEnumerable<string> GetColumnNames(this DataColumnCollection columns)
        {
            return from DataColumn dc in columns select dc.ColumnName.ToUpper();
        }

        public static IEnumerable<string> GetColumnNames(this DataTable dataTable)
        {
            return from DataColumn dc in dataTable.Columns select dc.ColumnName.ToUpper();
        }

        public static IEnumerable<DateTime> ValidateDate(this DataTable dataTable, string column, DateTime fromDate, DateTime toDate, Formats inputDateFormat)
        {
            if (dataTable is null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }

            if (string.IsNullOrEmpty(column))
            {
                throw new ArgumentException($"'{nameof(column)}' cannot be null or empty.", nameof(column));
            }

            try
            {
                var strDates = dataTable.ColumnData<object>(column).Distinct();

                var strTransactionDates = from x in strDates
                                          where x != null
                                          select Convert.ToDateTime(x.ToString()
                                          .GetDate(inputDateFormat, Formats.mmddyyy));
                return from t in strTransactionDates
                       where t.Between(fromDate, toDate)
                       select t;
            }
            catch
            {
                throw new Exception("InvalidDate:TransactionDate has multiple date fromats or it cannot  be blank.");
            }
        }

        public static IEnumerable<DateTime> ValidateDate(this List<string> strDates, DateTime fromDate, DateTime toDate, Formats inputDateFormat)
        {
            try
            {
                var strTransactionDates =
                from x in strDates
                select
                Convert.ToDateTime(x.GetDate(inputDateFormat, Formats.mmddyyy));
                return from t in strTransactionDates
                       where t.Between(fromDate, toDate)
                       select t;
            }
            catch
            {
                throw new Exception("InvalidDate:TransactionDate has multiple date fromats or it cannot  be blank.");
            }
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (var property in temp.GetProperties())
                {
                    if (property.Name != column.ColumnName || dr[property.Name] == DBNull.Value)
                    {
                        continue;
                    }
                    else
                    {
                        property.SetValue(obj, dr[column.ColumnName], null);
                    }
                }
            }
            return obj;
        }
    }
}