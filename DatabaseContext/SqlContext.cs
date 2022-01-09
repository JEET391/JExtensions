using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace JExtensions.DatabaseContext
{
    public class SqlContext
    {
        public SqlContext()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["DBContext"].ConnectionString.ToString().Trim();
            Connection = new SqlConnection(ConnectionString);
        }

        public SqlContext(string connectionString)
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
            Connection = new SqlConnection(ConnectionString);
        }

        private SqlConnection Connection { get; }
        private string ConnectionString { get; }

        /// <summary>
        /// BulkInsert
        /// </summary>
        /// <param name="sourceDataTable"></param>
        /// <param name="destinationTableName">Set the tableName</param>
        /// <param name="conStr">Optional</param>
        /// <returns></returns>
        public void BulkInsert(DataTable sourceDataTable, string destinationTableName)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(ConnectionString, SqlBulkCopyOptions.TableLock))
            {
                bulkCopy.DestinationTableName = destinationTableName;
                bulkCopy.BulkCopyTimeout = sourceDataTable.Rows.Count;
                bulkCopy.WriteToServer(sourceDataTable);
            }
        }

        /// <summary>
        /// Execute Insert/ Update command
        /// </summary>
        /// <param name="cmd">SQL Command object</param>
        /// <returns>No of Rows Effected</returns>
        /// <remarks>ByRef of Command Object would help us managing procedures which would use Output Parameters.</remarks>
        public int ExecuteProcedure(SqlCommand cmd)
        {
            cmd.Connection = Connection;
            return cmd.ExecuteNonQuery();
        }

        public SqlDataReader ExecuteReader(SqlCommand cmd)
        {
            cmd.Connection = Connection;
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Get a Scaler Value
        /// </summary>
        /// <param name="cmd">SQL Command Object</param>
        /// <returns>Object</returns>
        /// <remarks></remarks>
        public object ExecuteScaler(SqlCommand cmd)
        {
            cmd.Connection = Connection;
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// Open Connection
        /// </summary>
        /// <remarks></remarks>

        /// <summary>
        /// Get datarows
        /// </summary>
        /// <param name="cmd">SQL Command Object</param>
        /// <returns>Array of data Row</returns>
        /// <remarks></remarks>
        public DataRow[] GetDataRow(SqlCommand cmd)
        {
            cmd.Connection = Connection;
            var adaptor = new SqlDataAdapter
            {
                SelectCommand = cmd
            };
            var dataTable = new DataTable();
            adaptor.Fill(dataTable);
            return dataTable.Select();
        }

        /// <summary>
        /// Get Dataset
        /// </summary>
        /// <param name="cmd">SQL Command Object</param>
        /// <returns>DataSet</returns>
        /// <remarks></remarks>
        public DataSet GetDataSet(SqlCommand cmd)
        {
            cmd.Connection = Connection;
            var adaptor = new SqlDataAdapter
            {
                SelectCommand = cmd
            };
            var dataSet = new DataSet();
            adaptor.Fill(dataSet);
            return dataSet;
        }

        public DataTable GetDataTable(SqlCommand cmd)
        {
            cmd.Connection = Connection;
            var adaptor = new SqlDataAdapter
            {
                SelectCommand = cmd
            };
            var dataTable = new DataTable();
            adaptor.Fill(dataTable);
            return dataTable;
        }

        /// <summary>
        /// Close Connection
        /// </summary>
        /// <remarks></remarks>
        public void ReleaseConnection()
        {
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="commandText"></param>
        /// <param name="dataTableName"></param>
        /// <param name="conStr"></param>
        /// <returns></returns>
        public void TableInsert(DataTable dataTable, string commandText)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.AddWithValue("@Table", dataTable);
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.StoredProcedure;
                ExecuteProcedure(cmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}