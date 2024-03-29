using Common.MySerialize;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Common.Enum;

namespace SocketDemo.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientPC client = new ClientPC();
        public static MainWindow Instance = null;
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            client.Init();

        }

        private void btnSendMsg_Click(object sender, RoutedEventArgs e)
        {
            client.SendToServer(MsgType.Info, textSendMsg.Text);
        }

        private void btnSendFile_Click(object sender, RoutedEventArgs e)
        {
            //打开文件
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "";
            ofd.RestoreDirectory = true;
            ofd.ShowDialog();
            //文件全路径
            textSendFile.Text = ofd.FileName;
            //文件名
            string fileName = ofd.SafeFileName;
            byte[] buffer;
            using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
            {

                byte[] fileNameBuffer = Encoding.Default.GetBytes(fileName);

                byte[] FileBuffer = new byte[fs.Length];

                //以#为分隔符
                byte[] filespilt = Encoding.Default.GetBytes("#");

                buffer = new byte[fileNameBuffer.Length + FileBuffer.Length + 2 * filespilt.Length];


                //文件名
                Array.Copy(fileNameBuffer, buffer, fileNameBuffer.Length);
                //分隔符
                Array.Copy(filespilt, 0, buffer, fileNameBuffer.Length, filespilt.Length);
                //文件
                fs.Read(FileBuffer, 0, (int)fs.Length);
                Array.Copy(FileBuffer, 0, buffer, fileNameBuffer.Length + filespilt.Length, FileBuffer.Length);

            }
            client.SendToServer(MsgType.File, buffer);
        }

        /// <summary>
        /// 类型序列化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDoSerialize_Click(object sender, RoutedEventArgs e)
        {
            string content1 = texContent1.Text.Trim();
            string content2 = texContent2.Text.Trim();
            string content3 = texContent3.Text.Trim();
            string content4 = texContent4.Text.Trim();
            SerializeData data = new SerializeData();
            data.Content1 = content1;
            data.Content2 = content2;
            data.Content3 = content3;
            data.Content4 = content4;
            //获取程序路径
            string rootFilePath = System.Environment.CurrentDirectory;
            //创建Json文件存放目录
            string savePath = System.IO.Path.Combine(rootFilePath, "SerializeFileDir");
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            //文件命名
            string fileName = DateTime.Now.ToString("yyyyMMddTHHmmss");
            //序列化
            if (cheJson.IsChecked == true)
            {
                Serialize.ObjectToJson(data, System.IO.Path.Combine(savePath, fileName + "_Json"));
            }
            if (cheXml.IsChecked == true)
            {
                Serialize.ObjectToXml(data, System.IO.Path.Combine(savePath, fileName + "_Xml"));
            }
            if (cheFile.IsChecked == true)
            {
                Serialize.ObjectToFile(data, System.IO.Path.Combine(savePath, fileName + "_File"));
            }

        }
        /// <summary>
        /// 文件反序列化为类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDoDeSerialize_Click(object sender, RoutedEventArgs e)
        {
            SerializeData data = data = new SerializeData();
            //反序列化
            if (cheJson.IsChecked == true)
            {
                data = Deserialize.JsonToObject(data);
                if (data != null)
                {
                    MessageBox.Show($"Json：\n\rContent1：{data.Content1}\n\rContent2：{data.Content2}\n\rContent3：{data.Content3}\n\rContent4：{data.Content4}");

                }
            }
            if (cheXml.IsChecked == true)
            {
                data = Deserialize.XmlToObject(data);
                if (data != null)
                {
                    MessageBox.Show($"Xml：\n\rContent1：{data.Content1}\n\rContent2：{data.Content2}\n\rContent3：{data.Content3}\n\rContent4：{data.Content4}");

                }

            }
            if (cheFile.IsChecked == true)
            {
                data = Deserialize.FileToObject(data);
                if (data != null)
                {
                    MessageBox.Show($"File：\n\rContent1：{data.Content1}\n\rContent2：{data.Content2}\n\rContent3：{data.Content3}\n\rContent4：{data.Content4}");

                }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Uinit();
            client = null;
        }
    }
}
