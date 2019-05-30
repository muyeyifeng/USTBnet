using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Windows.Controls;
using System.Windows.Media;

namespace USTBnet
{
    class HttpCommands
    {
        /// <summary>
        /// 电子支付网网络缴费的交互顺序是
        /// POS PAYLOGIN页面获取Cookies
        /// POST用户名密码至PAYLOGIN
        /// POST PAYAUTH
        /// GET PAYNET
        /// </summary>
        private const string PAYLOGIN = "https://pay.ustb.edu.cn/fontuserLogin";
        private const string PAYAUTH = "https://pay.ustb.edu.cn/publicPayPro!checkPayProjectCouldPay.do?payProjectId=1 ";
        private const string PAYNET = "https://pay.ustb.edu.cn/netdetails511N004";
        private const string PAYCreateOrder = "https://pay.ustb.edu.cn/netCreateOrder";
        private const string PAYonlinePay = "https://pay.ustb.edu.cn/onlinePay";
        private static HttpClientHandler payHttpClientHandler = new HttpClientHandler() { UseCookies = true };
        private static HttpClientHandler loginHttpClientHandler = new HttpClientHandler() { UseCookies = true };
        /// <summary>
        /// 向目标地址发送无参Get请求，并将请求数据转换为字符串
        /// 仅对校园网登录页面有返回值
        /// </summary>
        /// <param name="url">主机地址</param>
        /// <returns></returns>
        public static string Get(string url)
        {
            try
            {
                HttpClient httpClient = new HttpClient(loginHttpClientHandler);
                httpClient.Timeout = TimeSpan.FromSeconds(3);
                string finalget = httpClient.GetStringAsync(url).Result;
                int start = finalget.IndexOf("<!--");
                int end = finalget.IndexOf(";//");
                string mes = finalget.Substring(start, end - start);
                string[] values = mes.Split(';');
                long[] intValues = new long[4];
                string[] stringValues = new string[3];
                for (int i = 0, j = 0, k = 0; i < values.Length; i++)
                {
                    string value = values[i];
                    if (value.Contains("time") || value.Contains("flow") || value.Contains("fee") || value.Contains("v6af"))
                    {
                        intValues[j++] = StringToInt(value.Split('=')[1]);
                    }
                    else if (value.Contains("v46m") || value.Contains("v4ip") || value.Contains("v6ip"))
                    {
                        stringValues[k++] = value.Split('=')[1].Replace("'", "");
                    }
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("登陆成功！\n");
                stringBuilder.Append("连接时长：").Append(intValues[0]).Append("分\n");
                stringBuilder.Append("流量（V4）：").Append(Math.Round(intValues[1] / 1024.0, 2)).Append("M\n");
                stringBuilder.Append("当前余额：").Append(Math.Round((intValues[2] - intValues[2] % 100) / 10000.0, 2)).Append("元\n");
                if (stringValues[0].Equals("0"))
                {
                    stringBuilder.Append("联网模式：仅IPV4模式\n");
                    stringBuilder.Append("IPV4地址：").Append(stringValues[1]);
                }
                else
                {
                    stringBuilder.Append("流量（V6）：").Append(Math.Round(intValues[3] / 4096.0, 2)).Append("M\n");
                    stringBuilder.Append("IPV4地址：").Append(stringValues[1]).Append("\n");
                    stringBuilder.Append("IPV6地址：\n").Append(stringValues[2]).Append("\n");
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return "怎么也找不到呢，是不是网络没连接？";
            }
        }
        /// <summary>
        /// 向主机地址POST数据，返回StatusCode
        /// </summary>
        /// <param name="url">主机地址</param>
        /// <param name="param">传递参数</param>
        public static int Post(string url, string[] param)
        {
            string username = param[0];
            string passwd = param[1];
            string v6ip = param[2];
            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "DDDDD", username},
                { "upass", passwd},
                { "v6ip",v6ip},
                {"0MKKey","123456789" }
            });
            string statusCode = "";
            try
            {
                HttpClient httpClient = new HttpClient(loginHttpClientHandler);
                httpClient.Timeout = TimeSpan.FromSeconds(3);
                httpClient.PostAsync(url, content).Result.StatusCode.ToString();
                statusCode = "200";
            }
            catch
            {
                statusCode = "102";
            }
            int statusInt = (int)StringToInt(statusCode);
            return statusInt;
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
                return "";
        }
        /// <summary>
        /// 向主机地址POST方法发送数据，获取Cookies用于用户识别
        /// 仅对电子支付网有效
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static void GetPayCookie()
        {
            HttpClient httpClient = new HttpClient(payHttpClientHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(1);
            httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            HttpResponseMessage httpResponseMessage = httpClient.PostAsync(PAYLOGIN, new FormUrlEncodedContent(new Dictionary<string, string>())).Result;
        }
        /// <summary>
        /// 登陆电子支付网站并跳转到指定页面
        /// 以网络流量费页面调试。
        /// 301 网络错误;302 重定向;balance 余额
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string PayPostLogin(string[] param)
        {
            try
            {
                string username = param[0];
                string passwd = param[1];
                string checkcode = param[2];
                var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "nickName", username},
                    { "password", passwd},
                    { "checkCode",checkcode}
                });

