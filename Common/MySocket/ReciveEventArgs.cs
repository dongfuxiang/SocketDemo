using System;
using System.Collections.Generic;
using System.Text;
using static Common.Enum;

namespace Common.MySocket
{
    /// <summary>
    /// 接收数据事件
    /// </summary>
    public class ReciveEventArgs : EventArgs
    {
        public MsgType MsgType { get; set; }
        public string RemoteIp { get; set; }
        public int Index { get; set; }
        public byte[] Data { get; set; }
    }
}
