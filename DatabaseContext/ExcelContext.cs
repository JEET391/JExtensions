using Excel;
using JExtensions.Enums;
using JExtensions.Extensions;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;

namespace JExtensions.DatabaseContext
{
    public class ExcelContext
    {
        public ExcelContext(string excelFile, bool htmlContent = false)
        {
            ExcelFileInfo = new FileInfo(excelFile);

            if (!ExcelFileInfo.Exists)
            {
                throw new FileNotFoundException($"{nameof(excelFile)} file not exists.", excelFile);
            }
            var extension = ExcelFileInfo.Extension.ToLower();

            if (extension != ".xls" || extension != ".xlsx" || extension != ".csv" || extension != ".xlsb")
            {
                throw new Exception($"{nameof(excelFile)} cannot read file type {extension}.");
            }
            if (htmlContent)
            {
                ConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ExcelFileInfo.FullName};Extended Properties=\"HTML Import;HDR=YES;ImportMixedTypes=Text;\"";
            }
            else
            {
                switch (extension)
                {
                    case ".xlsx":
                        ConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ExcelFileInfo.FullName};Extended Properties=\"Excel 12.0 Xml;IMEX =1;HDR=YES;ImportMixedTypes=Text;\"";
                        break;

                    case ".xlsb":
                        ConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ExcelFileInfo.FullName};Extended Properties=\"Excel 12.0;HDR=YES;ImportMixedTypes=Text;\"";
                        break;

                    case ".xls":
                        ConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ExcelFileInfo.FullName};Extended Properties=\"Excel 8.0;HDR=YES;ImportMixedTypes=Text;\"";
                        break;