                HttpClient httpClient = new HttpClient(payHttpClientHandler);
                httpClient.Timeout = TimeSpan.FromSeconds(3);
                string refreshcmd = "系统正在结账";
                HttpResponseMessage httpResponseMessage = httpClient.PostAsync(PAYLOGIN, content).Result;
                string postContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
                if (postContent.Contains(refreshcmd))
                {
                    return "303";
                }
                httpResponseMessage = httpClient.PostAsync(PAYAUTH, null).Result;
                string balance = PayGet(PAYNET, username);
                return balance;
            }
            catch
            {
                return "301";
            }

        }
        /// <summary>
        /// 获取网络流量费缴费页面信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string PayGet(string url, string username)
        {
            HttpClient httpClient = new HttpClient(payHttpClientHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(3);
            HttpResponseMessage httpResponseMessage = httpClient.GetAsync(url).Result;
            HttpRequestMessage requestsMessage = httpResponseMessage.RequestMessage;
            if (requestsMessage.RequestUri.AbsoluteUri.Contains(url))
            {
                string statusCode = httpResponseMessage.StatusCode.ToString();
                string finalget = httpResponseMessage.Content.ReadAsStringAsync().Result;
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
        /// 发送支付信息
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string PayPost(string[] param)
        {
            //POST https://pay.ustb.edu.cn/netCreateOrder
            //netNo=41603382&payAmt=10&netStatus=1&factorycode=N004&payProjectId=1
            //->{"payorderno":"19052922264681784261","returncode":"SUCCESS"
            //->"orderamt":1000,"orderno":"10008190529019604936"
            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "netNo", param[0]},
                { "payAmt", param[1]},
                { "netStatus","1"},
                { "factorycode","N004" },
                { "payProjectId","1"}
            });
            HttpClient createOrderHttpClient = new HttpClient(payHttpClientHandler);
            createOrderHttpClient.Timeout = TimeSpan.FromSeconds(3);
            HttpResponseMessage httpResponseMessage = createOrderHttpClient.PostAsync(PAYCreateOrder, content).Result;
            string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
            responseContent = responseContent.Replace("}", "");
            responseContent = responseContent.Replace("{", "");
            string[] responseContentList = responseContent.Split(',');
            List<string[]> json = new List<string[]>();
            foreach (string ojson in responseContentList)
            {
                json.Add(ojson.Replace("\"", "").Split(':'));
            }

            //POST https://pay.ustb.edu.cn/onlinePay HTTP/1.1
            //payType=03&orderno=10008190529019604936&orderamt=10.00
            //302-><div id="code" ><img src="data:image/png;base64,***"</img></div>
            string orderno = "";
            double orderamt = 0;
            foreach (string[] mes in json)
            {
                if (mes[0].Equals("orderno"))
                    orderno = mes[1];
                if (mes[0].Equals("orderamt"))
                {
                    double.TryParse(mes[1], out orderamt);
                    orderamt /= 100.0;
                }
            }
            content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "payType", "03"},
                { "orderno", orderno },
                { "orderamt",orderamt.ToString()},
            });
            HttpClient onlinePayHttpClient = new HttpClient(payHttpClientHandler);
            onlinePayHttpClient.Timeout = TimeSpan.FromSeconds(3);
            httpResponseMessage = onlinePayHttpClient.PostAsync(PAYonlinePay, content).Result;
            HttpRequestMessage requestsMessage = httpResponseMessage.RequestMessage;
            if (requestsMessage.RequestUri.AbsoluteUri.Contains("weiXinQRCode"))
            {
                try
                {
                    string requests = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    int start = requests.IndexOf("<div id=\"code\" >");
                    int end = requests.IndexOf("</img></div>", start);
                    requests = requests.Substring(start, end - start);
                    start = requests.IndexOf(",") + 1;
                    end = requests.IndexOf("\"", start);
                    string QRCode = requests.Substring(start, end - start);
                    return QRCode;
                }
                catch { }
            }
            return "200";
        }
        /// <summary>
        /// 下载电子支付网站图片，返回图片名
        /// </summary>
        /// <param name="picUrl"></param>
        /// <returns></returns>
        public static string SaveAsWebImg(string picUrl)
        {
            string result = "";
            try
            {
                if (!String.IsNullOrEmpty(picUrl))
                {
                    HttpClient getAuthImage = new HttpClient(payHttpClientHandler);
                    getAuthImage.Timeout = TimeSpan.FromSeconds(1);
                    var inputStream = getAuthImage.GetStreamAsync(picUrl);
                    Stream imageStream = getAuthImage.GetStreamAsync(picUrl).Result;

                    Random rd = new Random();
                    DateTime nowTime = DateTime.Now;
                    string fileName = "AuthImage" + nowTime.Minute.ToString() + nowTime.Second.ToString() + rd.Next(1000, 1000000) + ".png";
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
            catch { }
            return result;
        }
        /// <summary>
        /// 将字符串转换为十进制整数，字符串中的非数字会被忽略
        /// </summary>
        /// <param name="numstr">数字字符串</param>
        /// <returns></returns>
        private static long StringToInt(string numstr)
        {
            long result = 0;
            foreach (char chr in numstr)
            {
                if (chr >= '0' && chr <= '9')
                    result = result * 10 + (chr - '0');
            }
            return result;
        }
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
    }
}
