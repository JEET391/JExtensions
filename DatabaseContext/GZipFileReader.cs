using JExtensions.Constants;
using JExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;

namespace JExtensions.DatabaseContext
{
    public class GZipFileReader
    {
        private static DataTable dataTable = new DataTable();

        public GZipFileReader(string zipFile)
        {
            ZipFile = zipFile;
        }

        public int TotalRecords { get; set; }

        //private FileLog fileLog = null;
        public string ZipFile { get; }

        public void Read(int BulkInsertSize)
        {
            List<string> lines = new List<string>();
            using (FileStream reader = File.OpenRead(ZipFile))
            using (GZipStream zip = new GZipStream(reader, CompressionMode.Decompress, true))
            using (StreamReader unzip = new StreamReader(zip))
            {
                int numberOfLines = 0;
                while (!unzip.EndOfStream)
                {
                    lines.Add(unzip.ReadLine());
                    numberOfLines++;
                    if (numberOfLines == BulkInsertSize)
                    {
                        ProcessLines(lines);
                        lines.Clear();
                        numberOfLines = 0;
                    }
                }
                //REST OF LINES < 50000
                ProcessLines(lines);
                lines.Clear();
            }
        }

        public void Read(int BulkInsertSize, string tableName, string columns)
        {
            List<string> lines = new List<string>();
            using (FileStream reader = File.OpenRead(ZipFile))
            using (GZipStream zip = new GZipStream(reader, CompressionMode.Decompress, true))
            using (StreamReader unzip = new StreamReader(zip))
            {
                int numberOfLines = 0;
                while (!unzip.EndOfStream)
                {
                    lines.Add(unzip.ReadLine());
                    numberOfLines++;
                    if (numberOfLines == BulkInsertSize)
                    {
                        ProcessLines(lines, tableName, columns);
                        lines.Clear();
                        numberOfLines = 0;
                    }
                }
                //REST OF LINES < 50000
                ProcessLines(lines, tableName, columns);
                lines.Clear();
            }
        }

        private void ProcessLines(List<string> lines)
        {
            try
            {
                string columns = Constants.Constant.TableColumn;
                lines.Insert(0, columns);
                //CREATE _tempFILE
                //GZip.CreateFile(lines, TempFile, false, true);
                ExcelContext excelContext = new ExcelContext(ZipFile);
                dataTable = excelContext.Read();

                //ADD FILE INFO
                dataTable.AddColumn<int>("FILEID");
                dataTable.AcceptChanges();

                SqlContext sqlContext = new SqlContext();
                //sqlContext.ExecuteQuery("Truncate Table _temp" + Constants._TableName);
                sqlContext.BulkInsert(dataTable, "_temp" + Constant.TableName);
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = Constant.INSERT_DATATABLE,
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 0
                };
                int i = sqlContext.ExecuteProcedure(cmd);
                TotalRecords += dataTable.Rows.Count;
                Console.WriteLine(string.Format("FileName {0} Processed Records {1}", ZipFile, TotalRecords));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        private void ProcessLines(List<string> lines, string tableName, string columns)
        {
            try
            {
                SqlContext dbContext = new SqlContext();
                lines.Insert(0, columns);
                //CREATE _tempFILE
                //GZip.CreateFile(lines, TempFile, false, true);
                ExcelContext excel = new ExcelContext(ZipFile);
                dataTable = excel.Read();

                //ADD FILE INFO
                dataTable.AddColumn<int>("FILEID");
                //dbContext.ExecuteQuery("Truncate Table _temp" + tableName);
                dbContext.BulkInsert(dataTable, "_temp" + tableName);
                SqlCommand cmd = new SqlCommand
                {
                    CommandText = Constant.INSERT_DATATABLE,
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 0
                };
                int i = dbContext.ExecuteProcedure(cmd);
                TotalRecords += dataTable.Rows.Count;
                Console.WriteLine(string.Format("FileName {0} Processed Records {1}", ZipFile, TotalRecords));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }
    }
}