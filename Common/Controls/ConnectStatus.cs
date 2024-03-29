using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Common.Controls
{
    /// <summary>
    /// 显示连接状态的控件
    /// </summary>
    public class ConnectStatus
    {
        /// <summary>
        /// 连接状态
        /// </summary>
        bool IsConnect { get; set; }
        /// <summary>
        /// 服务端\客户端名称
        /// </summary>
        string PcName { get; set; }
        private Brush Color
        {
            get
            {
                return IsConnect ? Brushes.Green : Brushes.Green;
            }
            set { }
        }
        public ConnectStatus(string name, bool connect)
        {
            IsConnect = connect;
            PcName = name;
        }

        public Border GetConnectStatus()
        {
            Border border = new Border()
            {
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Margin = new Thickness(0, 2, 0, 2)
            };
            StackPanel sp1 = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };
            border.Child = sp1;
            StackPanel sp2 = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Background = Brushes.LightBlue,
                Height = 25,
            };
            sp1.Children.Add(sp2);
            Rectangle rectangle = new Rectangle()
            {
                Stroke = Brushes.Black,
                Fill = Color,
                Width = 60,
                Margin = new Thickness(0, 2, 0, 2)
            };
            Label label = new Label()
            {
                Content = PcName,
            };
            sp2.Children.Add(label);
            sp2.Children.Add(rectangle);

            return border;
        }

    }
}
