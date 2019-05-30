using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace USTBnet
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string LOGIN = "http://202.204.48.82";
        private const string DROP = "http://202.204.48.82/F.htm";
        private const string PAYIMAGE = "https://pay.ustb.edu.cn/authImage";
        private string appid = "ustb_muyeyfieng";
        private string _userid = "", _passwd = "", _v6ip = "";
        private const string LoginInfo = "infom.sav";
        private const string PayInfo = "payinfom.sav";
        private List<string> authImagePath = new List<string>();
        /// <summary>
        /// 窗体载入入口
        /// </summary>
        public MainWindow()
        {
            DeleteAuthImage();
            InitializeComponent();
            LoadLogin();
            LoadPay();
            RefreshAuthImage();
        }
        /**********************************************************************************************/
        /*************************************以下为控件事件所调函数***********************************/
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            int status = Login();
            if (status == 200)
            {
                userInfoTab.Visibility = Visibility.Visible;
                WriteFile(_userid, _passwd, RandomAESKey(), LoginInfo);
                ShowUserInfom(HttpCommands.Get(LOGIN));
            }
        }
        private void Drop_Click(object sender, RoutedEventArgs e)
        {
            int status = DropNet();
            switch (status)
            {
                case 301:
                    MessageBox.Show("网络错误！");
                    break;
                case 302:
                    break;
                case 400:
                    MessageBox.Show("注销成功！");
                    userInfoTab.Visibility = Visibility.Hidden;
                    userInfoText.Text = "";
                    break;
                default:
                    break;
            }
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            userid.Text = "";
            passwd.Password = "";
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void PasswdVisibility_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeImage(true, passwdVisibility, passwd, showPasswd);
        }
        private void PasswdVisibility_PreviewMouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChangeImage(false, passwdVisibility, passwd, showPasswd);

        }
        private void PaypasswdVisibility_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeImage(true, paypasswdVisibility, paypasswd, payShowPasswd);
        }
        private void PaypasswdVisibility_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeImage(false, paypasswdVisibility, paypasswd, payShowPasswd);
        }
        private void UserInfoRefresh_Click(object sender, RoutedEventArgs e)
        {
            userInfoText.Text = HttpCommands.Get(LOGIN);
        }
        private void PayLogin_Click(object sender, RoutedEventArgs e)
        {
            PayLogin();
        }
        private void AuthImageBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshAuthImage();
        }
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            ShowQRCode();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ChangePayControl("", false);
        }
        /**********************************************************************************************/
        /***********************************以下为校园网登陆所调函数***********************************/
        /// <summary>
        /// 登录页面默认加载模块
        /// </summary>
        private void LoadLogin()
        {
            string[] userinfo = ReadFile(LoginInfo);
            Boolean hasinfo = true;
            foreach (string info in userinfo)
            {
                if (info.Length == 0)
                    hasinfo = false;
            }
            if (hasinfo)
            {
                userid.Text = userinfo[0];
                passwd.Password = userinfo[1];
            }
            userInfoText.Text = HttpCommands.Get(LOGIN);
        }
        /// <summary>
        /// 返回值代码：
        /// 100 超时 101 用户名，密码不规范 102 网络连接状态错误 103 未知错误
        /// 200 登陆成功
        /// </summary>
        /// <returns></returns>
        private int Login()
        {
            _userid = userid.Text;
            _passwd = passwd.Password;
            _v6ip = HttpCommands.GetIPV6();
            int statusCode = 0;
            if (_userid != "" && _passwd != "")
            {
                statusCode = HttpCommands.Post(LOGIN, new string[] { _userid, _passwd, _v6ip });
            }
            else
            {
                statusCode = 101;
                MessageBox.Show("输入不能为空！");
            }
            return statusCode;
        }
        /// <summary>
        /// 返回值代码：
        /// 300 超时 301 网络状态错误 302 未知错误
        /// 400 登出成功 
        /// </summary>
        /// <returns></returns>
        private int DropNet()
        {
            string get = HttpCommands.Get(DROP);
            if (get.Length > 0)
            {
                return 400;
            }
            return 301;
        }
        /// <summary>
        /// 登陆成功后显示账户信息
        /// </summary>
        private void ShowUserInfom(string get)
        {
            userInfoText.Text = get;
            MessageBox.Show(get);
        }
        /// <summary>
        /// 图标更改及显示密码
        /// </summary>
        /// <param name="visability"></param>
        private void ChangeImage(Boolean visability, Image image, PasswordBox passwordBox, TextBox showBox)
        {
            string path = visability ? "icon/eye.png" : "icon/eye_no.png";
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(path, UriKind.Relative));
            image.Source = imageBrush.ImageSource;
            if (visability)
            {
                showBox.Visibility = Visibility.Visible;
                showBox.Text = passwordBox.Password;
            }
            else
            {
                showBox.Visibility = Visibility.Hidden;
                showBox.Text = "";
            }

        }
        /**********************************************************************************************/
        /*************************************以下为充值界面所调函数***********************************/
        /// <summary>
        /// 网络账户充值页面默认加载模块
        /// </summary>
        private void LoadPay()
        {
            HttpCommands.GetPayCookie();
            string[] payinfo = ReadFile(PayInfo);
            Boolean hasinfo = true;
            foreach (string info in payinfo)
            {
                if (info.Length == 0)
                    hasinfo = false;
            }
            if (hasinfo)
            {
                payuserid.Text = payinfo[0];
                paypasswd.Password = payinfo[1];
            }
        }
        /// <summary>
        /// 电子支付网站登录，返回Cookies,不做状态校验。
        /// </summary>
        /// <returns></returns>
        private void PayLogin()
        {
            string _payuserid = payuserid.Text;
            string _paypasswd = paypasswd.Password;
            string _checkcode = payauth.Text;
            string balance;
            if (_payuserid != "" && _paypasswd != "" && _checkcode != "")
            {
                balance = HttpCommands.PayPostLogin(new string[] { _payuserid, _paypasswd, _checkcode });
                if (balance == "302")
                {
                    MessageBox.Show("信息错误，请检查！");
                    RefreshAuthImage();
                }
                else if (balance == "301")
                {
                    MessageBox.Show("未知错误，请重试！");
                    RefreshAuthImage();
                }
                else if (balance == "303")
                {
                    string mes = "系统开放时间：凌晨00:15至晚间23:20\n但是根据经验，开放时间远晚于00:15";
                    timeOut.Visibility = Visibility.Visible;
                    timeOut.Text = mes;
                    MessageBox.Show(mes);
                }
                else
                {
                    WriteFile(payuserid.Text, paypasswd.Password, RandomAESKey(), PayInfo);
                    RefreshAuthImage();
                    ChangePayControl(balance, true);
                }
            }
            else
            {
                MessageBox.Show("输入不能为空！");
                RefreshAuthImage();
            }
        }
        /// <summary>
        /// 显示微信支付QR码
        /// </summary>
        private void ShowQRCode()
        {
            double reChargeDouble;
            if (double.TryParse(reCharge.Text, out reChargeDouble))
            {
                string status = HttpCommands.PayPost(new string[] { payuserid.Text, reCharge.Text });
                //string status = "";
                if (status.Equals("200"))
                {
                    MessageBox.Show("Error");
                }
                else
                {
                    string qRCodepath = HttpCommands.Base64StringToImage(status);
                    Grid grid = new Grid();
                    
                    ImageBrush imageBrush = new ImageBrush();
                    imageBrush.ImageSource = new BitmapImage(new Uri(qRCodepath, UriKind.Relative));
                    Image image = new Image();
                    image.Source = imageBrush.ImageSource;
                    image.HorizontalAlignment = HorizontalAlignment.Center;
                    image.VerticalAlignment = VerticalAlignment.Top;
                    image.Height = 260;
                    image.Width = 260;

                    Label label = new Label();
                    label.Content = "扫码支付后关闭窗口\n2-5分钟后更新余额";
                    label.Height = 50;
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.VerticalAlignment = VerticalAlignment.Bottom;

                    grid.Children.Add(image);
                    grid.Children.Add(label);

                    Window window = new Window();
                    window.Content = grid;
                    window.Title = "微信扫码支付";
                    window.Height = 350;
                    window.Width = 300;
                    window.ResizeMode = ResizeMode.NoResize;
                    window.ShowDialog();
                    reCharge.Text = "";
                    ChangePayControl("", false);
                    RefreshAuthImage();
                }
            }
            else
            {
                MessageBox.Show("请输入正确的金额！");
            }
        }
        /// <summary>
        /// 成功获取网络流量费缴费页面信息
        /// </summary>
        /// <param name="balance"></param>
        private void ChangePayControl(string balance, Boolean visibility)
        {
            showBalanceText.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
            showBalance.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
            reChargeText.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
            reCharge.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
            submit.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
            back.Visibility = visibility ? Visibility.Visible : Visibility.Hidden;
            showBalance.Content = balance;

            paypwd.Visibility = !visibility ? Visibility.Visible : Visibility.Hidden;
            paypasswd.Visibility = !visibility ? Visibility.Visible : Visibility.Hidden;
            payauth.Visibility = !visibility ? Visibility.Visible : Visibility.Hidden;
            authImage.Visibility= !visibility ? Visibility.Visible : Visibility.Hidden;
            authImageBtn.Visibility = !visibility ? Visibility.Visible : Visibility.Hidden;
            payLogin.Visibility = !visibility ? Visibility.Visible : Visibility.Hidden;
            payExit.Visibility = !visibility ? Visibility.Visible : Visibility.Hidden;
            paypasswdVisibility.Visibility = !visibility ? Visibility.Visible : Visibility.Hidden;
            payShowPasswd.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 刷新验证码
        /// </summary>
        private void RefreshAuthImage()
        {
            string path = HttpCommands.SaveAsWebImg(PAYIMAGE);
            ImageBrush imageBrush = new ImageBrush();
            if (path.Length > 0)
            {
                imageBrush.ImageSource = new BitmapImage(new Uri(path, UriKind.Relative));
                authImageBtn.Background = imageBrush;
                authImage.Visibility = Visibility.Hidden;
                authImageBtn.Visibility = Visibility.Visible;
            }
            else
            {
                authImage.Source = new BitmapImage(new Uri("icon/error.png", UriKind.Relative));
                authImage.Visibility = Visibility.Visible;
                authImageBtn.Visibility = Visibility.Hidden;
            }
        }
        /**********************************************************************************************/
        /***************************************以下为通用调用函数*************************************/
        /// <summary>
        /// 读取配置文件,若不存在配置文件，则新建一个配置文件并随机生成AES密钥
        /// </summary>
        /// <returns></returns>
        private string[] ReadFile(string filePath)
        {
            string _userid = "";
            string _passwd = "";
            string AESKey;
            string readfile;
            try
            {
                readfile = File.ReadAllText(filePath);
            }
            catch
            {
                File.WriteAllText(filePath, "");
                readfile = File.ReadAllText(filePath);
            }
            if (readfile.Length > 43)
            {
                AESKey = readfile.Substring(0, 43);
                string encodeInfo = readfile.Substring(43);
                string decodeInfo = Cryptography.AES_decrypt(encodeInfo, AESKey, ref appid);
                _userid = decodeInfo.Substring(0, decodeInfo.IndexOf(':'));
                _passwd = decodeInfo.Substring(decodeInfo.IndexOf(':') + 1);
            }
            return new string[] { _userid, _passwd };
        }
        /// <summary>
        /// 写入/更新配置文件
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="passwd"></param>
        /// <returns></returns>
        private Boolean WriteFile(string _userid, string _passwd, string AESKey, string filePath)
        {
            string encodeInfo = Cryptography.AES_encrypt(_userid + ":" + _passwd, AESKey, appid);
            File.WriteAllText(filePath, AESKey + encodeInfo);
            return true;
        }
        /// <summary>
        /// 随机生成AES密钥，该方法仅在必要写入（如更新登录信息）时使用
        /// </summary>
        /// <returns></returns>
        private string RandomAESKey()
        {
            Random random = new Random();
            char[] code = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 43; i++)
            {
                stringBuilder.Append(code[random.Next(code.Length)]);
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 删除上次运行产生的png图片
        /// </summary>
        private void DeleteAuthImage()
        {
            string rootPath = Directory.GetCurrentDirectory();
            DirectoryInfo root = new DirectoryInfo(rootPath);
            foreach (FileInfo file in root.GetFiles())
            {
                if ((file.Name.Contains("AuthImage")|| file.Name.Contains("QRCODE")) && file.Name.Contains(".png"))
                {
                    authImagePath.Add(file.Name);
                }
            }
            foreach (string path in authImagePath)
            {
                File.Delete(path);
            }
        }
    }
}
