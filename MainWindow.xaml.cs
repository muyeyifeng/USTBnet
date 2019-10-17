using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace USTBnet
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private int type;
        private string appid = "ustb_muyeyfieng";
        private bool f_exit = false;
        private string _userid = "", _passwd = "", _v6ip = "";
        private const string LoginInfo = "C:\\Users\\Public\\Documents\\USTBnet\\infom.sav";
        private const string PayInfo = "C:\\Users\\Public\\Documents\\USTBnet\\payinfom.sav";
        private List<string> authImagePath = new List<string>();
        /// <summary>
        /// 窗体载入入口
        /// </summary>
        public MainWindow()
        {
            Closing += new CancelEventHandler(Form_Closing);
            DeleteAuthImage();
            InitializeComponent();
            SoftInformation();
            try
            {
                LoadLogin(out bool flag);
                LoadPay();
                RefreshAuthImage();
                if (flag)
                {
                    Login();
                    Close();
                }
                InitIconFrom();
                //MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch
            {

            }
        }
        /**********************************************************************************************/
        /*************************************以下为控件事件所调函数***********************************/
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }
        private void Drop_Click(object sender, RoutedEventArgs e)
        {
            DropNet();
            //MessageBox.Show("注销成功！");
            //userInfoTab.Visibility = Visibility.Hidden;
            userInfoText.Text = "";

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
        private void PasswdVisibility_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChangeImage(true, passwdVisibility, passwd, showPasswd);
        }
        private void PasswdVisibility_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChangeImage(false, passwdVisibility, passwd, showPasswd);
        }
        private void PaypasswdVisibility_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChangeImage(true, paypasswdVisibility, paypasswd, payShowPasswd);
        }
        private void PaypasswdVisibility_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ChangeImage(false, paypasswdVisibility, paypasswd, payShowPasswd);
        }
        private void UserInfoRefresh_Click(object sender, RoutedEventArgs e)
        {
            userInfoText.Text = CNLLogin.Get();
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
        private void LoadLogin(out bool hasinfo)
        {
            string[] userinfo = ReadFile(LoginInfo);
            hasinfo = true;
            foreach (string info in userinfo)
            {
                if (info.Length == 0)
                    hasinfo = false;
            }
            if (hasinfo)
            {
                userid.Text = userinfo[0];
                passwd.Password = userinfo[1];
                AutoLogin.IsChecked = userinfo[2].Equals("True") ? true : false;
            }
            hasinfo = hasinfo && AutoLogin.IsChecked.Value;
            userInfoText.Text = CNLLogin.Get();
        }
        /// <summary>
        /// 返回值代码：
        /// 100 超时 101 用户名，密码不规范 102 网络连接状态错误 103 未知错误
        /// 200 登陆成功
        /// </summary>
        /// <returns></returns>
        private void Login()
        {
            _userid = userid.Text;
            _passwd = passwd.Password;
            _v6ip = CNLLogin.GetIPV6();
            int statusCode = 0;
            if (_userid != "" && _passwd != "")
            {
                if (CNLLogin.Post(new string[] { _userid, _passwd, _v6ip }) != null)
                    statusCode = 200;
            }
            else
            {
                statusCode = 101;
                MessageBox.Show("输入不能为空！");
            }
            if (statusCode == 200)
            {
                userInfoTab.Visibility = Visibility.Visible;
                WriteFile(_userid, _passwd, AutoLogin.IsChecked.Value.ToString(), RandomAESKey(), LoginInfo);
                userInfoText.Text = CNLLogin.Get();
                ShowUserInfom();
            }
        }
        /// <summary>
        /// 注销登录
        /// </summary>
        /// <returns></returns>
        private void DropNet()
        {
            CNLLogin.Drop();
            NotifyIcon notifyIcon = new NotifyIcon
            {
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipTitle = "消息通知",
                BalloonTipText = "注销成功！"
            };
            notifyIcon.Visible = true;
            notifyIcon.Icon = Properties.Resources.login;
            notifyIcon.ShowBalloonTip(1000);
            notifyIcon.Dispose();
        }
        /// <summary>
        /// 登陆成功后显示账户信息
        /// </summary>
        private void ShowUserInfom()
        {
            NotifyIcon notifyIcon = new NotifyIcon
            {
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipTitle = "账户信息",
                BalloonTipText = CNLLogin.Get()
            };
            notifyIcon.Visible = true;
            notifyIcon.Icon = Properties.Resources.login;
            notifyIcon.ShowBalloonTip(1000);
            notifyIcon.Dispose();
        }
        /// <summary>
        /// 图标更改及显示密码
        /// </summary>
        /// <param name="visability"></param>
        private void ChangeImage(bool visability, Image image, PasswordBox passwordBox, TextBox showBox)
        {
            string path = visability ? "icon/eye.png" : "icon/eye_no.png";
            ImageBrush imageBrush = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(path, UriKind.Relative))
            };
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
            Pay.GetPayCookie();
            string[] payinfo = ReadFile(PayInfo);
            bool hasinfo = true;
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
            if (string.IsNullOrEmpty(_payuserid) && string.IsNullOrEmpty(_paypasswd) && string.IsNullOrEmpty(_checkcode))
            {
                string status = Pay.PostLogin(new string[] { _payuserid, _paypasswd, _checkcode });
                if (status == "301")
                {
                    MessageBox.Show("未知错误，请重试！");
                    RefreshAuthImage();
                }
                else if (status == "303")
                {
                    string mes = "系统开放时间：凌晨00:15至晚间23:20\n但是根据经验，开放时间远晚于00:15";
                    timeOut.Visibility = Visibility.Visible;
                    timeOut.Text = mes;
                    MessageBox.Show(mes);
                }
                else if (status == "400")
                {
                    type = new Choice().GetChoice();
                    if (type == 1)
                    {
                        string balance = Pay.GetBalance(_payuserid);
                        if (balance == "302")
                        {
                            MessageBox.Show("信息错误，请检查！");
                            RefreshAuthImage();
                        }
                        else
                        {
                            WriteFile(payuserid.Text, paypasswd.Password, "0", RandomAESKey(), PayInfo);
                            RefreshAuthImage();
                            ChangePayControl(balance, true);
                        }
                    }
                    else if (type == -1)
                    {
                        WriteFile(payuserid.Text, paypasswd.Password, "0", RandomAESKey(), PayInfo);
                        RefreshAuthImage();
                        ChangePayControl("不可获取", true);
                    }
                    else
                    {
                        RefreshAuthImage();
                    }
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
            if (double.TryParse(reCharge.Text, out double reChargeDouble))
            {
                string status;
                if (type == 1)
                    status = Pay.CreateOrder(new string[] { payuserid.Text, reChargeDouble.ToString() });
                else
                    status = Pay.CreateCOrder(new string[] { reChargeDouble.ToString() });
                //string status = "";
                if (status == null)
                {
                    MessageBox.Show("Error");
                }
                else
                {
                    string qRCodepath = Pay.Base64StringToImage(status);
                    new QRImg(qRCodepath, type == 1);
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
        private void ChangePayControl(string balance, bool visibility)
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
            authImage.Visibility = !visibility ? Visibility.Visible : Visibility.Hidden;
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
            string path = Pay.AuthImage();
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
            string[] userinfo = null;
            string AESKey;
            string readfile;
            try
            {
                readfile = File.ReadAllText(filePath);
            }
            catch
            {
                if (!Directory.Exists("C:\\Users\\Public\\Documents\\USTBnet\\"))
                    Directory.CreateDirectory("C:\\Users\\Public\\Documents\\USTBnet\\");
                //File.WriteAllText(filePath, "");
                readfile = File.ReadAllText(filePath);
            }
            if (readfile.Length > 43)
            {
                AESKey = readfile.Substring(0, 43);
                string encodeInfo = readfile.Substring(43);
                string decodeInfo = Cryptography.AES_decrypt(encodeInfo, AESKey, ref appid);
                userinfo = decodeInfo.Split(':');
                if (userinfo.Length == 2)
                {
                    userinfo = new string[] { userinfo[0], userinfo[1], "False" };
                }
            }
            if (userinfo == null)
            {
                userinfo = new string[] { "" };
            }
            return userinfo;
        }
        /// <summary>
        /// 写入/更新配置文件
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="passwd"></param>
        /// <returns></returns>
        private bool WriteFile(string _userid, string _passwd, string _autologin, string AESKey, string filePath)
        {
            string encodeInfo = Cryptography.AES_encrypt(_userid + ":" + _passwd + ":" + _autologin, AESKey, appid);
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
        /// 程序结束前操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            WriteFile(userid.Text, passwd.Password, AutoLogin.IsChecked.Value.ToString(), RandomAESKey(), LoginInfo);
            WriteFile(payuserid.Text, paypasswd.Password, "0", RandomAESKey(), PayInfo);
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
                if ((file.Name.Contains("AuthImage") || file.Name.Contains("QRCODE")) && file.Name.Contains(".png"))
                {
                    authImagePath.Add(file.Name);
                }
            }
            foreach (string path in authImagePath)
            {
                File.Delete(path);
            }
        }
        /// <summary>
        /// 显示软件信息
        /// </summary>
        private void SoftInformation()
        {
            try
            {
                string version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                StringBuilder info = new StringBuilder();
                info.Append("制作人：muyeyifeng\n");
                info.Append("版本：").Append(version).Append("\n");
                info.Append("联系方式：master@muyeyifeng.site");
                Information.Content = info.ToString();
            }
            catch
            {

            }
        }
        /// <summary>
        /// 通知栏显示图标
        /// </summary>
        private void InitIconFrom()
        {
            NotifyIcon notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.login,
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipTitle = "账户信息",
                BalloonTipText = userInfoText.Text
            };
            notifyIcon.Visible = true;
            notifyIcon.MouseDown += new MouseEventHandler(NotifyIcon_MouseDown);

        }
        /// <summary>
        /// 重写Closing函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_Closing(object sender, CancelEventArgs e)
        {
            Hide();
            if (!f_exit)
                e.Cancel = true;
        }
        /// <summary>
        /// 重写单击鼠标函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!IsVisible)
                {
                    Show();
                    Activate();
                }
                else
                {
                    Close();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                string isStartUp = CreatLnkInStartup.StartPathIsExists() ? "取消开机启动" : "设置开启启动";
                ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                contextMenuStrip.Items.Add("退出", null, new EventHandler(Exit));
                contextMenuStrip.Items.Add(isStartUp, null, new EventHandler(ChangeStartUp));
                contextMenuStrip.Show(System.Windows.Forms.Control.MousePosition);

            }
        }

        /// <summary>
        /// 退出事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit(object sender, EventArgs e)
        {
            f_exit = true;
            this.Close();
        }
        private void ChangeStartUp(object sender, EventArgs e)
        {
            string isStartUp = CreatLnkInStartup.StartPathIsExists() ? "取消" : "设置";
            if (MessageBox.Show("是否"+isStartUp+"开机自启：", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (CreatLnkInStartup.StartPathIsExists())
                    CreatLnkInStartup.RemoveStartUp();
                else
                    CreatLnkInStartup.CopyToStartUp();
            }
        }
    }
}
/// <summary>
/// 创建选择窗口
/// </summary>
class Choice : Window
{
    private int whichOne = 0;
    public Choice()
    {
        Grid grid = new Grid();

        Label choice = new Label
        {
            Content = "请选择充值项目：",
            Height = 50,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top
        };

        Button nWTF = new Button
        {
            Content = "网络流量费充值",
            Height = 25,
            Width = 90
        };
        Thickness nWTFthickness = new Thickness(10, 25, 10, 10);
        nWTF.Margin = nWTFthickness;
        nWTF.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        nWTF.Click += new RoutedEventHandler(NetWorkTrafficFee);

        Button cCF = new Button
        {
            Content = "校园卡充值",
            Height = 25,
            Width = 90
        };
        Thickness cCFthickness = new Thickness(10, 25, 10, 10);
        cCF.Margin = cCFthickness;
        cCF.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        cCF.Click += new RoutedEventHandler(CampusCardFee);

        grid.Children.Add(choice);
        grid.Children.Add(nWTF);
        grid.Children.Add(cCF);

        Content = grid;
        Title = "充值选择";
        Height = 100;
        Width = 230;
        ResizeMode = ResizeMode.NoResize;
        ShowDialog();
    }
    /// <summary>
    /// 选择网络流量费充值
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NetWorkTrafficFee(object sender, RoutedEventArgs e)
    {
        whichOne = 1;
        Close();
    }
    /// <summary>
    /// 选择校园卡账户充值
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CampusCardFee(object sender, RoutedEventArgs e)
    {
        whichOne = -1;
        Close();
    }
    /// <summary>
    /// 获取选择状态
    /// </summary>
    /// <returns></returns>
    public int GetChoice()
    {
        return whichOne;
    }
}
/// <summary>
/// 创建QR码窗口
/// </summary>
class QRImg : Window
{
    public QRImg(string qRCodepath, bool type)
    {
        Grid grid = new Grid();

        ImageBrush imageBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri(qRCodepath, UriKind.Relative))
        };
        Image image = new Image
        {
            Source = imageBrush.ImageSource,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Height = 260,
            Width = 260
        };

        Label label = new Label
        {
            Content = type ? "扫码支付后关闭窗口\n2-5分钟后更新余额" : "扫码支付后关闭窗口",
            Height = 50,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom
        };

        grid.Children.Add(image);
        grid.Children.Add(label);

        Content = grid;
        Title = "微信扫码支付";
        Height = 350;
        Width = 300;
        ResizeMode = ResizeMode.NoResize;
        ShowDialog();
    }
}