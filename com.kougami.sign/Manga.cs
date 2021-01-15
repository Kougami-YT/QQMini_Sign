using Newtonsoft.Json;
using QQMini.PluginSDK.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace com.kougami.sign
{
    public static class Manga
    {
        public static string url_sign = "https://manga.bilibili.com/twirp/activity.v1.Activity/ClockIn";
        public static string url_signinfo = "https://manga.bilibili.com/twirp/activity.v1.Activity/GetClockInInfo";
        public static string url_accountinfo = "http://api.bilibili.com/x/web-interface/nav";
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
            QMLog.CurrentApi.Debug("检测b漫是否签到......");
            string[] member = Config.Get("manga.ini", "all", "member").Split(',');
            foreach (string i in member)
            {
                string cookie = Config.Get("manga.ini", i, "cookie");
                string result = Run(cookie);
                if (result.Contains("已签")) continue;
                if (Config.Get("manga.ini", i, "group", "-1") == "-1")
                {
                    QMApi.CurrentApi.SendFriendMessage(long.Parse(Config.Get("config.ini", "all", "robot")), long.Parse(i), result);
                    if (result.Contains("失败")) QMApi.CurrentApi.SendFriendMessage(long.Parse(Config.Get("config.ini", "all", "robot")), long.Parse(i), "将于 10 分钟后重试");
                }
                else
                {
                    QMApi.CurrentApi.SendGroupTempMessage(long.Parse(Config.Get("config.ini", "all", "robot")), long.Parse(Config.Get("manga.ini", i, "group")), long.Parse(i), result);
                    if (result.Contains("失败")) QMApi.CurrentApi.SendGroupTempMessage(long.Parse(Config.Get("config.ini", "all", "robot")), long.Parse(Config.Get("manga.ini", i, "group")), long.Parse(i), "将于 10 分钟后重试");
                }
            }
        }

        public static string Run(string cookie)
        {
            BLBLAccountInfo accountinfo = GetAccountInfo(cookie);
            mangaSignInfo info = GetSignInfo(cookie);
            if (info.data.status == 1) return "[" + accountinfo.data.mid + "]" + accountinfo.data.uname + " bilibili漫画今日已签到";
            Response_manga_Sign sign_result = Sign(cookie);
            string result = "";
            if (sign_result.code == "0")
            {
                info = GetSignInfo(cookie);
                result += "[" + accountinfo.data.mid + "]" + accountinfo.data.uname + " bilibili漫画签到成功";
                result += "\n连续签到 " + info.data.day_count + " 天";
                result += "今日奖励：" + info.data.point_infos[(info.data.day_count % 7) - 1].title;
            }
            else
            {
                result += "[" + accountinfo.data.mid + "]" + accountinfo.data.uname + " bilibili漫画签到失败";
            }
            return result;
        }

        public static BLBLAccountInfo GetAccountInfo(string cookie)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Cookie", cookie);
            return JsonConvert.DeserializeObject<BLBLAccountInfo>(HTTP.GET(url_accountinfo, header));
        }

        public static mangaSignInfo GetSignInfo(string cookie)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Cookie", cookie);
            return JsonConvert.DeserializeObject<mangaSignInfo>(HTTP.POST(url_signinfo, null, header, 1));
        }

        public static Response_manga_Sign Sign(string cookie)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Cookie", cookie);
            Dictionary<string, string> body = new Dictionary<string, string>();
            body.Add("platform", "android");
            return JsonConvert.DeserializeObject<Response_manga_Sign>(HTTP.POST(url_sign, body, header, 1));
        }

    }

    #region 签到信息实体类

    public class mangaSignInfo
    {
        public int code { get; set; }
        public string msg { get; set; }
        public Data_manga data { get; set; }
    }

    public class Data_manga
    {
        public int day_count { get; set; }
        public int status { get; set; }
        public int[] points { get; set; }
        public string credit_icon { get; set; }
        public string sign_before_icon { get; set; }
        public string sign_today_icon { get; set; }
        public string breathe_icon { get; set; }
        public Point_Infos[] point_infos { get; set; }
        public string new_credit_x_icon { get; set; }
        public string coupon_pic { get; set; }
    }

    public class Point_Infos
    {
        public int point { get; set; }
        public int origin_point { get; set; }
        public bool is_activity { get; set; }
        public string title { get; set; }
    }

    #endregion

    #region 账号信息实体类

    public class BLBLAccountInfo
    {
        public int code { get; set; }
        public string message { get; set; }
        public int ttl { get; set; }
        public Data_Account data { get; set; }
    }

    public class Data_Account
    {
        public bool isLogin { get; set; }
        public int email_verified { get; set; }
        public string face { get; set; }
        public Level_Info level_info { get; set; }
        public int mid { get; set; }
        public int mobile_verified { get; set; }
        public int money { get; set; }
        public int moral { get; set; }
        public Official official { get; set; }
        public Officialverify officialVerify { get; set; }
        public Pendant pendant { get; set; }
        public int scores { get; set; }
        public string uname { get; set; }
        public long vipDueDate { get; set; }
        public int vipStatus { get; set; }
        public int vipType { get; set; }
        public int vip_pay_type { get; set; }
        public int vip_theme_type { get; set; }
        public Vip_Label vip_label { get; set; }
        public int vip_avatar_subscript { get; set; }
        public string vip_nickname_color { get; set; }
        public Wallet wallet { get; set; }
        public bool has_shop { get; set; }
        public string shop_url { get; set; }
        public int allowance_count { get; set; }
        public int answer_status { get; set; }
    }

    public class Level_Info
    {
        public int current_level { get; set; }
        public int current_min { get; set; }
        public int current_exp { get; set; }
        public string next_exp { get; set; }
    }

    public class Official
    {
        public int role { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public int type { get; set; }
    }

    public class Officialverify
    {
        public int type { get; set; }
        public string desc { get; set; }
    }

    public class Pendant
    {
        public int pid { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public int expire { get; set; }
        public string image_enhance { get; set; }
        public string image_enhance_frame { get; set; }
    }

    public class Vip_Label
    {
        public string path { get; set; }
        public string text { get; set; }
        public string label_theme { get; set; }
    }

    public class Wallet
    {
        public int mid { get; set; }
        public int bcoin_balance { get; set; }
        public int coupon_balance { get; set; }
        public int coupon_due_time { get; set; }
    }

    #endregion

    #region 签到实体类

    public class Response_manga_Sign
    {
        public string code { get; set; }
        public string msg { get; set; }
        public Meta meta { get; set; }
    }

    public class Meta
    {
        public string argument { get; set; }
    }

    #endregion

}
