using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

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

        public static List<ColumnMapping> GetMappingList(DataTable mappedTable)
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
    }
}