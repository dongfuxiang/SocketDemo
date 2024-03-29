using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace Common.MySerialize
{
    public static class Deserialize
    {
        /// <summary>
        /// json字符串转成对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="json">json格式字符串</param>
        /// <returns>对象</returns>
        public static T JsonToObject<T>(T t) where T : class
        {
            try
            {
                DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(T));
                var type = t.GetType();
                var p = type.GetProperties();

                //打开文件
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "";
                ofd.RestoreDirectory = true;
                ofd.ShowDialog();
                //文件全路径
                string filePath = ofd.FileName;
                //获取Json字符串
                string json = File.ReadAllText(filePath);

                using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
                {
                    T result = formatter.ReadObject(stream) as T;
                    return result;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

            return null;


        }


        /// <summary>
        /// 将xml文件反序列化为对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="t">对象</param>
        /// <param name="path">xml路径</param>
        /// <returns>对象</returns>
        public static T XmlToObject<T>(T t) where T : class
        {
            try
            {
                //打开文件
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "";
                ofd.RestoreDirectory = true;
                ofd.ShowDialog();
                //文件全路径
                string filePath = ofd.FileName;
                XmlSerializer formatter = new XmlSerializer(typeof(T));
                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    XmlReader xmlReader = new XmlTextReader(stream);
                    T result = formatter.Deserialize(xmlReader) as T;
                    return result;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// 将文件反序列化为对象
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        /// <returns>对象</returns>
        public static T FileToObject<T>(T t) where T : class
        {
            try
            {
                //打开文件
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "";
                ofd.RestoreDirectory = true;
                ofd.ShowDialog();
                //文件全路径
                string filePath = ofd.FileName;
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    T result = formatter.Deserialize(stream) as T;
                    return result;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            return null;


        }
    }
}