                    case ".csv":
                        ConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={ExcelFileInfo.DirectoryName};Extended Properties=\"Text;HDR=YES;ImportMixedTypes=Text;FMT=Delimited(,)\"";
                        break;
                }
            }

            Connection = new OleDbConnection(ConnectionString);
        }

        public FileInfo ExcelFileInfo { get; }
        private OleDbConnection Connection { get; }
        private string ConnectionString { get; }

        private string WorkSheetName { get; set; }
        private List<string> WorkSheetNames { get; set; }

        public string ExportToExcel(DataSet ds, string outputDirectory, string fileName, bool appendDateTimeStampInExportedFile)
        {
            fileName = appendDateTimeStampInExportedFile ? fileName.AppendDateTimeStamp(Formats.DateTimeStamp) : fileName;
            return ExportToExcel(ds, Path.Combine(outputDirectory, fileName + ".csv"));
        }

        public string ExportToExcel(DataTable dt, string outputDirectory, string fileName, bool appendDateTimeStampInExportedFile)
        {
            fileName = appendDateTimeStampInExportedFile ? fileName.AppendDateTimeStamp(Formats.DateTimeStamp) : fileName;
            return ExportToExcel(dt, Path.Combine(outputDirectory, fileName + ".csv"));
        }

        public string ExportToExcel(DataSet ds, string outputDirectory, string fileName, bool includeHeaders, bool appendDateTimeStampInExportedFile, string delimiter, Quote quote)
        {
            fileName = appendDateTimeStampInExportedFile ? fileName.AppendDateTimeStamp(Formats.DateTimeStamp) : fileName;
            return ExportToExcel(ds, Path.Combine(outputDirectory, fileName + ".csv"), includeHeaders, delimiter, quote);
        }

        public string ExportToExcel(DataTable dt, string outputDirectory, string fileName, bool includeHeaders, bool appendDateTimeStampInExportedFile, string delimiter, Quote quote)
        {
            fileName = appendDateTimeStampInExportedFile ? fileName.AppendDateTimeStamp(Formats.DateTimeStamp) : fileName;
            return ExportToExcel(dt, Path.Combine(outputDirectory, fileName + ".csv"), includeHeaders, delimiter, quote);
        }

        public DataTable GetMappedTable(List<string> mappingList, DataTable dataTable)
        {
            try
            {
                string invalidColumns = string.Join(",", ValidateColumns(dataTable, mappingList));
                if (invalidColumns != "")
                {
                    throw new Exception("Columns not matched : " + invalidColumns);
                }
                return GetMappedTable(dataTable, mappingList);
            }
            catch (Exception ex)
            {
                throw new Exception("There might be some error while reading file :" + ex.Message);
            }
        }

        public DataTable GetMappedTable(List<ColumnMapping> mappingList, DataTable dataTable)
        {
            try
            {
                string invalidColumns = string.Join(",", ValidateColumns(dataTable, mappingList));
                if (invalidColumns != "")
                {
                    throw new Exception("Columns not matched : " + invalidColumns);
                }
                return GetMappedTable(dataTable, mappingList);
            }
            catch (Exception ex)
            {
                throw new Exception("There might be some error while reading file :" + ex.Message);
            }
        }

        public List<ColumnMapping> GetMappingList(DataTable mappedTable)
        {
            var excelMapping = new List<ColumnMapping>();
            foreach (DataRow dr in mappedTable.Rows)
            {
                excelMapping.Add(new ColumnMapping { FromColumn = dr[0].ToString(), ToColumn = dr[1].ToString() });
            }
            return excelMapping;
        }

        public DataTable Read()
        {
            try
            {
                OpenConnection();
                SetWorkSheets();
                WorkSheetName = WorkSheetNames[0].ToString();
                return GetDataFromWorkSheet();
            }
            finally
            {
                CloseConnections();
            }
        }

        public DataTable Read(int sheetIndex)
        {
            try
            {
                OpenConnection();
                SetWorkSheets();
                WorkSheetName = WorkSheetNames[sheetIndex].ToString();
                return GetDataFromWorkSheet();
            }
            finally
            {
                CloseConnections();
            }
        }

        public DataTable Read(string tableName)
        {
            try
            {
                OpenConnection();
                SetWorkSheets();
                WorkSheetNames.Sort();
                int index = WorkSheetNames.BinarySearch(tableName);
                WorkSheetName = WorkSheetNames[index].ToString();
                return GetDataFromWorkSheet();
            }
            finally
            {
                CloseConnections();
            }
        }

        public DataTable Read(string strFile, List<ColumnMapping> mappingList)
        {
            DataTable dt = Read(strFile);
            string invalidColumns = string.Join(",", ValidateColumns(dt, mappingList));
            if (invalidColumns != "")
            {
                throw new Exception("Columns not matched : " + invalidColumns);
            }
            return GetMappedTable(dt, mappingList);
        }

        public DataSet ReadAsDataSet()
        {
            DataSet ds = new DataSet();
            FileStream stream = ExcelFileInfo.OpenRead();

            IExcelDataReader excelReader;
            switch (ExcelFileInfo.Extension.ToLower())
            {
                case ".csv":
                    ds.Tables.Add(ReadCSV(stream));
                    return ds;

                case ".xls":
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                    excelReader.IsFirstRowAsColumnNames = true;
                    ds = excelReader.AsDataSet();
                    excelReader.Close();
                    return ds;

                case ".xlsx":
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    excelReader.IsFirstRowAsColumnNames = true;
                    ds = excelReader.AsDataSet();
                    excelReader.Close();
                    return ds;

                default:
                    throw new Exception("ExcelDataReaderFactory() - unknown/unsupported file extension");
            }
        }

        public DataTable ReadAsDataTable()
        {
            DataSet ds = ReadAsDataSet();
            if (ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            return new DataTable();
        }

        public DataTable ReadAsDataTable(int columnIndex, bool removeEmptyRows)
        {
            DataTable dt = ReadAsDataSet().Tables[0];

            if (dt.Rows.Count > 0)
            {
                if (removeEmptyRows)
                {
                    return RemoveEmptyRows(dt, columnIndex);
                }
            }
            return dt;
        }

        public DataTable ReadAsDataTable(string columnName, bool removeEmptyRows)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));
            }

            var dt = ReadAsDataSet().Tables[0];
            return dt.Rows.Count > 0 && removeEmptyRows ? RemoveEmptyRows(dt, columnName) : dt;
        }

        public DataTable ReadAsSection(string headerFirstColumnName)
        {
            try
            {
                DataTable dataTable = ReadAsDataTable();
                return ReadAsSection(dataTable, headerFirstColumnName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable ReadAsSection(DataTable dataTable, string headerFirstColumnName)
        {
            try
            {
                if (dataTable.ColumnNames()[0] == headerFirstColumnName)
                {
                    return dataTable;
                }
                else
                {
                    int header = 0;
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        if (StringExtensions.ToString(dr[0]) == headerFirstColumnName)
                        {
                            break;
                        }
                        header++;
                    }
                    //Create New Datatable Schema
                    DataTable dt_new = new DataTable();
                    var row = dataTable.Rows[header].RowData();
                    int blankColumn = 0;
                    foreach (var x in row)
                    {
                        if (StringExtensions.ToString(x) == "")
                        {
                            dataTable.Columns.RemoveAt(blankColumn);
                            continue;
                        }
                        blankColumn++;
                        dt_new.Columns.Add(x.ToString().Trim());
                    }
                    dataTable.AcceptChanges();
                    dataTable.AsEnumerable().Skip(header + 1).CopyToDataTable(dt_new, LoadOption.OverwriteChanges);
                    return dt_new;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable ReadAsSection(DataTable dataTable, int header)
        {
            try
            {
                //Create New Datatable Schema
                DataTable dt_new = new DataTable();
                var row = dataTable.Rows[header].RowData<string>();
                foreach (string x in row)
                {
                    dt_new.Columns.Add(x);
                }
                dataTable.AsEnumerable().Skip(header + 1).CopyToDataTable(dt_new, LoadOption.OverwriteChanges);
                return dt_new;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable ReadCSV(Stream stream)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(stream))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    //read column names
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column)
                        {
                            AllowDBNull = true
                        };
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return csvData;
        }

        public DataTable ReadCSV(string strFile)
        {
            FileStream stream = File.Open(strFile, FileMode.Open, FileAccess.Read);
            return ReadCSV(stream);
        }

        public DataTable RemoveEmptyRows(DataTable dataTable, int columnIndex)
        {
            return (from row in dataTable.AsEnumerable()
                    where !string.IsNullOrWhiteSpace(row[columnIndex].ToString())
                    select row).CopyToDataTable();
        }

        public DataTable RemoveEmptyRows(DataTable dataTable, string columnName)
        {
            return (from row in dataTable.AsEnumerable()
                    where !string.IsNullOrWhiteSpace(row[columnName].ToString())
                    select row).CopyToDataTable();
        }

        public List<string> ValidateColumns(DataTable dataTable, List<string> mappedList)
        {
            if (dataTable.Columns.Contains("NoName"))
            {
                dataTable.Columns.Remove("NoName");
            }
            var dataTableColumns = (from DataColumn dc in dataTable.Columns select dc.ColumnName.ToUpper());
            var columns = mappedList.EvenIndexItems().Select(x => x.ToUpper());
            var invalidColumns = (from x in columns where !x.In(dataTableColumns.ToArray()) select x);
            return invalidColumns.ToList();
        }

        public List<string> ValidateColumns(DataTable dataTable, List<ColumnMapping> mappedList)
        {
            if (dataTable.Columns.Contains("NoName"))
            {
                dataTable.Columns.Remove("NoName");
            }
            var dataTableColumns = (from DataColumn dc in dataTable.Columns select dc.ColumnName.ToUpper());
            var columns = mappedList.Select(x => x.FromColumn.ToUpper());
            var invalidColumns = (from x in columns where !x.In(dataTableColumns.ToArray()) select x);
            return invalidColumns.ToList();
        }

        private void CloseConnections()
        {
            if (Connection == null)
            {
                return;
            }
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }

            Connection.Dispose();
        }

        private string ExportToExcel(DataTable dataTable, string outputPath)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(outputPath));
            using (StreamWriter writer = File.CreateText(outputPath))
                WriteDataTable(dataTable, writer, true);
            return outputPath;
        }

        private string ExportToExcel(DataSet dataSet, string outputPath)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(outputPath));
            using (StreamWriter writer = File.CreateText(outputPath))
            {
                bool includeHeaders = true;
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    if (includeHeaders)
                    {
                        WriteDataTable(dataTable, writer, includeHeaders);
                        includeHeaders = false;
                        continue;
                    }
                    WriteDataTable(dataTable, writer, includeHeaders);
                }
            }
            return outputPath;
        }

        private string ExportToExcel(DataTable dataTable, string outputPath, bool includeHeaders, string delimiter, Quote quote)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(outputPath));
            using (StreamWriter writer = File.CreateText(outputPath))
                WriteDataTable(dataTable, writer, includeHeaders, delimiter, quote);
            return outputPath;
        }

        private string ExportToExcel(DataSet dataSet, string outputPath, bool includeHeaders, string delimiter, Quote quote)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(outputPath));
            using (StreamWriter writer = File.CreateText(outputPath))
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    if (includeHeaders)
                    {
                        WriteDataTable(dataTable, writer, includeHeaders, delimiter, quote);
                        includeHeaders = false;
                        continue;
                    }
                    WriteDataTable(dataTable, writer, includeHeaders, delimiter, quote);
                }
            }
            return outputPath;
        }

        private DataTable GetDataFromWorkSheet()
        {
            try
            {
                var selectString = $"SELECT * FROM [{WorkSheetName}]";
                using (var command = new OleDbCommand(selectString, Connection) { CommandType = CommandType.Text })
                {
                    using (var dataAdapter = new OleDbDataAdapter(command))
                    {
                        var workSheetData = new DataTable();
                        dataAdapter.Fill(workSheetData);
                        return workSheetData;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetMappedTable(DataTable fromDataTable, List<string> mappedList)
        {
            var fromDataTableColumns = mappedList.EvenIndexItems().ToList();
            var toDataTableColumns = mappedList.OddIndexItems().ToList();
            DataTable toDataTable = new DataTable();
            foreach (string column in toDataTableColumns)
            {
                toDataTable.Columns.Add(column);
            }
            int n = toDataTableColumns.Count;
            foreach (DataRow fromRow in fromDataTable.Rows)
            {
                DataRow toRow = toDataTable.NewRow();
                for (int i = 0; i < n; i++)
                {
                    toRow[toDataTableColumns[i].ToString()] = fromRow[fromDataTableColumns[i].ToString()];
                }
                toDataTable.Rows.Add(toRow);
            }
            toDataTable.AcceptChanges();
            return toDataTable;
        }

        private DataTable GetMappedTable(DataTable fromDataTable, List<ColumnMapping> mappedList)
        {
            List<string> fromDataTableColumns = mappedList.Select(x => x.FromColumn).ToList();
            List<string> toDataTableColumns = mappedList.Select(x => x.ToColumn).ToList();
            DataTable toDataTable = new DataTable();
            foreach (string column in toDataTableColumns)
            {
                toDataTable.Columns.Add(column);
            }
            int n = toDataTableColumns.Count;
            foreach (DataRow fromRow in fromDataTable.Rows)
            {
                DataRow toRow = toDataTable.NewRow();
                for (int i = 0; i < n; i++)
                {
                    toRow[toDataTableColumns[i].ToString()] = fromRow[fromDataTableColumns[i].ToString()];
                }
                toDataTable.Rows.Add(toRow);
            }
            toDataTable.AcceptChanges();
            return toDataTable;
        }

        private void OpenConnection()
        {
            if (Connection == null)
            {
                return;
            }
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
        }

        private void SetWorkSheets()
        {
            WorkSheetNames = new List<string>();
            if (ExcelFileInfo.Extension.ToLower() == ".csv")
            {
                WorkSheetNames.Add(ExcelFileInfo.Name);
                return;
            }

            //To Get Worksheet Names from All Sheets
            var workSheetNamesDataTable = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            if (workSheetNamesDataTable == null)
            {
                return;
            }

            foreach (DataRow workSheet in workSheetNamesDataTable.Rows)
            {
                if (workSheet == null)
                {
                    continue;
                }
                var tableName = workSheet["TABLE_NAME"].ToString();
                if (string.IsNullOrEmpty(tableName))
                {
                    continue;
                }
                if (tableName.Contains("_xlnm"))
                {
                    continue;
                }
                WorkSheetNames.Add(tableName);
            }
        }

        private void WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders, string delimiter, Quote quote)
        {
            if (includeHeaders)
            {
                IEnumerable<string> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => column.ColumnName.AddQuote(Quote.DoubleQuote));

                writer.WriteLine(string.Join(",", headerValues));
            }
            IEnumerable<string> items = null;

            foreach (DataRow row in sourceTable.Rows)
            {
                items = row.ItemArray.Select(o => o.ToString().AddQuote(quote));
                writer.WriteLine(string.Join(delimiter, items));
            }
            writer.Flush();
        }

        private void WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders)
        {
            if (includeHeaders)
            {
                IEnumerable<string> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => column.ColumnName.AddQuote(Quote.DoubleQuote));

                writer.WriteLine(string.Join(",", headerValues));
            }
            IEnumerable<string> items = null;

            foreach (DataRow row in sourceTable.Rows)
            {
                items = row.ItemArray.Select(o => (o.ToString().AddQuote(Quote.DoubleQuote)));
                writer.WriteLine(string.Join(",", items));
            }
            writer.Flush();
        }
    }
}