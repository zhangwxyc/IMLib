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
    /// 实现分片上传和断点下载的webclient
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
            using (StreamWriter sw = new StreamWriter(saveFilePath, true))
            {
                do
                {
                    this.Headers["Range"] = string.Format("bytes={0}-", currentLength);
                    byte[] currentData = this.DownloadData(url);
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
                    if (currentLength < startPosition)
                    {
                        throw new Exception("Server Error");
                    }
                    sw.BaseStream.Seek(startPosition, SeekOrigin.Current);
                    sw.Write(currentData);
                    //var args = new DownloadProgressChangedEventArgs();
                    //args.BytesReceived = currentLength;
                    onDownload(state.ToString(), currentLength, totalLength);
                    //OnDownloadProgressChanged(new DownloadProgressChangedEventArgs() {      });
                    currentLength = endPosition;
                } while (endPosition < totalLength);
            }
        }


    }
}
