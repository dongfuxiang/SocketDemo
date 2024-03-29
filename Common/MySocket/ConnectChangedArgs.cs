using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Common.MySocket
{
    public class ConnectChangedArgs : EventArgs
    {
        public string Info { get; set; }
        public string Msg { get; set; }
        public bool IsConnection { get; set; }
    }
}
