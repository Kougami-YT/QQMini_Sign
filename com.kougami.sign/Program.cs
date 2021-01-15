using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;
using System;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;

namespace com.kougami.sign
{
    public class Program : PluginBase
    {
        public static bool enable = false;
        public override PluginInfo PluginInfo
        {
            get
            {
                PluginInfo info = new PluginInfo();
                info.PackageId = "com.kougami.sign";
                info.Name = "签到机";
                info.Version = new System.Version(1, 0, 0, 0);
                info.Author = "Kougami";
                info.Description = "QQMini插件";
                return info;
            }
        }

        /// <summary>
        /// 启动事件
        /// </summary>
        public override void OnInitialize()
        {
            Config.path = QMApi.GetPluginDataDirectory();
            if (Config.Get("config.ini", "all", "robot", "") != "")
            {
                enable = true;
            }
            Genshin.Start();
            Tieba.Start();
            Manga.Start();
        }

        /// <summary>
        /// 群消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnReceiveGroupMessage(QMGroupMessageEventArgs e)
        {
            //QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, e.Message);
            if (e.Message.Text == "启动" && !enable)
            {
                Config.Set("config.ini", "all", "robot", e.RobotQQ.Id.ToString());
                enable = true;
                QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, "启动成功");
            }
            if (e.Message.Text == "关闭" && enable)
            {
                Config.Set("config.ini", "all", "robot", "");
                enable = false;
                QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, "关闭成功");
            }
            else if (e.Message.Text.Contains("[@" + e.RobotQQ.Id.ToString() + "]") && enable)
            {
                //QMApi.SendGroupMessage(e.RobotQQ, e.FromGroup, Menu());
            }

