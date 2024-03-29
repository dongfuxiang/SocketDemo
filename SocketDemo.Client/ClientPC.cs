using Common.MySocket;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using static Common.Enum;

namespace SocketDemo.Client
{
    class ClientPC
    {
        private SocketClient client = null;
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            try
            {
                client = new SocketClient("127.0.0.1", 7001);
                client.DataReceived += Client_DataReceived1;
                client.ConnentChanged += Client_ConnentChanged;

                Task.Factory.StartNew(() => { Connect(); });
            }
            catch (Exception ex)
            {

            }
        }



        public void Connect()
        {
            try
            {
                while (client != null)
                {
                    if (!client.IsConnected)
                    {
                        client.Connect();
                    }
                    Thread.Sleep(500);
                }
            }
            catch (Exception e)
            {


            }
        }
        /// <summary>
        /// 接收服务端消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_DataReceived1(object sender, ReciveEventArgs e)
        {
            if ((int)e.MsgType == 0)
            {
                string data = Encoding.Default.GetString(e.Data);
                MainWindow.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow.Instance.textReciveMsg.Items.Add($"{DateTime.Now} 服务端{client.SocketSend.RemoteEndPoint} [{e.MsgType}]->{data}");
                }));
            }
            else if ((int)e.MsgType == 1)
            {
                ParseFileByte(e, out string filename);
                MainWindow.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow.Instance.textReciveMsg.Items.Add($"{DateTime.Now} 服务端{client.SocketSend.RemoteEndPoint} [{e.MsgType}]->{filename}");
                }));

            }
        }
        /// <summary>
        /// 连接状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_ConnentChanged(object sender, ConnectChangedArgs e)
        {
            if (e.IsConnection)
            {
                MainWindow.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow.Instance.textReciveMsg.Items.Add($"{DateTime.Now}::服务端{e.Info}->{e.Msg}");
                    MainWindow.Instance.rectServer.Fill = Brushes.Green;
                    MainWindow.Instance.LocalIpPort.Content = client.LocalEndPoint.ToString();
                }));
            }
            else
            {
                MainWindow.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow.Instance.textReciveMsg.Items.Add($"{DateTime.Now}::服务端{e.Info}->{e.Msg}");
                    MainWindow.Instance.rectServer.Fill = Brushes.Red;

                }));
            }
        }
        /// <summary>
        /// 发送消息到服务户端
        /// </summary>
        /// <param name="str"></param>
        public void SendToServer(MsgType type, object obj)
        {
            byte[] buffer = new byte[0];
            if (obj.GetType().FullName.Contains("String"))
            {
                buffer = Encoding.Default.GetBytes(obj.ToString());
                client.Send(0, (int)type, buffer);
            }
            else if (obj.GetType().FullName.Contains("Byte"))
            {
                buffer = (byte[])obj;
                client.Send(0, (int)type, buffer);
            }

        }

        /// <summary>
        /// 解析收到的文件数据
        /// </summary>
        /// <param name="e"></param>
        /// <param name="filename">文件名</param>
        /// <param name="fileExt">文件后缀</param>
        /// <returns></returns>
        private void ParseFileByte(ReciveEventArgs e, out string filename)
        {
            //获取程序路径
            string rootFilePath = System.Environment.CurrentDirectory;
            //"#"的ASCALL值为35
            //1.遍历Byte[]，找到分隔符
            int split = Array.IndexOf(e.Data, (byte)35);
            //2.获取文件名
            byte[] fileNameB = new byte[split];
            Array.Copy(e.Data, 0, fileNameB, 0, split);
            string fileName = Encoding.Default.GetString(fileNameB);
            filename = fileName;
            //3.获取文件
            byte[] fileData = new byte[e.Data.Length - split - 1];
            Array.Copy(e.Data, split + 1, fileData, 0, e.Data.Length - split - 1);

            //5.创建接收文件存储的目录
            string newPath = System.IO.Path.Combine(rootFilePath, "RecFileDir");
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            using (FileStream fs = new FileStream(newPath + $@"\{fileName}", FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(fileData);
                    bw.Close();
                }
            }

        }
        public void Uinit()
        {
            client?.Dispose();
            client = null;
        }

    }
}
