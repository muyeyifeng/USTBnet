using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Net.Sockets;
using System.Collections.Generic;

namespace USTBnet
{
    /// <summary>
    /// 
    /// </summary>
    class WebContents
    {
        /**********************************************************************************************/
        /******************************************实际调用方法****************************************/
        /// <summary>
        /// 返回GET方法对host的网页内容，返回值为字符串类型
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static string GetHttpRequest(HttpClientHandler httpClientHandler, string host)
        {
            try
            {
                HttpClient httpClient = new HttpClient(httpClientHandler)
                {
                    Timeout = TimeSpan.FromSeconds(3)
                };
                var getAsync = httpClient.GetStringAsync(host);
                getAsync.Wait();
                string finalget = getAsync.Result;
                return finalget;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 返回POST方法对host的网页内容，返回值为字符串类型
        /// </summary>
        /// <param name="host">目标网站</param>
        /// <param name="paramNames">参数名列表</param>
        /// <param name="param">参数值列表</param>
        /// <returns></returns>
        public static string PostHttpRequest(HttpClientHandler httpClientHandler, string host, string[] paramNames, string[] param)
        {
            try
            {
                Dictionary<string, string> keyValuePairs = GroupKeyValues(paramNames, param);
                var content = new FormUrlEncodedContent(keyValuePairs);
                HttpClient httpClient = new HttpClient(httpClientHandler)
                {
                    Timeout = TimeSpan.FromSeconds(3)
                };
                var postAsync = httpClient.PostAsync(host, content);
                postAsync.Wait();
                HttpResponseMessage httpResponseMessage = postAsync.Result;
                httpResponseMessage.StatusCode.ToString();
                var readAsync = httpResponseMessage.Content.ReadAsStringAsync();
                readAsync.Wait();
                return readAsync.Result;
            }
            catch
            {
                return null;
            }

        }
        /**********************************************************************************************/
        /******************************************辅助实现方法****************************************/
        /// <summary>
        /// 将参数名和参数值组合
        /// </summary>
        /// <param name="paramNames"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GroupKeyValues(string[] paramNames, string[] param)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            if (paramNames == null || param == null || paramNames.Length != param.Length)
                return keyValuePairs;
            for (int i = 0; i < paramNames.Length; i++)
            {
                keyValuePairs.Add(paramNames[i], param[i]);
            }
            return keyValuePairs;
        }
        /// <summary>
        /// BASE64编码图片解码方法
        /// </summary>
        /// <param name="strbase64">BASE64字符串</param>
        /// <returns></returns>
        public static string Base64StringToImage(string strbase64)
        {
            try
            {
                byte[] bArr = Convert.FromBase64String(strbase64);
                Stream imageStream = new MemoryStream(bArr);
                Random rd = new Random();
                DateTime nowTime = DateTime.Now;
                string fileName = "QRCODE" + nowTime.Minute.ToString() + nowTime.Second.ToString() + rd.Next(1000, 1000000) + ".png";
                Stream fileStream = new FileStream(fileName, FileMode.Create);
                int size;
                do
                {
                    size = imageStream.Read(bArr, 0, (int)bArr.Length);
                    fileStream.Write(bArr, 0, size);
                } while (size > 0);
                fileStream.Close();
                imageStream.Close();
                return fileName;
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 返回本机IPV6地址数组v6iplist
        /// </summary>
        /// <returns></returns>
        public static string GetIPV6()
        {
            List<string> AddressIP = new List<string>();
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    string perpareIp = _IPAddress.ToString();
                    if (perpareIp.IndexOf("2001:da8") >= 0 && !AddressIP.Contains(perpareIp))
                        AddressIP.Add(perpareIp);
                }
            }
            string[] v6ipList = AddressIP.ToArray();
            if (v6ipList.Length > 0)
                return v6ipList[0];
            else
                return null;
        }
        /// <summary>
        /// 将字符串转换为十进制整数，字符串中的非数字会被忽略
        /// </summary>
        /// <param name="numstr">数字字符串</param>
        /// <returns></returns>
        protected static long StringToInt(string numstr)
        {
            long result = 0;
            foreach (char chr in numstr)
            {
                if (chr >= '0' && chr <= '9')
                    result = result * 10 + (chr - '0');
            }
            return result;
        }
        /// <summary>
        /// 下载电子支付网站图片，返回图片名
        /// </summary>
        /// <param name="picUrl"></param>
        /// <returns></returns>
        public static string SaveAsWebImg(HttpClientHandler httpClientHandler, string picUrl, string picName)
        {
            string result = "";
            try
            {
                if (!String.IsNullOrEmpty(picUrl))
                {
                    HttpClient getAuthImage = new HttpClient(httpClientHandler)
                    {
                        Timeout = TimeSpan.FromSeconds(0.5)
                    };
                    var inputStream = getAuthImage.GetStreamAsync(picUrl);
                    inputStream.Wait();
                    Stream imageStream = inputStream.Result;

                    Random rd = new Random();
                    DateTime nowTime = DateTime.Now;
                    string fileName = picName + nowTime.Minute.ToString() + nowTime.Second.ToString() + rd.Next(1000, 1000000) + ".png";
                    Stream fileStream = new FileStream(fileName, FileMode.Create);
                    byte[] bArr = new byte[1024];
                    int size;
                    do
                    {
                        size = imageStream.Read(bArr, 0, (int)bArr.Length);
                        fileStream.Write(bArr, 0, size);
                    } while (size > 0);
                    fileStream.Close();
                    imageStream.Close();
                    result = fileName;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return result;
        }
    }
    class Pay : WebContents
    {
        //网络流量缴费参数 NWTF:NetWorkTrafficFee
        private static readonly string NWTF_fontuserLogin = "https://pay.ustb.edu.cn/fontuserLogin";
        private static readonly string NWTF_checkPayProject = "https://pay.ustb.edu.cn/publicPayPro!checkPayProjectCouldPay.do?payProjectId=1 ";
        private static readonly string NWTF_checkCPayProject = "https://pay.ustb.edu.cn/publicPayPro!checkPayProjectCouldPay.do?payProjectId=105 ";
        private static readonly string NWTF_netdetails511N004 = "https://pay.ustb.edu.cn/netdetails511N004";
        private static readonly string NWTF_netCreateOrder = "https://pay.ustb.edu.cn/netCreateOrder";
        private static readonly string NWTF_scardRechargeCreateOrder = "https://pay.ustb.edu.cn/scardRechargeCreateOrder";
        private static readonly string NWTF_onlinePay = "https://pay.ustb.edu.cn/onlinePay";
        private static readonly string NWTF_authImage = "https://pay.ustb.edu.cn/authImage";
        private static readonly string[] LoginParamsName = { "nickName", "password", "checkCode" };
        private static readonly string[] NetCreateOrderParamsName = { "netNo", "payAmt", "netStatus", "factorycode", "payProjectId" };
        private static readonly string[] OnlinePayParamsName = { "payType", "orderno", "orderamt" };
        private static readonly string[] scardRechargeCreateOrderParamsName = { "payAmt", "factorycode", "payProjectId" };

        //网络访问进程，网络流量费与校园卡充值共用payCenterHttpClientHandler
        private static readonly HttpClientHandler payCenterHttpClientHandler = new HttpClientHandler() { UseCookies = true };
        /// <summary>
        /// 发送一次POST获取Cookie
        /// </summary>
        public static void GetPayCookie()
        {
            PostHttpRequest(payCenterHttpClientHandler, NWTF_fontuserLogin, null, null);
        }
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="param">用户名、密码</param>
        /// <returns></returns>
        public static string PostLogin(string[] param)
        {
            try
            {
                string refreshcmd = "系统正在结账";
                string postContent = PostHttpRequest(payCenterHttpClientHandler, NWTF_fontuserLogin, LoginParamsName, param);
                if (postContent.Contains(refreshcmd))
                {
                    return "303";
                }
                return "400";
            }
            catch
            {
                return "301";
            }

        }
        /// <summary>
        /// 验证充值状态安全
        /// 创建校园卡充值订单
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string CreateCOrder(string[] param)
        {
            PostHttpRequest(payCenterHttpClientHandler, NWTF_checkCPayProject, null, null);
            return ScardCreateOrder(param);
        }
        public static string ScardCreateOrder(string[] param)
        {
            //POST https://pay.ustb.edu.cn/netCreateOrder
            //payAmt=10&factorycode=Z001&payProjectId=105
            //->{"payorderno":"19052922264681784261","returncode":"SUCCESS"
            //->"orderamt":1000,"orderno":"10008190529019604936"
            string[] tmpparam = new string[3];
            param.CopyTo(tmpparam, 0);
            tmpparam[1] = "Z001"; tmpparam[2] = "105";
            string responseContent = PostHttpRequest(payCenterHttpClientHandler, NWTF_scardRechargeCreateOrder, scardRechargeCreateOrderParamsName, tmpparam);
            responseContent = responseContent.Replace("}", "");
            responseContent = responseContent.Replace("{", "");
            responseContent = responseContent.Replace("\"", "");
            string[] responseContentList = responseContent.Split(',');
            Dictionary<string, string> jsonValuePairs = new Dictionary<string, string>();
            foreach (string ojson in responseContentList)
            {
                string[] tmp = ojson.Split(':');
                if (!jsonValuePairs.ContainsKey(tmp[0]))
                    jsonValuePairs.Add(tmp[0], tmp[1]);
            }
            return OnlinePay(jsonValuePairs);
        }
        /// <summary>
        /// 验证支付安全
        /// 流量费
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GetBalance(string username)
        {
            PostHttpRequest(payCenterHttpClientHandler, NWTF_checkPayProject, null, null);
            string balance = BalanceSubString(username);
            return balance;
        }
        /// <summary>
        /// 获取余额
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns></returns>
        private static string BalanceSubString(string username)
        {
            HttpClient httpClient = new HttpClient(payCenterHttpClientHandler)
            {
                Timeout = TimeSpan.FromSeconds(3)
            };
            var getAsync = httpClient.GetAsync(NWTF_netdetails511N004);
            HttpResponseMessage httpResponseMessage = getAsync.Result;
            HttpRequestMessage requestsMessage = httpResponseMessage.RequestMessage;
            if (requestsMessage.RequestUri.AbsoluteUri.Contains(NWTF_netdetails511N004))
            {
                var readAsync = httpResponseMessage.Content.ReadAsStringAsync();
                readAsync.Wait();
                string finalget = readAsync.Result;
                int start = finalget.IndexOf("'" + username);
                int end = finalget.IndexOf('>', start);
                try
                {
                    string infom = finalget.Substring(start, end - start);
                    string[] infomlist = infom.Split('\t');
                    return infomlist[2];
                }
                catch { }
            }
            return "302";
        }
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="param">学号、支付金额</param>
        /// <returns></returns>
        public static string CreateOrder(string[] param)
        {
            //POST https://pay.ustb.edu.cn/netCreateOrder
            //netNo=*******&payAmt=10&netStatus=1&factorycode=N004&payProjectId=1
            //->{"payorderno":"19052922264681784261","returncode":"SUCCESS"
            //->"orderamt":1000,"orderno":"10008190529019604936"
            string[] tmpparam = new string[5];
            param.CopyTo(tmpparam, 0);
            tmpparam[2] = "1"; tmpparam[3] = "N004"; tmpparam[4] = "1";
            string responseContent = PostHttpRequest(payCenterHttpClientHandler, NWTF_netCreateOrder, NetCreateOrderParamsName, tmpparam);
            responseContent = responseContent.Replace("}", "");
            responseContent = responseContent.Replace("{", "");
            responseContent = responseContent.Replace("\"", "");
            string[] responseContentList = responseContent.Split(',');
            Dictionary<string, string> jsonValuePairs = new Dictionary<string, string>();
            foreach (string ojson in responseContentList)
            {
                string[] tmp = ojson.Split(':');
                if (!jsonValuePairs.ContainsKey(tmp[0]))
                    jsonValuePairs.Add(tmp[0], tmp[1]);
            }
            return OnlinePay(jsonValuePairs);
        }
        /// <summary>
        /// 在线支付
        /// </summary>
        /// <param name="jsonValuePairs">支付类型、订单号、支付金额</param>
        /// <returns></returns>
        private static string OnlinePay(Dictionary<string, string> jsonValuePairs)
        {
            //POST https://pay.ustb.edu.cn/onlinePay HTTP/1.1
            //payType=03&orderno=10008190529019604936&orderamt=10.00
            //302-><div id="code" ><img src="data:image/png;base64,***"</img></div>
            jsonValuePairs.TryGetValue("orderno", out string orderno);
            jsonValuePairs.TryGetValue("orderamt", out string orderamt);
            double.TryParse(orderamt, out double tmpOrderamt);
            orderamt = (tmpOrderamt / 100).ToString();
            string[] param = { "03", orderno, orderamt };
            try
            {
                string requests = PostHttpRequest(payCenterHttpClientHandler, NWTF_onlinePay, OnlinePayParamsName, param);
                int start = requests.IndexOf("<div id=\"code\" >");
                int end = requests.IndexOf("</img></div>", start);
                requests = requests.Substring(start, end - start);
                start = requests.IndexOf(",") + 1;
                end = requests.IndexOf("\"", start);
                string QRCode = requests.Substring(start, end - start);
                return QRCode;
            }
            catch { return null; }
        }
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        public static string AuthImage()
        {
            return SaveAsWebImg(payCenterHttpClientHandler, NWTF_authImage, "AuthImage");
        }
    }
    class CNLLogin : WebContents
    {
        //校园网登录参数 CNL:CampusNetworkLogin
        private static readonly string CNL_login = "http://202.204.48.82";
        private static readonly string CNL_drop = "http://202.204.48.82/F.htm";
        private static readonly string[] ParamsName = new string[] { "DDDDD", "upass", "v6ip", "0MKKey" };
        //网络访问进程,登陆校园网单独使用(实际并不需要特意设定静态变量)
        private static readonly HttpClientHandler loginHttpClientHandler = new HttpClientHandler() { UseCookies = true };
        /// <summary>
        /// POST用户名及密码登陆
        /// </summary>
        /// <param name="param">依次为：学号、密码、ipv6地址</param>
        /// <returns></returns>
        public static string Post(string[] param)
        {
            string[] tmpparam = new string[4];
            param.CopyTo(tmpparam, 0);
            tmpparam[3] = "123456789";  //补足0MKKey
            return PostHttpRequest(loginHttpClientHandler, CNL_login, ParamsName, tmpparam);
        }
        /// <summary>
        /// GET登录页面信息
        /// </summary>
        /// <returns></returns>
        public static string Get()
        {
            try
            {
                string finalget = GetHttpRequest(loginHttpClientHandler, CNL_login);
                int start = finalget.IndexOf("time");
                int end = finalget.IndexOf(";//");
                string mes = finalget.Substring(start, end - start);
                return BuildMes(UserValuePairs(mes));
            }
            catch
            {
                return "怎么也找不到呢，是不是网络没连接？";
            }
        }
        /// <summary>
        /// POST方法注销登陆
        /// </summary>
        public static void Drop()
        {
            GetHttpRequest(loginHttpClientHandler, CNL_drop);
        }
        /// <summary>
        /// 取出用户信息，利用字典存储以便读取
        /// </summary>
        /// <param name="userMes"></param>
        /// <returns></returns>
        private static Dictionary<string, string> UserValuePairs(string userMes)
        {
            userMes = userMes.Replace("'", "");
            userMes = userMes.Replace(" ", "");
            string[] slp = userMes.Split(';');
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            foreach (string a in slp)
            {
                string[] tmp = a.Split('=');
                if (tmp.Length == 2)
                    keyValuePairs.Add(tmp[0], tmp[1]);
            }
            return keyValuePairs;
        }
        /// <summary>
        /// 提取用户信息并整合
        /// </summary>
        /// <param name="userValuePairs"></param>
        /// <returns></returns>
        private static string BuildMes(Dictionary<string, string> userValuePairs)
        {
            userValuePairs.TryGetValue("time", out string time);
            userValuePairs.TryGetValue("flow", out string flow);
            userValuePairs.TryGetValue("fee", out string fee);
            userValuePairs.TryGetValue("v6af", out string v6af);
            userValuePairs.TryGetValue("v46m", out string v46m);
            userValuePairs.TryGetValue("v4ip", out string v4ip);
            userValuePairs.TryGetValue("v6ip", out string v6ip);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("登陆成功！\n");
            stringBuilder.Append("连接时长：").Append(time).Append("分\n");
            stringBuilder.Append("流量（V4）：").Append(Math.Round(StringToInt(flow) / 1024.0, 2)).Append("M\n");
            stringBuilder.Append("当前余额：").Append(Math.Round((StringToInt(fee) - StringToInt(fee) % 100) / 10000.0, 2)).Append("元\n");
            if (v46m.Equals("0"))
            {
                stringBuilder.Append("联网模式：仅IPV4模式\n");
                stringBuilder.Append("IPV4地址：").Append(v4ip);
            }
            else
            {
                stringBuilder.Append("流量（V6）：").Append(Math.Round(StringToInt(v6af) / 4096.0, 2)).Append("M\n");
                stringBuilder.Append("IPV4地址：").Append(v4ip).Append("\n");
                stringBuilder.Append("IPV6地址：\n").Append(v6ip).Append("\n");
            }
            return stringBuilder.ToString();
        }
    }
}