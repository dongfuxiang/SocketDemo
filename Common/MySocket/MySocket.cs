using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Common.Enum;

namespace Common.MySocket
{

    public abstract class MySocket : IDisposable
    {


        public abstract bool IsConnected { get; set; }
        public abstract event EventHandler<ReciveEventArgs> DataReceived;

        public abstract string Ip { get; set; }
        public abstract int Point { get; set; }
        protected abstract void Receive(object o);
        public abstract void Send(int index, int msgtype, object data);
        public abstract void Dispose();
    }

    public class SocketSrever : MySocket
    {
        private bool IsListening = true;
        private bool IsReceiving = true;
        public int ClientCount { get; set; }
        //是否连接
        public override bool IsConnected { get; set; } = false;
        //接收数据的事件
        public override event EventHandler<ReciveEventArgs> DataReceived;
        public event EventHandler<ConnectChangedArgs> ConnentChanged;
        public Socket SocketWatch { get; set; }
        //本地Ip
        public override string Ip { get; set; }
        //本地端口
        public override int Point { get; set; }
        //存储远程连接的IP地址与Socket对象
        public List<Socket> Sockets { get; set; }

        public SocketSrever(int point, string ip = null)
        {
            Sockets = new List<Socket>();
            //1.创建一个负责监听的Socket，监听自己的ip和端口号
            SocketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //2.让负责监听的Socket绑定ip和端口号，本地创建 IPEndPoint 对象，拥有此 ip:post 的访问权限。目的是绑定本地机器的某个端口，所有经过此端口的数据就归你管了。
            Ip = ip;
            Point = point;
            IPAddress ipAddress;
            if (string.IsNullOrEmpty(ip))
            {
                ipAddress = IPAddress.Any;
            }
            else
            {
                ipAddress = IPAddress.Parse(ip);
            }
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, point);
            SocketWatch.Bind(iPEndPoint);
            //3.设置监听队列，在某一时间内可以连入服务器的最大客户端数量
            SocketWatch.Listen(ClientCount);

            Thread thread = new Thread(Listen);
            thread.IsBackground = false;
            thread.Start(SocketWatch);

        }

