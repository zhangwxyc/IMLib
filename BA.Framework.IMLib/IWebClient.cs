using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib
{
    public interface IWebClient
    {
        event Action<string, long, long> OnUpload;
        event Action<string, long, long> OnDownload;
        event Action<object, ErrorEventArgs> OnError;

        void Download(string url, string filePath, string msgId);
        void Upload(string url, string filePath, string msgId);

        void Cancel();

    }
}
