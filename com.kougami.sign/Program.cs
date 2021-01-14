using QQMini.PluginSDK.Core;
using QQMini.PluginSDK.Core.Model;
using System;
using System.Diagnostics.Eventing.Reader;

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
            Config.path = QMApi.CurrentApi.GetPluginDataDirectory();
            if (Config.Get("config.ini", "all", "robot", "") != "")
            {
                enable = true;
            }
            Genshin.Start();
            Tieba.Start();
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

            if (e.Message.Text == "原神签到")
            {
                QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "Cookie录入开始，请发送一条单独包含Cookie的消息（多账号可用 # 分割Cookie）\n输入none清除已录入数据");
                Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "writing", "true");
            }
            else if (Config.Get("genshin.ini", e.FromQQ.Id.ToString(), "writing", "false") == "true")
            {
                Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "writing", "false");
                if (e.Message.Text == "none")
                {
                    Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "cookie", "");
                    Config.Set("genshin.ini", "all", "member", Config.Get("genshin.ini", "all", "member").Replace("," + e.FromQQ.Id.ToString(), "").Replace(e.FromQQ.Id.ToString(), ""));
                    QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "清除成功");
                }
                else
                {
                    Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "cookie", e.Message.Text);
                    if (!Config.Get("genshin.ini", "all", "member").Contains(e.FromQQ.Id.ToString())) Config.Set("genshin.ini", "all", "member", Config.Get("genshin.ini", "all", "member", "") == "" ? e.FromQQ.Id.ToString() : Config.Get("genshin.ini", "all", "member") + "," + e.FromQQ.Id.ToString());
                    QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "Cookie录入完毕！接下来将进行一次测试签到");
                    try
                    {
                        string[] cookie = Config.Get("genshin.ini", e.FromQQ.Id.ToString(), "cookie").Split('#');
                        foreach (string i in cookie)
                        {
                            QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, Genshin.Run(i));
                        }
                    }
                    catch
                    {
                        QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "发生未知错误，请联系物理管理员");
                    }
                }
            }
            else if (e.Message.Text == "贴吧签到")
            {
                QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "Cookie录入开始，请发送一条单独包含Cookie的消息\n输入none清除已录入数据");
                Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "writing", "true");
            }
            else if (Config.Get("tieba.ini", e.FromQQ.Id.ToString(), "writing", "false") == "true")
            {
                Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "writing", "false");
                if (e.Message.Text == "none")
                {
                    Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "cookie", "");
                    Config.Set("tieba.ini", "all", "member", Config.Get("tieba.ini", "all", "member").Replace("," + e.FromQQ.Id.ToString(), "").Replace(e.FromQQ.Id.ToString(), ""));
                    QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "清除成功");
                }
                else
                {
                    Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "cookie", e.Message.Text);
                    if (!Config.Get("tieba.ini", "all", "member").Contains(e.FromQQ.Id.ToString())) Config.Set("tieba.ini", "all", "member", Config.Get("tieba.ini", "all", "member", "") == "" ? e.FromQQ.Id.ToString() : Config.Get("tieba.ini", "all", "member") + "," + e.FromQQ.Id.ToString());
                    QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "Cookie录入完毕！接下来将进行一次测试签到");
                    //try
                    {
                        string cookie = e.Message.Text;
                        string result = Tieba.Run(cookie);
                        QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, result);
                        if (result.Contains("失败")) QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "将于 10 分钟内重试");
                    }
                    //catch
                    {
                        //QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "发生未知错误，请联系物理管理员");
                    }
                }
            }

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

            if (e.Message.Text == "原神签到")
            {
                QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, "Cookie录入开始，请发送一条单独包含Cookie的消息（多账号可用 # 分割Cookie）\n输入none清除已录入数据");
                Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "writing", "true");
            }
            else if (Config.Get("genshin.ini", e.FromQQ.Id.ToString(), "writing", "false") == "true")
            {
                Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "writing", "false");
                if (e.Message.Text == "none")
                {
                    Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "cookie", "");
                    Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "group", "");
                    Config.Set("genshin.ini", "all", "member", Config.Get("genshin.ini", "all", "member").Replace("," + e.FromQQ.Id.ToString(), "").Replace(e.FromQQ.Id.ToString(), ""));
                    QMApi.SendFriendMessage(e.RobotQQ, e.FromQQ, "清除成功");
                }
                else
                {
                    Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "cookie", e.Message.Text);
                    Config.Set("genshin.ini", e.FromQQ.Id.ToString(), "group", e.FromGroup.Id.ToString());
                    if (!Config.Get("genshin.ini", "all", "member").Contains(e.FromQQ.Id.ToString())) Config.Set("genshin.ini", "all", "member", Config.Get("genshin.ini", "all", "member", "") == "" ? e.FromQQ.Id.ToString() : Config.Get("genshin.ini", "all", "member") + "," + e.FromQQ.Id.ToString());
                    QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, "Cookie录入完毕！接下来将进行一次测试签到");
                    try
                    {
                        string[] cookie = Config.Get("genshin.ini", e.FromQQ.Id.ToString(), "cookie").Split('#');
                        foreach (string i in cookie)
                        {
                            QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, Genshin.Run(i));
                        }
                    }
                    catch
                    {
                        QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, "发生未知错误，请联系物理管理员");
                    }
                }
            }
            else if (e.Message.Text == "贴吧签到")
            {
                QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, "Cookie录入开始，请发送一条单独包含Cookie的消息\n输入none清除已录入数据");
                Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "writing", "true");
            }
            else if (Config.Get("tieba.ini", e.FromQQ.Id.ToString(), "writing", "false") == "true")
            {
                Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "writing", "false");
                if (e.Message.Text == "none")
                {
                    Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "cookie", "");
                    Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "group", "");
                    Config.Set("tieba.ini", "all", "member", Config.Get("tieba.ini", "all", "member").Replace("," + e.FromQQ.Id.ToString(), "").Replace(e.FromQQ.Id.ToString(), ""));
                    QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, "清除成功");
                }
                else
                {
                    Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "cookie", e.Message.Text);
                    Config.Set("tieba.ini", e.FromQQ.Id.ToString(), "group", e.FromGroup.Id.ToString());
                    if (!Config.Get("tieba.ini", "all", "member").Contains(e.FromQQ.Id.ToString())) Config.Set("tieba.ini", "all", "member", Config.Get("tieba.ini", "all", "member", "") == "" ? e.FromQQ.Id.ToString() : Config.Get("tieba.ini", "all", "member") + "," + e.FromQQ.Id.ToString());
                    QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, "Cookie录入完毕！接下来将进行一次测试签到");
                    try
                    {
                        string cookie = Config.Get("tieba.ini", e.FromQQ.Id.ToString(), "cookie");
                        string result = Tieba.Run(cookie);
                        QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, result);
                        if (result.Contains("失败")) QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, "将于 10 分钟内重试");
                    }
                    catch
                    {
                        QMApi.SendGroupTempMessage(e.RobotQQ, e.FromGroup, e.FromQQ, "发生未知错误，请联系物理管理员");
                    }
                }
            }

            return QMEventHandlerTypes.Intercept;
        }

    }
}