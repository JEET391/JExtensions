using System.Configuration;

namespace JExtensions.Constants
{
    public static class Constant
    {
        internal static readonly string GET_NOT_UPLOADED_FILES = "GET_NOT_UPLOADED_FILES";
        internal static readonly string INSERT_DATATABLE = "INSERT_DATATABLE";
        internal static readonly string INSERT_FILE_SUMMARY = "INSERT_FILE_SUMMARY";

        internal static int BulkInsertSize => int.Parse(ConfigurationManager.AppSettings["BulkInsertSize"].ToString());
        internal static int DeletePeriod => int.Parse(ConfigurationManager.AppSettings["DeletePeriod"].ToString());
        internal static string FTP => ConfigurationManager.AppSettings["FTP.Port"].ToString();
        internal static string FTPDirectory => ConfigurationManager.AppSettings["FTP.Directory"].ToString();
        internal static string LogTable => ConfigurationManager.AppSettings["LogTable"].ToString();
        internal static string Password => ConfigurationManager.AppSettings["FTP.Password"].ToString();
        internal static string TableColumn => ConfigurationManager.AppSettings["TableColumn"].ToString();
        internal static string TableName => ConfigurationManager.AppSettings["TableName"].ToString();
        internal static string UploadDirectory => ConfigurationManager.AppSettings["UploadDirectory"].ToString();
        internal static string UserName => ConfigurationManager.AppSettings["FTP.UserName"].ToString();
        internal static string WebRootPath => ConfigurationManager.AppSettings["WebRootPath"].ToString();
    }
}