        /// <summary>
        /// 等待客户端连接，并创建与之通信的Socket，被线程执行的函数一定是object
        /// </summary>
        /// <param name="o"></param>
        private void Listen(object o)
        {
            Socket socketWatch = o as Socket;
            while (IsListening)
            {
                try
                {
                    //4.负责监听的Socket，来接收客户端的连接（接收连接后必须创建一个负责通信的Socket）
                    //负责监听的Socket
                    //Accept函数会在当前位置停止等待客户端接入
                    Socket SocketSend = socketWatch.Accept();
                    Sockets.Add(SocketSend);
                    IsConnected = true;
                    ConnectChangedArgs args = new ConnectChangedArgs();
                    args.Info = SocketSend.RemoteEndPoint.ToString();
                    args.IsConnection = IsConnected;
                    args.Msg = "连接成功！";
                    ConnentChanged?.Invoke(this, args);

                    //开启一个线程，不断监听客户端发来的消息
                    Thread thread2 = new Thread(Receive);
                    thread2.Start(SocketSend);

                }
                catch (Exception e)
                {
                    //MessageBox.Show($"监听异常 {e}");

                }
            }

        }
        /// <summary>
        /// 服务器不断接收客户端发来的消息
        /// </summary>
        /// <param name="o"></param>
        protected override void Receive(object o)
        {
            Socket socketSend = o as Socket;
            bool flag = true;
            while (IsReceiving)
            {
                try
                {
                    if (!socketSend.Connected)
                    {
                        flag = false;
                    }
                    if (!flag)
                    {
                        IsConnected = false;
                        Sockets.RemoveAll(p => p.RemoteEndPoint == socketSend.RemoteEndPoint);
                        ConnectChangedArgs args = new ConnectChangedArgs();
                        args.Info = socketSend.RemoteEndPoint.ToString();
                        args.IsConnection = IsConnected;
                        args.Msg = "连接断开";
                        ConnentChanged?.Invoke(this, args);
                        return;
                    }
                    //5.客户端连接成功后，服务端应该接收客户端发来的消息
                    //Recive接收的是byte数组，这里设置接收的字节数
                    //文件或消息过大时一次性接收不下，会多次收到消息
                    byte[] buffer = new byte[1024 * 1024 * 20];
                    //Recice返回int类型，代表实际接收到的有效字节数
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        flag = false;
                        continue;
                    }


                    for (int i = 0; i < Sockets.Count; i++)
                    {

                        if (Sockets[i].RemoteEndPoint.ToString() == socketSend.RemoteEndPoint.ToString())
                        {
                            ProcessRec(buffer, out byte[] data, out int mastTpye);
                            ReciveEventArgs reciveEventArgs = new ReciveEventArgs();
                            reciveEventArgs.Data = data;
                            reciveEventArgs.MsgType = (MsgType)mastTpye;
                            reciveEventArgs.RemoteIp = socketSend.RemoteEndPoint.ToString();
                            reciveEventArgs.Index = i;
                            DataReceived?.Invoke(this, reciveEventArgs);

                        }
                    }

                }
                catch (Exception)
                {


                }
            }
        }
        /// <summary>
        /// 消息解析
        /// </summary>
        /// <param name="buffer"></param>
        private void ProcessRec(byte[] buffer, out byte[] data, out int mastTpye)
        {
            mastTpye = 0;
            data = null;
            try
            {
                int i = Array.IndexOf(buffer, (byte)35);
                byte[] typebyte = new byte[i];
                Array.Copy(buffer, 0, typebyte, 0, i);
                byte[] databyte = new byte[buffer.Length - typebyte.Length - 1];
                Array.Copy(buffer, typebyte.Length + 1, databyte, 0, buffer.Length - typebyte.Length - 1);
                data = databyte;
                mastTpye = Convert.ToInt32(Encoding.Default.GetString(typebyte));
            }
            catch (Exception e)
            {
                string s = e.Message;

            }
        }

        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="index">客户端编号</param>
        /// <param name="msgtype">消息类型</param>
        /// <param name="data">数据</param>
        public override void Send(int index, int msgtype, object data)
        {
            string split = "#";
            byte[] splitbyte = Encoding.Default.GetBytes(split);
            byte[] typebyte = Encoding.Default.GetBytes(msgtype.ToString());
            byte[] buffer = data as byte[];

            List<byte> bufferlist = typebyte.ToList();
            bufferlist.AddRange(splitbyte.ToList());
            bufferlist.AddRange(buffer.ToList());

            buffer = bufferlist.ToArray();

            Sockets[index]?.Send(buffer);
        }


        public override void Dispose()
        {
            IsListening = false;
            IsReceiving = false;
            SocketWatch?.Close();
            SocketWatch = null;

            foreach (var item in Sockets)
            {
                item.Dispose();
            }

        }
    }

    public class SocketClient : MySocket
    {
        private bool _isReceiving = true;
        //是否连接
        public override bool IsConnected { get; set; } = false;
        //接收数据的事件
        public override event EventHandler<ReciveEventArgs> DataReceived;
        //连接状态改变事件
        public event EventHandler<ConnectChangedArgs> ConnentChanged;
        private ManualResetEvent mreTimeOut = new ManualResetEvent(false);
        //负责通信的socket
        public Socket SocketSend { get; set; }
        //远程Ip
        public override string Ip { get; set; }
        //远程端口
        public override int Point { get; set; }

        public EndPoint LocalEndPoint { get; set; }
        private IPAddress ipAddress { get; set; }
        IPEndPoint iPEndPoint { get; set; }
        public SocketClient(string ip, int point)
        {

            Ip = ip;
            Point = point;


        }

        public void Connect()
        {
            SocketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ipAddress = IPAddress.Parse(Ip);
            iPEndPoint = new IPEndPoint(ipAddress, Point);
            mreTimeOut.Reset();
            //获得远程连接服务端的IP地址和端口号
            SocketSend.BeginConnect(iPEndPoint, ConnectCallback, null);
            mreTimeOut.WaitOne(3000);
            if (IsConnected)
            {
                ConnectChangedArgs changedArgs = new ConnectChangedArgs();
                changedArgs.IsConnection = true;
                changedArgs.Info = SocketSend.RemoteEndPoint.ToString();
                changedArgs.Msg = "连接成功";
                ConnentChanged?.Invoke(this, changedArgs);
                LocalEndPoint = SocketSend.LocalEndPoint;
                //开启一个线程，不断接收服务端发来的消息
                Thread thread = new Thread(Receive);
                thread.Start();
            }

        }

        /// <summary>
        /// 异步连接回调方法
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                IsConnected = false;
                if (SocketSend != null)
                {
                    SocketSend.EndConnect(ar);
                    IsConnected = SocketSend.Connected;
                }
            }
            catch (Exception)
            {
                IsConnected = false;
            }
            finally
            {
                mreTimeOut.Set();
            }
        }
        protected override void Receive(object o)
        {
            bool flag = true;
            while (_isReceiving)
            {
                try
                {
                    if (!SocketSend.Connected)
                    {
                        flag = false;
                    }
                    if (!flag)
                    {
                        IsConnected = false;
                        ConnectChangedArgs args = new ConnectChangedArgs();
                        args.Info = SocketSend.RemoteEndPoint.ToString();
                        args.IsConnection = IsConnected;
                        args.Msg = "连接断开";
                        ConnentChanged?.Invoke(this, args);
                        return;
                    }

                    byte[] buffer = new byte[1024 * 1024 * 20];
                    int r = SocketSend.Receive(buffer);
                    if (r == 0)
                    {
                        flag = false;
                        continue;
                    }

                    ProcessRec(buffer, out byte[] data, out int mastTpye);
                    ReciveEventArgs reciveEventArgs = new ReciveEventArgs();
                    reciveEventArgs.MsgType = (MsgType)mastTpye;
                    reciveEventArgs.Data = data;
                    DataReceived?.Invoke(this, reciveEventArgs);


                }
                catch (Exception e)
                {


                }
            }
        }

        /// <summary>
        /// 消息解析
        /// </summary>
        /// <param name="buffer"></param>
        private void ProcessRec(byte[] buffer, out byte[] data, out int mastTpye)
        {
            mastTpye = 0;
            data = null;
            try
            {
                int i = Array.IndexOf(buffer, (byte)35);
                byte[] typebyte = new byte[i];
                Array.Copy(buffer, 0, typebyte, 0, i);
                byte[] databyte = new byte[buffer.Length - typebyte.Length - 1];
                Array.Copy(buffer, typebyte.Length + 1, databyte, 0, buffer.Length - typebyte.Length - 1);
                data = databyte;
                mastTpye = Convert.ToInt32(Encoding.Default.GetString(typebyte));
            }
            catch (Exception e)
            {
                string s = e.Message;

            }
        }

        public override void Send(int index, int msgtype, object data)
        {
            string split = "#";
            byte[] splitbyte = Encoding.Default.GetBytes(split);
            byte[] typebyte = Encoding.Default.GetBytes(msgtype.ToString());
            byte[] buffer = data as byte[];

            List<byte> bufferlist = typebyte.ToList();
            bufferlist.AddRange(splitbyte.ToList());
            bufferlist.AddRange(buffer.ToList());

            buffer = bufferlist.ToArray();

            SocketSend.Send(buffer);
        }

        public override void Dispose()
        {
            IsConnected = false;
            _isReceiving = false;
            SocketSend?.Dispose();
            SocketSend = null;
        }
    }

}
