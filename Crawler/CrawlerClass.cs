using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    class CrawlerClass
    {
        public static string Crawl(string url,
            out HttpStatusCode statusCode,
            out Dictionary<string, string> header,
            bool noEncoding = true)
        {
            string content = string.Empty;
            var responseUrl = string.Empty;
            statusCode = HttpStatusCode.SeeOther;

            var request = GetRequest(url);

            int retry = 0;
            int redirect = 0;
            Exception e = null;
            bool? result = null;
            string redirectUrl = string.Empty;
            header = null;

            while (retry < Configuration.MaxRetryWhenCrawl && redirect < Configuration.MaxRedirectCount)
            {
                try
                {
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        result = CheckResponse(response, out statusCode, out content, out redirectUrl,
                            out responseUrl,
                            out header);
                    }
                }
                catch (WebException ex)
                {
                    e = ex;
                    using (var response = (HttpWebResponse)ex.Response)
                    {
                        result = CheckResponse(response, out statusCode, out content, out redirectUrl,
                            out responseUrl,
                            out header);
                    }
                }
                catch (Exception ex)
                {
                    e = ex;
                }

                if (result == true)
                {
                    return content;
                }
                else if (result == false)
                {
                    request = GetRequest(redirectUrl);
                    ++redirect;
                    continue;
                }
                else if (statusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception("Bad request error hit");
                }

                ++retry;
            }

            if (e != null)
            {
                throw e;
            }
            else
            {
                throw new Exception("Max retry/redirect hit");
            }
        }

        private static HttpWebRequest GetRequest(string url)
        {
#pragma warning disable 612, 618
            // Disable encode of Uri class which has issue for some charactors
            // Make sure encode the url before this
            var uri = new Uri(url, true);
            var request = WebRequest.CreateHttp(uri);
            request.Timeout = Configuration.CrawlTimeout;
            //request.AllowAutoRedirect = followRedirect;
            request.AllowAutoRedirect = true;
            //request.Proxy = null;
            request.Method = "GET";
            request.UserAgent = Configuration.UserAgent;

            return request;
        }


        /*
        * return
        *      true: to return
        *      null: to retry
        *      false: follow redirect
        */
        private static bool? CheckResponse(HttpWebResponse response, out HttpStatusCode statusCode, out string content,
            out string redirectUrl, out string responseUrl, out Dictionary<string, string> header)
        {
            content = string.Empty;
            redirectUrl = string.Empty;
            responseUrl = string.Empty;
            statusCode = HttpStatusCode.SeeOther;
            header = new Dictionary<string, string>();

            if (response != null)
            {
                responseUrl = response.ResponseUri.AbsoluteUri;

                // TODO, get header before redirect
                for (int i = 0; i < response.Headers.Count; ++i)
                {
                    var key = response.Headers.Keys[i];
                    var value = response.Headers[i];
                    if (!header.ContainsKey(key))
                    {
                        header.Add(key, value);
                    }
                }

                statusCode = response.StatusCode;
                if (statusCode == HttpStatusCode.OK)
                {
                    // Get last modified timestamp
                    //DateTime lastModifiedValue;
                    //if (!string.IsNullOrWhiteSpace(response.Headers["Last-Modified"]) &&
                    //    DateTime.TryParse(response.Headers["Last-Modified"], out lastModifiedValue))
                    //{
                    //    lastModified = lastModifiedValue.ToUniversalTime();
                    //}

                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            var readStream = new StreamReader(responseStream, Encoding.UTF8);
                            content = readStream.ReadToEnd();
                            return true;
                        }
                    }
                }
                else if (statusCode == HttpStatusCode.Moved || statusCode == HttpStatusCode.MovedPermanently ||
                         statusCode == HttpStatusCode.Redirect || statusCode == HttpStatusCode.TemporaryRedirect)
                {
                    redirectUrl = response.Headers["Location"];
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        return false;
                    }
                }
                else if (statusCode == HttpStatusCode.NotFound)
                {
                    // Status code which doesn't need retry
                    return true;
                }
            }

            return null;
        }

        public static string HttpDownloadFile(string url, string path)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            string fileName = response.Headers["Content-Disposition"];//attachment;filename=FileName.txt
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = response.ResponseUri.Segments[response.ResponseUri.Segments.Length - 1];
            }
            else
            {
                fileName = fileName.Remove(0, fileName.IndexOf("filename=") + 9);
            }
            string filePath = Path.Combine(path, fileName);
            int index = 1;
            while (File.Exists(filePath))
            {
                if (index == 1)
                {
                    fileName = fileName.Replace(".zip", (++index) + ".zip");
                }
                else
                {
                    fileName = fileName.Replace(index + ".zip", (++index) + ".zip");
                }
                filePath = Path.Combine(path, fileName);

            }
            using (Stream responseStream = response.GetResponseStream())
            {
                long totalLength = response.ContentLength;
                //using (Stream stream = new FileStream(filePath, overwrite ? FileMode.Create : FileMode.CreateNew))
                using (Stream stream = new FileStream(filePath, FileMode.CreateNew))
                {
                    byte[] bArr = new byte[1024];
                    int size;
                    while ((size = responseStream.Read(bArr, 0, bArr.Length)) > 0)
                    {
                        stream.Write(bArr, 0, size);
                    }
                }
            }
            return fileName;
        }

    }
}
