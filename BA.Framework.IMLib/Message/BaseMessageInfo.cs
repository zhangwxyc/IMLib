using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BA.Framework.IMLib.Message
{
    /// <summary>
    /// 基础消息
    /// </summary>
    public class BaseMessageInfo
    {
        //[JsonConverter(typeof(StringEnumConverter))]
        /// <summary>
        /// 消息类型
        /// </summary>
        [JsonConverter(typeof(CustomEnumJsonConverter))]
        [JsonProperty("type")]
        public MessageType MsgType { get; set; }
        /// <summary>
        /// 消息ID
        /// </summary>
        [JsonProperty("msg_id")]
        public string MessageId { get; set; }

        private dynamic _data;
        /// <summary>
        /// 消息数据内容
        /// </summary>
        [JsonProperty("data")]
        public dynamic Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
                if (_data != null)
                {
                    DataDict.Clear();
                    Type dType = _data.GetType();
                    if (dType.IsClass && dType.Name != "String")
                    {
                        if (dType.Name == "JObject")
                        {
                            foreach (Newtonsoft.Json.Linq.JProperty jproperty in ((Newtonsoft.Json.Linq.JObject)_data).Properties())
                            {
                                DataDict.Add(jproperty.Name, jproperty.Value);
                            }
                        }

                        else
                        {
                            var propArray = dType.GetProperties();
                            foreach (var propItem in propArray)
                            {
                                DataDict.Add(propItem.Name, propItem.GetValue(_data));
                            }
                        }
                    }
                    else
                        DataDict.Add("data", _data);
                }
            }
        }

        private Dictionary<string, object> _dataDict;

        [JsonIgnore]
        public Dictionary<string, object> DataDict
        {
            get { return _dataDict; }
            set { _dataDict = value; }
        }

        public BaseMessageInfo()
        {
            MessageId = Guid.NewGuid().ToString();
            DataDict = new Dictionary<string, object>();
        }
        /// <summary>
        /// 文件类消息包含此属性
        /// </summary>
        [JsonIgnore]
        public FileMessageInfo RelateFileInfo { get; set; }
    }
}
