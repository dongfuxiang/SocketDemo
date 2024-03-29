using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Common.Controls;
using Common.MySocket;
using static Common.Enum;

namespace SocketDemo.Sever
{
    /// <summary>
    /// 服务端PC
    /// </summary>
    class ServerPC
    {
        private static SocketSrever server = null;
        public void Init()
        {

            server = new SocketSrever(7001, "127.0.0.1");
            server.ClientCount = 10;
            server.DataReceived += Server_DataReceived1;
            server.ConnentChanged += Server_ConnentChanged;


        }
        private void Server_ConnentChanged(object sender, ConnectChangedArgs e)
        {
            MainWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow.Instance.ClientStackPanel.Children.Clear();
                //消息框中打印连接信息
                MainWindow.Instance.textReciveMsg.Items.Add($"{DateTime.Now}::客户端[{e.Info}]->{e.Msg}");
                //更新记录客户端的combox
                MainWindow.Instance.clientComBox.ItemsSource = server.Sockets.Select(x => x.RemoteEndPoint.ToString());
                MainWindow.Instance.clientComBox.SelectedIndex = 0;
                foreach (var item in server.Sockets)
                {
                    UpdateConnectStatus($"客户端[{item.RemoteEndPoint}]", e.IsConnection);
                }
            }));

        }

        private void Server_DataReceived1(object sender, ReciveEventArgs e)
        {
            if ((int)e.MsgType == 0)
            {
                string data = Encoding.Default.GetString(e.Data);

                MainWindow.Instance.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MainWindow.Instance.textReciveMsg.Items.Add($"{DateTime.Now} 客户端[{server.Sockets[e.Index].RemoteEndPoint}] [{e.MsgType}]->{data}");
                    }));

            }
            else if ((int)e.MsgType == 1)
            {
                ParseFileByte(e, out string filename);
                MainWindow.Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MainWindow.Instance.textReciveMsg.Items.Add($"{DateTime.Now} 客户端[{server.Sockets[e.Index].RemoteEndPoint}] [{e.MsgType}]->{filename}");
                }));
            }
        }

        /// <summary>
        /// 发送消息到客户端
        /// </summary>
        /// <param name="str"></param>
        public void SendToClient(int index, MsgType type, object obj)
        {
            byte[] buffer = new byte[0];
            if (obj.GetType().FullName.Contains("String"))
            {
                buffer = Encoding.Default.GetBytes(obj.ToString());
                server.Send(index, (int)type, buffer);
            }
            else if (obj.GetType().FullName.Contains("Byte"))
            {
                buffer = (byte[])obj;
                server.Send(index, (int)type, buffer);
            }
        }
        /// <summary>
        /// 更新连接状态
        /// </summary>
        /// <param name="name">PCName</param>
        /// <param name="connect">连接状态</param>
        private void UpdateConnectStatus(string name, bool connect)
        {
            ConnectStatus status = new ConnectStatus(name, connect);
            MainWindow.Instance.ClientStackPanel.Children.Add(status.GetConnectStatus());
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
            server.Dispose();
            server = null;
        }
    }
}
