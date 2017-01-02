using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Utility.Library.Misc
{
    public class WebClient
    {

        public bool Download(Uri source, string target, bool permitOverwrite)
        {
            //1. check target
            if (File.Exists(target))
            {
                 if(!permitOverwrite)
                    throw (new ApplicationException("Target file already exists"));

                if(!FileSystem.DeleteFile(target))
                    return false;
            }
            
            try
            {
                WebRequest request = WebRequest.Create(source);
                WebResponse response = request.GetResponse();
                string filename = "";
                int contentLength = 0;

                for (int a = 0; a < response.Headers.Count; a++)
                {
                    try
                    {
                        string val = response.Headers.Get(a);

                        switch (response.Headers.GetKey(a).ToLower())
                        {
                            case "content-length":
                                contentLength = Convert.ToInt32(val);
                                break;
                            case "content-disposition":
                                string[] v2 = val.Split(';');
                                foreach (string s2 in v2)
                                {
                                    string[] v3 = s2.Split('=');
                                    if (v3.Length == 2)
                                    {
                                        if (v3[0].Trim().ToLower() == "filename")
                                        {
                                            char[] sss = { ' ', '"' };
                                            filename = v3[1].Trim(sss);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    catch (Exception) { };
                }

                if (string.IsNullOrEmpty(filename))
                    return false;
                
                using (Stream stream = response.GetResponseStream())
                {
                    int pos = 0;
                    byte[] buf2 = new byte[8192];
                    using (FileStream fs = new FileStream(target, FileMode.CreateNew))
                    {
                        while ((0 == contentLength) || (pos < contentLength))
                        {
                            int maxBytes = 8192;
                            if ((0 != contentLength) && ((pos + maxBytes) > contentLength)) 
                                maxBytes = contentLength - pos;
                            int bytesRead = stream.Read(buf2, 0, maxBytes);
                            if (bytesRead <= 0) 
                                break;
                            fs.Write(buf2, 0, bytesRead);

                            pos += bytesRead;
                        }
                    }
                }
            }
            catch
            {
                // when something goes wrong - at least do the cleanup :)
                if (target.Length > 0)
                {
                   FileSystem.DeleteFolder(target);
                   return false;
                }
            }
            return true;
        }
    }
}
