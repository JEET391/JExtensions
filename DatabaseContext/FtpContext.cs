using JExtensions.Constants;
using JExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace JExtensions.DatabaseContext
{
    //Initialise FTP Server
    //FTP ftpClient = new FTP("ftp://103.233.76.81:22", "workday_it", ""wM9gMoQoTPry");
    public class FtpContext
    {
        /// <summary>
        /// Initialise FTP Server
        /// </summary>
        /// <param name="ftpUrl">ftp://103.233.76.81:22</param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        public FtpContext(string ftpUrl, string userId, string password)
        {
            Password = password;
            Url = ftpUrl;
            UserId = userId;
        }

        private string Password { get; }
        private string Url { get; }
        private string UserId { get; }
        /* Download File */

        public void Download(string remoteFile, string localFile)
        {
            if (string.IsNullOrEmpty(remoteFile))
            {
                throw new ArgumentException($"'{nameof(remoteFile)}' cannot be null or empty.", nameof(remoteFile));
            }

            if (string.IsNullOrEmpty(localFile))
            {
                throw new ArgumentException($"'{nameof(localFile)}' cannot be null or empty.", nameof(localFile));
            }

            try
            {
                /* Create an FTP Request */
                var ftpRequest = (FtpWebRequest)WebRequest.Create(remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(UserId, Password);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                /* Establish Return Communication with the FTP Server */
                var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Get the FTP Server's Response Stream */
                var ftpStream = ftpResponse.GetResponseStream();
                /* Open a File Stream to Write the Downloaded File */
                var localFileStream = new FileStream(localFile, FileMode.Create);
                /* Buffer for the Downloaded Data */
                var byteBuffer = new byte[2048];
                int bytesRead = ftpStream.Read(byteBuffer, 0, 2048);
                /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, 2048);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Retrun the DirectoryList
        /// </summary>
        /// <param name="directory">FTP DirectoryName</param>
        /// <param name="fileExtension">.gz</param>
        /// <returns></returns>
        public List<FileInfo> GetDirectoryInformation(string directory, string fileExtension, string fileNameStartsWith)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Url + "/" + directory);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(UserId, Password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            string[] list = null;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            var fileLogList = new List<FileInfo>();
            foreach (string line in list)
            {
                // Create directory info
                var detail = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (detail.Length <= 5)
                {
                    continue;
                }
                string fileName = detail[8];
                var fileInfo = new FileInfo(fileName);
                if (fileInfo.Extension != fileExtension || fileInfo.Name.Left(fileNameStartsWith.Length) != fileNameStartsWith)
                {
                    continue;
                }
                fileLogList.Add(fileInfo);
            }
            return fileLogList;
        }

        public List<FileInfo> GetDirectoryInformation(string directory, string[] fileExtensions, string fileNameStartsWith)
        {
            List<List<FileInfo>> fileLogLists = new List<List<FileInfo>>();
            List<FileInfo> returnFileLogList = new List<FileInfo>();
            foreach (string extension in fileExtensions)
                fileLogLists.Add(GetDirectoryInformation(directory, extension, fileNameStartsWith));
            foreach (List<FileInfo> fileLogList in fileLogLists)
                foreach (FileInfo fileLog in fileLogList)
                    returnFileLogList.Add(fileLog);
            return returnFileLogList;
        }

        public void Upload(string remoteFile, string localFile)
        {
            if (string.IsNullOrEmpty(remoteFile))
            {
                throw new ArgumentException($"'{nameof(remoteFile)}' cannot be null or empty.", nameof(remoteFile));
            }

            if (string.IsNullOrEmpty(localFile))
            {
                throw new ArgumentException($"'{nameof(localFile)}' cannot be null or empty.", nameof(localFile));
            }

            try
            {
                /* Create an FTP Request */
                var ftpRequest = (FtpWebRequest)WebRequest.Create(Url + "/" + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                ftpRequest.Credentials = new NetworkCredential(UserId, Password);
                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                /* Establish Return Communication with the FTP Server */
                var ftpStream = ftpRequest.GetRequestStream();
                /* Open a File Stream to Read the File for Upload */
                FileStream localFileStream = new FileStream(localFile, FileMode.Create);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[2048];
                int bytesSent = localFileStream.Read(byteBuffer, 0, 2048);
                /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesSent != 0)
                    {
                        ftpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, 2048);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                /* Resource Cleanup */
                localFileStream.Close();
                ftpStream.Close();
                ftpRequest = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}