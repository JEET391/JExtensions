using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

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