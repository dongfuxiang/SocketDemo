using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace Common.MySerialize
{
    public static  class Serialize
    {
        /// <summary>
        /// 将对象序列化为Json文件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="t">实例</param>
        /// <param name="path">保存路径</param>
        public static void ObjectToJson<T>(T t, string path) where T : class
        {
            try
            {
                DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(T));
                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    formatter.WriteObject(stream, t);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            
        }
        /// <summary>
        /// 将对象序列化为xml文件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="t">对象</param>
        /// <param name="path">xml存放路径</param>
        public static void ObjectToXml<T>(T t, string path) where T : class
        {
            try
            {
                XmlSerializer formatter = new XmlSerializer(typeof(T));
                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    formatter.Serialize(stream, t);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            
        }
        /// <summary>
        /// 将对象序列化为文件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="t">实例</param>
        /// <param name="path">存放路径</param>
        public static void ObjectToFile<T>(T t, string path)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(stream, t);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
