using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    /// <summary>
    /// 实现分片上传和断点下载的webclient(同步)
    /// </summary>
    public class WebClientV2 : WebClient
    {
        /// <summary>
        /// 分块上传长度
        /// </summary>
        int _blockLlength = 100;
        public bool Upload_Block(string url, string filePath, object state)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            int totalLength = fileData.Length;
            for (int startpos = 0; startpos < totalLength; )
            {
                byte[] blockData = null;
                if (startpos + _blockLlength > totalLength)
                {
                    blockData = fileData.Skip(startpos).Take(totalLength - startpos).ToArray();
                }
                else
                {
                    blockData = fileData.Skip(startpos).Take(_blockLlength).ToArray();
                }

                try
                {
                    //***bytes 21010-47021/47022
                    this.Headers.Remove(HttpRequestHeader.ContentRange);
                    this.Headers.Add(HttpRequestHeader.ContentRange, "bytes " + startpos + "-" + (startpos + blockData.Length) + "/" + totalLength);

                    this.UploadData(url, "POST", blockData);
                }
                catch (Exception ex)
                {
                    // sMsg = ex.ToString();
                    return false;
                }
            }
            return true;
        }

        public event Action<object, ErrorEventArgs> OnError;

        public WebClientV2()
        {
        }

        void WebClientV2_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var fileMsgInfo = e.UserState as dynamic;
            if (e.Error != null)
            {
                //传递到异常事件中去
                OnError(sender, new ErrorEventArgs() { ExceptionInfo = e.Error, MsgId = fileMsgInfo.MessageId, ProcessType = fileMsgInfo.ProcessType });
            }
            else
            {
                using (FileStream sw = new FileStream(fileMsgInfo.FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    sw.Write(e.Result, 0, e.Result.Length);
                }
            }
        }
        public class DownloadState
        {
            public string FilePath { get; set; }

        }
        void WebClientV2_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 断点下载，需要服务端实现Custom_Range
        /// </summary>
        /// <param name="url"></param>
        /// <param name="saveFilePath"></param>
        /// <param name="state"></param>
        /// <param name="onDownload"></param>
        public void DownLoad_BreakPoint(string url, string saveFilePath, object state, Action<string, long, long> onDownload)
        {
            long currentLength = 0;
            if (File.Exists(saveFilePath))
            {
                var fileInfo = new FileInfo(saveFilePath);
                currentLength = fileInfo.Length;
            }
            //long lStartPos = 0;
            long startPosition = 0;
            long endPosition = 0;
            long totalLength = 0;

                    this.Headers.Add("CustomRange", string.Format("bytes={0}-", currentLength));

                    //this.Headers.Add("Custom_Range",string.Format("bytes={0}-", currentLength));
                    this.DownloadDataAsync(new Uri(url),state);

            using (FileStream sw = new FileStream(saveFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                do
                {

                    string contentRange = this.ResponseHeaders["Content-Range"];

                    //bytes 10000-19999/1157632
                    if (!string.IsNullOrEmpty(contentRange))
                    {
                        contentRange = contentRange.Replace("bytes", "").Trim();
                        contentRange = contentRange.Substring(0, contentRange.IndexOf("/"));
                        totalLength = long.Parse(contentRange.Substring(contentRange.IndexOf("/") + 1));
                        string[] ranges = contentRange.Split('-');
                        startPosition = long.Parse(ranges[0]);
                        endPosition = long.Parse(ranges[1]);
                    }
                    else
                    {
                        startPosition = 0;
                        //totalLength = currentData.Length;
                        endPosition = totalLength;
                    }
                    if (currentLength < startPosition)
                    {
                        throw new Exception("Server Error");
                    }
                    
                    sw.Seek(startPosition, SeekOrigin.Current);
                   // sw.Write(currentData, 0, currentData.Length);
                    currentLength = endPosition;

                    onDownload(state.ToString(), currentLength, totalLength);
                   

                } while (endPosition < totalLength);
            }
        }


    }
}
