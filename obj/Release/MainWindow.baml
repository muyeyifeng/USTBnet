   M S B A M L   `   `   `  ���� B  >USTBnet, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:USTBnetUSTBnet      USTBnet.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     'localclr-namespace:USTBnet     -     � Title$  校园网登录程序��$	��300��$	��200�� � 
ResizeMode$ NoResize=�.����  �� TabStripPlacement$	 Top=�.@��� $��登陆页面��$��70��.��� -   $��	LoginGrid��.+��� -   $
��uMes��$��学号/工号��$	��Top=�$
��Left=�$��Center=�$��20,0,0,0q�$��25��$��70���� -   $��userid��$	��Top=�$��25��$	��150��{�. $��Center=�$��Stretch=�$��80,0,0,0q��� -   $
��pMes��$��密码��$	��Top=�$
��Left=�$��Center=�$��	20,30,0,0q�$��25��$��70��G� -   $��passwd��$	��Top=�$��25��$	��150��{�. $��Center=�$
��Left=�$��Stretch=�$��	80,30,0,0q��� -   $��passwdVisibility��$��icon/eye_no.png��$��15��$��15��$	��Top=�$��Right=�$��0,35,5,0q��� -   $��LoginButton��$��登录/刷新��$��75��$	��Top=�$
��Left=�$��	50,70,0,0q��� -	   $��
DropButton��$��注销��$��75��$	��Top=�$��Right=�$��	0,70,50,0q��� -
   $��ClearButton��$��清除��$��75��$	��Top=�$
��Left=�$��
50,100,0,0q��� -   $��
ExitButton��$��退出��$��75��$	��Top=�$��Right=�$��
0,100,50,0q��� -   $��	AutoLogin��$	��Top=�$��Center=�$��Right=�$��Center=�$��0,5,5,0q� d� 	IsChecked$
 TrueY� � ToolTip$ 自动登录���� -   $��
showPasswd��$y�Hidden=�$	��Top=�$��25��$	��150��{�. $��Center=�$
��Left=�$��Stretch=�$��	80,30,0,0q��� -   $��userInfoTab��$��账户信息��$y�Visible=�$��70��.��� -   $��userInfoGrid��.+��� -   $��userInfoText���� -   $��userInfoRefresh��$��刷新��$��75��$��25��$��Right=�$	��Top=�$��
0,20,20,20q��� -   $��payTab��$��账户充值��$y�Visible=�$��70��.��� -   $��payGrid��.+��� -   $��payid��$��	用户名��$	��Top=�$
��Left=�$��Center=�$��20,0,0,0q�$��25��$��70���� -   $��	payuserid��$	��Top=�$��25��$	��150��{�. $��Center=�$��Stretch=�$��80,0,0,0q��� -   $��paypwd��$��密码��$	��Top=�$
��Left=�$��Center=�$��	20,30,0,0q�$��25��$��70��G� -   $��	paypasswd��$	��Top=�$��25��$	��150��{�. $��Center=�$
��Left=�$��Stretch=�$��	80,30,0,0q��� -   $��paypasswdVisibility��$��icon/eye_no.png��$��15��$��15��$	��Top=�$��Right=�$��0,35,5,0q��� -   $��payauth��$	��Top=�$��25��$	��150��{�. $��Center=�$��Stretch=�$��	80,60,0,0q��� -   $��	authImage��$��icon/error.png��$��85��$��25��$	��Top=�$
��Left=�$��	15,60,0,0q��� -   $��authImageBtn��$��85��$��25��$	��Top=�$
��Left=�$��	15,60,0,0q����� �� -   $��payLogin��$��登录��$��75��$	��Top=�$
��Left=�$��
50,100,0,0q��� -   $��payExit��$��退出��$��75��$	��Top=�$��Right=�$��
0,100,50,0q��� -   $��showBalanceText��$y�Hidden=�$��余额��$	��Top=�$
��Left=�$��Center=�$��	20,30,0,0q�$��25��$��70���� -   $��showBalance��$y�Hidden=�$	��Top=�$��25��$	��150��{�. $��Center=�$
��Left=�$��Stretch=�$��	80,30,0,0q��� -    $��reChargeText��$y�Hidden=�$��充值金额��$	��Top=�$
��Left=�$��Center=�$��	20,60,0,0q�$��25��$��70���� -!   $��reCharge��$y�Hidden=�$	��Top=�$��25��$	��150��{�. $��Center=�$
��Left=�$��Stretch=�$��	80,60,0,0q��� -"   $��submit��$y�Hidden=�$��充值��$��75��$	��Top=�$
��Left=�$��
50,100,0,0q��� -#   $
��back��$y�Hidden=�$��返回��$��75��$	��Top=�$��Right=�$��
0,100,50,0q��� -$   $��timeOut��$y�Hidden=��� -%   $��payShowPasswd��$y�Hidden=�$	��Top=�$��25��$	��150��{�. $��Center=�$
��Left=�$��Stretch=�$��	80,30,0,0q��� $��软件信息��$��70��.��� .+��� -&   $��Information��$��Center=�$��Center=