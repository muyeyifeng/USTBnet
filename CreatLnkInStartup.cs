using System;
using System.IO;

namespace USTBnet
{
    class CreatLnkInStartup
    {
        readonly static string defaultStartPath = Environment.GetEnvironmentVariable("USERPROFILE") + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\校园网登录.appref-ms";
        readonly static string defaultOriginPath = Environment.GetEnvironmentVariable("USERPROFILE") + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Muyeyifeng\\校园网登录\\校园网登录.appref-ms";

        public static void CopyToStartUp()
        {
            try
            {
                File.Copy(defaultOriginPath, defaultStartPath, false);
            }
            catch { }

        }
        public static void RemoveStartUp()
        {
            try
            {
                File.Delete(defaultStartPath);
            }
            catch { }
        }
        public static bool StartPathIsExists()
        {
            return File.Exists(defaultStartPath);
        }
    }
}
