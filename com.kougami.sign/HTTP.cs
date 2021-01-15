using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace com.kougami.sign
{
    public static class HTTP
    {

        /// <summary>
        /// 发送GET请求 带请求头
        /// </summary>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static string GET(string url, Dictionary<string, string> header = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            foreach (KeyValuePair<string, string> i in header)
            {
                request.Headers[i.Key] = i.Value;
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            string str = streamReader.ReadToEnd();
            return str;
        }

        /// <summary>
        /// 发送POST请求 带请求头、请求体
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="header"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public static string POST(string url, Dictionary<string, string> body = null, Dictionary<string, string> header = null, int datatype = 0)
        {
            if (datatype == 0)
            {
                string param = JsonConvert.SerializeObject(body);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json;charset=UTF-8";
                if (header != null && header.Count != 0)
                {
                    foreach (var item in header)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }
                byte[] payload = Encoding.UTF8.GetBytes(param);
                request.ContentLength = payload.Length;
                string strValue = "";
                try
                {
                    Stream writer = request.GetRequestStream();
                    writer.Write(payload, 0, payload.Length);
                    writer.Close();
                    HttpWebResponse response;
                    response = (HttpWebResponse)request.GetResponse();
                    Stream s;
                    s = response.GetResponseStream();
                    string StrDate = "";
                    StreamReader Reader = new StreamReader(s, Encoding.UTF8);
                    while ((StrDate = Reader.ReadLine()) != null)
                    {
                        strValue += StrDate;
                    }
                }
                catch (Exception e)
                {
                    strValue = e.Message;
                }
                return strValue;
            }
            else if (datatype == 1)
            {
                string PostData = "";
                if (body != null)
                {
                    foreach (var i in body)
                    {
                        if (PostData != "") PostData += "&";
                        PostData += i.Key + "=" + i.Value;
                    }
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                if (header != null && header.Count != 0)
                {
                    foreach (var item in header)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }
                byte[] byteArray = Encoding.UTF8.GetBytes(PostData);
                request.ContentLength = byteArray.Length;
                using (Stream newStream = request.GetRequestStream())
                {
                    newStream.Write(byteArray, 0, byteArray.Length);//写入参数
                    newStream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string EndResult = "";
                Stream rspStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(rspStream, Encoding.UTF8))
                {
                    EndResult = reader.ReadToEnd();
                    rspStream.Close();
                }
                response.Close();
                return EndResult;
            }
            return "";
        }

    }
}
