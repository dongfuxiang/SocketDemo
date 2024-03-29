using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SocketDemo.Srever
{
    /// <summary>
    /// 将被序列化为Json的对象
    /// </summary>
    /// 
    //[DataContract]指定该类型要定义或实现一个数据协定，并可由序列化程序（如System.Runtime.Serialization.DataContractSerializer）进行序列化。
    //[DataMember]当应用于类型的成员时，指定该成员是数据协定的一部分并可由System.Runtime.Serialization.DataContractSerializer进行序列化。

    [Serializable]
    [DataContract]
    public class SerializeData
    {
        [DataMember]
        public string Content1 { get; set; }
        [DataMember]
        public string Content2 { get; set; }
        [DataMember]
        public string Content3 { get; set; }
        [DataMember]
        public string Content4 { get; set; }
    }
}
