using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace com.kougami.sign
{
    public static class Config
    {
        public static string path;
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public static void Set(string file, string section, string key, string value) //写配置项
        {
            WritePrivateProfileString(section, key, value, path + file);
        }
        public static string Get(string file, string section, string key, string def = "") //读配置项
        {
            StringBuilder result = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, result, 1024, path + file);
            return result.ToString();
        }
    }
}
