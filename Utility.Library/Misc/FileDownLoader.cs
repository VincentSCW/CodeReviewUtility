using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;

namespace Utility.Library.Misc
{

    /// <summary>
    /// File Downloader class that will download the files from a host to the target directory.
    /// The Host can be a web site, a ftp server or a network or local location
    /// </summary>
    public class FileDownLoader
    {
        public static void DownLoad(string credentials, string targetDirectory, bool checkIfNewer, params Uri[] files)
        {
            DownLoadInternal(credentials, targetDirectory, checkIfNewer, (f) => { }, files);
        }

        public static void DownLoadAsync(string credentials, string targetDirectory, bool checkIfNewer, Action<string> completed, params Uri[] files)
        {
            Task.Factory.StartNew(() =>
            {
                DownLoadInternal(credentials, targetDirectory, checkIfNewer, completed, files);
            });
        }

        private static void DownLoadInternal(string credentials, string targetDirectory, bool checkIfNewer, Action<string> completed, params Uri[] files)
        {
            try
            {
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                    System.Threading.Tasks.Parallel.ForEach(files, (uri) =>
                    {
                        string file = Path.GetFileName(uri.ToString());
                        string target = Path.Combine(targetDirectory, file.Trim());

                        //if the Host is blank or NULL, then no download server has been specified, so return
                        //true if a local file exists, and we were told to checkNewer, or false if we are not to
                        //checkNewer, or the local file does not exist.
                            DateTime remoteTime;
                            if (checkIfNewer && DownLoadRequired(uri, target, credentials, out remoteTime))
                            {
                                Trace.WriteLine("File DownLoad Required " + file);

                                var client = new System.Net.WebClient();
                                if (!string.IsNullOrEmpty(credentials) && credentials.Contains("\\"))
                                {
                                    var parts = credentials.Split('\\');
                                    client.Credentials = new NetworkCredential(parts[0], parts[1]);
                                }
                                //  we are going to try a download.
                                client.DownloadFile(uri, target);
                                // Update the target to match the orginal time stamp
                                FileInfo info = new FileInfo(target);
                                info.LastWriteTime = remoteTime;
                            }

                            completed(file);
                    });
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            completed(null);
        }

        private static bool DownLoadRequired(Uri source, string target, string credentials, out DateTime remoteTime)
        {
            remoteTime = DateTime.Now;
            if (File.Exists(target))
            {
                //create the WebRequest.  
                WebRequest request = WebRequest.Create(source);
                if (!string.IsNullOrEmpty(credentials) && credentials.Contains("\\"))
                {
                    var parts = credentials.Split('\\');
                    request.Credentials = new NetworkCredential(parts[0], parts[1]);
                }

                if (source.HostNameType == UriHostNameType.Dns) // if it is a HttpWebRequest, then we send it off to get the details of the file 
                {
                    using (var response = request.GetResponse() as HttpWebResponse)
                    {
                        //and pick off the last modified date/time.
                        remoteTime = response.LastModified;
                    }
                }
                else if (source.HostNameType == UriHostNameType.IPv4 || source.HostNameType == UriHostNameType.IPv6)
                {
                    request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    // it is a File/FtpWebRequest, simply go get the date/time of the remote file.
                    using (var response = request.GetResponse() as FtpWebResponse)
                    {
                        remoteTime = response.LastModified;
                    }
                }
                else
                {
                    // it is a generic WebRequest, simply go get the date/time of the remote file.
                    using (var response = request.GetResponse() as WebResponse)
                    {
                        var fi = new FileInfo(response.ResponseUri.LocalPath);
                        remoteTime = fi.LastWriteTime;
                    }
                }

                FileInfo info = new FileInfo(target);
                return !DateTime.Equals(info.LastWriteTime, remoteTime);
            }

            return true;
        }
    }
}