            return QMEventHandlerTypes.Intercept;    // 返回继续执行时, 后续的插件将会接收到此消息
        }
        
        string Menu()
        {
            string result = "";

            return result;
        }

        /// <summary>
        /// 好友消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnReceiveFriendMessage(QMPrivateMessageEventArgs e)
        {
            if (!enable) return QMEventHandlerTypes.Continue;

            OnReceivePrivateMessage(e.RobotQQ, -1, e.FromQQ, e.Message);

            return QMEventHandlerTypes.Intercept;
        }

        /// <summary>
        /// 群临时消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override QMEventHandlerTypes OnReceiveGroupTempMessage(QMGroupPrivateMessageEventArgs e)
        {
            if (!enable) return QMEventHandlerTypes.Continue;

            OnReceivePrivateMessage(e.RobotQQ, e.FromGroup, e.FromQQ, e.Message);

            return QMEventHandlerTypes.Intercept;
        }

        public void OnReceivePrivateMessage(long robotQQ, long fromGroup, long fromQQ, string message)
        {
            if (message == "原神签到")
            {
                SendPrivateMessage(robotQQ, fromGroup, fromQQ, "Cookie录入开始，请发送一条单独包含Cookie的消息（多账号可用 # 分割Cookie）\n输入none清除已录入数据");
                Config.Set("genshin.ini", fromQQ.ToString(), "writing", "true");
            }
            else if (Config.Get("genshin.ini", fromQQ.ToString(), "writing", "false") == "true")
            {
                Config.Set("genshin.ini", fromQQ.ToString(), "writing", "false");
                if (message == "none")
                {
                    Config.Set("genshin.ini", fromQQ.ToString(), "cookie", "");
                    Config.Set("genshin.ini", fromQQ.ToString(), "group", "");
                    Config.Set("genshin.ini", "all", "member", Config.Get("genshin.ini", "all", "member").Replace("," + fromQQ.ToString(), "").Replace(fromQQ.ToString(), ""));
                    SendPrivateMessage(robotQQ, fromGroup, fromQQ, "清除成功");
                }
                else
                {
                    string cookies = "";
                    foreach (string i in message.Split('#'))
                    {
                        if (cookies != "") cookies += "#";
                        cookies += GetPart(i, "account_id", "cookie_token");
                    }
                    Config.Set("genshin.ini", fromQQ.ToString(), "cookie", cookies);
                    Config.Set("genshin.ini", fromQQ.ToString(), "group", fromGroup.ToString());
                    if (!Config.Get("genshin.ini", "all", "member").Contains(fromQQ.ToString())) Config.Set("genshin.ini", "all", "member", Config.Get("genshin.ini", "all", "member", "") == "" ? fromQQ.ToString() : Config.Get("genshin.ini", "all", "member") + "," + fromQQ.ToString());
                    SendPrivateMessage(robotQQ, fromGroup, fromQQ, "Cookie录入完毕！接下来将进行一次测试签到");
                    try
                    {
                        string[] cookie = Config.Get("genshin.ini", fromQQ.ToString(), "cookie").Split('#');
                        foreach (string i in cookie)
                        {
                            SendPrivateMessage(robotQQ, fromGroup, fromQQ, Genshin.Run(i));
                        }
                    }
                    catch
                    {
                        SendPrivateMessage(robotQQ, fromGroup, fromQQ, "发生未知错误，请联系物理管理员");
                    }
                }
            }
            else if (message == "贴吧签到")
            {
                SendPrivateMessage(robotQQ, fromGroup, fromQQ, "Cookie录入开始，请发送一条单独包含Cookie的消息\n输入none清除已录入数据");
                Config.Set("tieba.ini", fromQQ.ToString(), "writing", "true");
            }
            else if (Config.Get("tieba.ini", fromQQ.ToString(), "writing", "false") == "true")
            {
                Config.Set("tieba.ini", fromQQ.ToString(), "writing", "false");
                if (message == "none")
                {
                    Config.Set("tieba.ini", fromQQ.ToString(), "cookie", "");
                    Config.Set("tieba.ini", fromQQ.ToString(), "group", "");
                    Config.Set("tieba.ini", "all", "member", Config.Get("tieba.ini", "all", "member").Replace("," + fromQQ.ToString(), "").Replace(fromQQ.ToString(), ""));
                    SendPrivateMessage(robotQQ, fromGroup, fromQQ, "清除成功");
                }
                else
                {
                    Config.Set("tieba.ini", fromQQ.ToString(), "cookie", GetPart(message, "BDUSS"));
                    Config.Set("tieba.ini", fromQQ.ToString(), "group", fromGroup.ToString());
                    if (!Config.Get("tieba.ini", "all", "member").Contains(fromQQ.ToString())) Config.Set("tieba.ini", "all", "member", Config.Get("tieba.ini", "all", "member", "") == "" ? fromQQ.ToString() : Config.Get("tieba.ini", "all", "member") + "," + fromQQ.ToString());
                    SendPrivateMessage(robotQQ, fromGroup, fromQQ, "Cookie录入完毕！接下来将进行一次测试签到");
                    try
                    {
                        string cookie = Config.Get("tieba.ini", fromQQ.ToString(), "cookie");
                        string result = Tieba.Run(cookie);
                        SendPrivateMessage(robotQQ, fromGroup, fromQQ, result);
                        if (result.Contains("失败")) SendPrivateMessage(robotQQ, fromGroup, fromQQ, "将于 10 分钟内重试");
                    }
                    catch
                    {
                        SendPrivateMessage(robotQQ, fromGroup, fromQQ, "发生未知错误，请联系物理管理员");
                    }
                }
            }
            else if (message == "漫画签到")
            {
                SendPrivateMessage(robotQQ, fromGroup, fromQQ, "Cookie录入开始，请发送一条单独包含Cookie的消息\n输入none清除已录入数据");
                Config.Set("manga.ini", fromQQ.ToString(), "writing", "true");
            }
            else if (Config.Get("manga.ini", fromQQ.ToString(), "writing", "false") == "true")
            {
                Config.Set("manga.ini", fromQQ.ToString(), "writing", "false");
                if (message == "none")
                {
                    Config.Set("manga.ini", fromQQ.ToString(), "cookie", "");
                    Config.Set("manga.ini", fromQQ.ToString(), "group", "");
                    Config.Set("manga.ini", "all", "member", Config.Get("manga.ini", "all", "member").Replace("," + fromQQ.ToString(), "").Replace(fromQQ.ToString(), ""));
                    SendPrivateMessage(robotQQ, fromGroup, fromQQ, "清除成功");
                }
                else
                {
                    Config.Set("manga.ini", fromQQ.ToString(), "cookie", GetPart(message, "SESSDATA", "bili_jct"));
                    Config.Set("manga.ini", fromQQ.ToString(), "group", fromGroup.ToString());
                    if (!Config.Get("manga.ini", "all", "member").Contains(fromQQ.ToString())) Config.Set("manga.ini", "all", "member", Config.Get("manga.ini", "all", "member", "") == "" ? fromQQ.ToString() : Config.Get("manga.ini", "all", "member") + "," + fromQQ.ToString());
                    SendPrivateMessage(robotQQ, fromGroup, fromQQ, "Cookie录入完毕！接下来将进行一次测试签到");
                    try
                    {
                        string cookie = Config.Get("manga.ini", fromQQ.ToString(), "cookie");
                        string result = Manga.Run(cookie);
                        SendPrivateMessage(robotQQ, fromGroup, fromQQ, result);
                        if (result.Contains("失败")) SendPrivateMessage(robotQQ, fromGroup, fromQQ, "将于 10 分钟内重试");
                    }
                    catch
                    {
                        SendPrivateMessage(robotQQ, fromGroup, fromQQ, "发生未知错误，请联系物理管理员");
                    }
                }
            }
        }

        public void SendPrivateMessage(long robotQQ, long fromGroup, long fromQQ, string message)
        {
            if (fromGroup == -1)
            {
                QMApi.SendFriendMessage(robotQQ, fromQQ, message);
            }
            else
            {
                QMApi.SendGroupTempMessage(robotQQ, fromGroup, fromQQ, message);
            }
        }

        /// <summary>
        /// 提取cookie需要部分
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private string GetPart(string cookie, params string[] keys)
        {
            string result = "";
            foreach (string i in keys)
            {
                if (!cookie.Contains(i + "=")) continue;
                if (!cookie.Substring(cookie.IndexOf(i + "=")).Contains(";"))
                {
                    result += cookie.Substring(cookie.IndexOf(i + "=")) + ";";
                }
                else
                {
                    result += i + "=" + Tieba.Mid(cookie, i + "=", ";") + ";";
                }
            }
            return result;
        }

        public override void OnOpenSettingMenu()
        {
            Setting form = new Setting();
            form.Show();
        }
    }
}