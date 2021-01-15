using Newtonsoft.Json;
using QQMini.PluginSDK.Core;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace com.kougami.sign
{
    public static class Genshin
    {
        public static string act_id = "e202009291139501";
        public static string url_role = "https://api-takumi.mihoyo.com/binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn";
        public static string url_sign = "https://api-takumi.mihoyo.com/event/bbs_sign_reward/sign";
        public static string url_award = "https://api-takumi.mihoyo.com/event/bbs_sign_reward/home?act_id=" + act_id;
        public static string url_info = "https://api-takumi.mihoyo.com/event/bbs_sign_reward/info?act_id=" + act_id + "&region=cn_gf01&uid=";
        public static Timer timer = new Timer();

        public static void Start()
        {
            timer.Enabled = true;
            timer.Interval = 600000; //间隔10分钟
            timer.Start();
            timer.Elapsed += new ElapsedEventHandler(Event_Timer);
        }

        private static void Event_Timer(object source, ElapsedEventArgs e)
        {
            if (!Program.enable) return;
            QMLog.CurrentApi.Debug("检测原神是否签到......");
            string[] member = Config.Get("genshin.ini", "all", "member").Split(',');
            foreach (string i in member)
            {
                string[] cookie = Config.Get("genshin.ini", i, "cookie").Split('#');
                foreach (string j in cookie)
                {
                    Role role = Get_Role(j);
                    string uid = role.data.list[0].game_uid;

                    SignInfo signinfo = Get_SignInfo(j, uid);
                    if (signinfo.data.is_sign) continue;
                    string result = "";
                    Response_Genshin_Sign info = Sign(j, uid);
                    if (info.retcode != 0)
                    {
                        result = "签到失败！\n错误代码：" + info.retcode + "\n错误提示：" + info.message;
                    }
                    else
                    {
                        result += "[" + role.data.list[0].game_uid + "]" + role.data.list[0].nickname + " 签到成功";
                        signinfo = Get_SignInfo(j, uid);
                        result += "\n累计签到 " + signinfo.data.total_sign_day + " 天";
                        Award award = Get_Award(j);
                        result += "\n今日奖励：" + award.data.awards[signinfo.data.total_sign_day - 1].name + "×" + award.data.awards[signinfo.data.total_sign_day - 1].cnt;
                    }
                    if (Config.Get("genshin.ini", i, "group", "-1") == "-1")
                    {
                        QMApi.CurrentApi.SendFriendMessage(long.Parse(Config.Get("config.ini", "all", "robot")), long.Parse(i), result);
                    }
                    else
                    {
                        QMApi.CurrentApi.SendGroupTempMessage(long.Parse(Config.Get("config.ini", "all", "robot")), long.Parse(Config.Get("genshin.ini", i, "group")), long.Parse(i), result);
                    }
                }
            }
        }

        public static string Run(string cookie)
        {
            string result = "";
            Role role = Get_Role(cookie);
            string uid = role.data.list[0].game_uid;

            Response_Genshin_Sign info = Sign(cookie, uid);
            if (info.retcode == -5003)
            {
                return "[" + role.data.list[0].game_uid + "]" + role.data.list[0].nickname + " 今日已签到";
            }
            else if (info.retcode != 0)
            {
                return "签到失败！\n错误代码：" + info.retcode + "\n错误提示：" + info.message;
            }
            result += "[" + role.data.list[0].game_uid + "]" + role.data.list[0].nickname + " 签到成功";

            SignInfo signinfo = Get_SignInfo(cookie, uid);
            result += "\n累计签到 " + signinfo.data.total_sign_day + " 天";

            Award award = Get_Award(cookie);
            result += "\n今日奖励：" + award.data.awards[signinfo.data.total_sign_day - 1].name + "×" + award.data.awards[signinfo.data.total_sign_day - 1].cnt;

            return result;
        }
        
        public static Award Get_Award(string cookie)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Cookie", cookie);
            return JsonConvert.DeserializeObject<Award>(HTTP_GET(url_award, header));
        }

        public static SignInfo Get_SignInfo(string cookie, string uid)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Cookie", cookie);
            return JsonConvert.DeserializeObject<SignInfo>(HTTP_GET(url_info + uid, header));
        }

        public static Role Get_Role(string cookie)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Cookie", cookie);
            return JsonConvert.DeserializeObject<Role>(HTTP_GET(url_role, header));
        }


        public static Response_Genshin_Sign Sign(string cookie, string uid)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("UserAgent", "Mozilla/5.0 (Linux; Android 6.0.1; MuMu Build/V417IR; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36 miHoYoBBS/2.4.0");
            header.Add("Content", "text/plain");
            header.Add("x-rpc-device_id", "fa498beb-eddf-345d-84e1-a3145b225309");
            header.Add("x-rpc-client_type", "5");
            header.Add("x-rpc-app_version", "2.2.1");
            header.Add("DS", Get_DS());
            header.Add("Cookie", cookie);
            Body_Genshin_Sign body = new Body_Genshin_Sign("e202009291139501", "cn_gf01", uid);
            return JsonConvert.DeserializeObject<Response_Genshin_Sign>(HTTP_POST(url_sign, body, header));
        }

        static string Get_DS()
        {
            string n = "cx2y9z9a29tfqvr1qsq6c7yz99b5jsqt";
            string i = GetTimeStamp();
            string r = GetRandomString(6);
            string c = GenerateMD5(string.Format("salt={0}&t={1}&r={2}", n, i, r));
            return string.Format("{0},{1},{2}", i, r, c);
        }

        /// <summary>
        /// MD5字符串加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns>加密后字符串</returns>
        public static string GenerateMD5(string txt)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] buffer = Encoding.Default.GetBytes(txt);
                //开始加密
                byte[] newBuffer = mi.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 随机生成字母与数字组合的字符串
        /// </summary>
        /// <param name="length">/param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            byte[] r = new byte[length];
            Random rand = new Random((int)(DateTime.Now.Ticks % 1000000));
            //生成8字节原始数据
            for (int i = 0; i < length; i++)
            {
                int ran;
                //while循环剔除非字母和数字的随机数
                do
                {
                    //数字范围是ASCII码中字母数字和一些符号
                    ran = rand.Next(48, 122);
                    r[i] = Convert.ToByte(ran);
                } while ((ran >= 58 && ran <= 64) || (ran >= 91 && ran <= 96));
            }
            //转换成8位String类型               
            return Encoding.ASCII.GetString(r);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// 发送GET请求 带请求头
        /// </summary>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static string HTTP_GET(string url, Dictionary<string, string> header = null)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
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
        /// <param name="obj_model"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string HTTP_POST(string url, object obj_model, Dictionary<string, string> dic = null)
        {
            string param = JsonConvert.SerializeObject(obj_model);
            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            if (dic != null && dic.Count != 0)
            {
                foreach (var item in dic)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            byte[] payload;
            payload = System.Text.Encoding.UTF8.GetBytes(param);
            request.ContentLength = payload.Length;
            string strValue = "";
            try
            {
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();
                System.Net.HttpWebResponse response;
                response = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream s;
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
    }

    #region 签到请求body

    public class Body_Genshin_Sign
    {
        public string act_id { get; set; }
        public string region { get; set; }
        public string uid { get; set; }
        public Body_Genshin_Sign(string act_id, string region, string uid)
        {
            this.act_id = act_id;
            this.region = region;
            this.uid = uid;
        }
    }

    #endregion

    #region 签到实体类

    public class Response_Genshin_Sign
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public Data_Sign data { get; set; }
    }
    public class Data_Sign
    {
        public string code { get; set; }
    }

    #endregion

    #region 角色信息实体类

    public class Role
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public Data_Role data { get; set; }
    }

    public class Data_Role
    {
        public List[] list { get; set; }
    }

    public class List
    {
        public string game_biz { get; set; }
        public string region { get; set; }
        public string game_uid { get; set; }
        public string nickname { get; set; }
        public int level { get; set; }
        public bool is_chosen { get; set; }
        public string region_name { get; set; }
        public bool is_official { get; set; }
    }

    #endregion

    #region 签到信息实体类

    public class SignInfo
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public Data_SignInfo data { get; set; }
    }

    public class Data_SignInfo
    {
        public int total_sign_day { get; set; }
        public string today { get; set; }
        public bool is_sign { get; set; }
        public bool first_bind { get; set; }
        public bool is_sub { get; set; }
        public bool month_first { get; set; }
    }

    #endregion

    #region 签到奖励

    public class Award
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public Data_Award data { get; set; }
    }

    public class Data_Award
    {
        public int month { get; set; }
        public Awards[] awards { get; set; }
    }

    public class Awards
    {
        public string icon { get; set; }
        public string name { get; set; }
        public int cnt { get; set; }
    }

    #endregion

}
