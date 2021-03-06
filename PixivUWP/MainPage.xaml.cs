﻿//PixivUniversal
//Copyright(C) 2017 Pixeez Plus Project

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; version 2
//of the License.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
using Pixeez;
using PixivUWP.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Storage.Streams;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace PixivUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        private Tokens token;
        //public static MainPage Current { get; private set; }

        //public Button RefrechButton
        //{
        //    get
        //    {
        //        return btn_Refresh;
        //    }
        //}

        public MainPage()
        {
            Data.TmpData.mainPage = this;
            this.InitializeComponent();
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            menuItems[0].Label = loader.GetString("Main");
            menuItems[1].Label = loader.GetString("Rank");
            menuItems[2].Label = loader.GetString("Feeds");
            menuItems[3].Label = loader.GetString("Mywork");
            menuItems[4].Label = loader.GetString("Collection");
            menuItems[5].Label = loader.GetString("Download");
            menuBottomItems[0].Label = loader.GetString("Settings");
            menuBottomItems[1].Label = loader.GetString("Feedback");
            version.Text = "v" + Data.VersionHelper.GetThisAppVersionString().ToString() + "β";
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(Title);
            MenuItemList.ItemsSource = menuItems;
            MenuBottomItemList.ItemsSource = menuBottomItems;
            MenuItemList.SelectedIndex = 0;
            token = Data.TmpData.CurrentAuth.Tokens;
            if (DeviceTypeHelper.GetDeviceFormFactorType() == DeviceFormFactorType.Phone)
            {
                TitlebarGrid.Visibility = Visibility.Collapsed;
            }
            else if (DeviceTypeHelper.GetDeviceFormFactorType() == DeviceFormFactorType.Tablet)
            {
                pad1.Width = new Windows.UI.Xaml.GridLength(0);
            }
            tb_Username.Text = Data.TmpData.CurrentAuth.Authorize.User.Name;
            tb_Email.Text = Data.TmpData.CurrentAuth.Authorize.User.Email;
            var asyncres = token.SendRequestAsync(MethodType.GET,
                Data.TmpData.CurrentAuth.Authorize.User.ProfileImageUrls.Px170x170, null);
            var awaiter = asyncres.GetAwaiter();
            awaiter.OnCompleted(async () =>
            {
                try
                {
                    var res = awaiter.GetResult();
                    BitmapImage img = new BitmapImage();
                    await img.SetSourceAsync((await res.GetResponseStreamAsync()).AsInputStream() as IRandomAccessStream);
                    img_BAvatar.Visibility = Visibility.Collapsed;
                    img_Avatar.ImageSource = img;
                }
                catch { }

            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.RegisterBackgroundTask();
            if (Data.TmpData.islight)
                logo.Source = new BitmapImage(new Uri("ms-appx:///Assets/SplashScreen.scale-200.png"));
            else
                logo.Source = new BitmapImage(new Uri("ms-appx:///Assets/DarkSplashScreen.scale-200.png"));
        }

        private async void RegisterBackgroundTask()
        {
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.AlwaysAllowed ||
                backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == taskName)
                    {
                        task.Value.Unregister(true);
                    }
                }

                BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder();
                taskBuilder.Name = taskName;
                taskBuilder.TaskEntryPoint = taskEntryPoint;
                taskBuilder.SetTrigger(new TimeTrigger(15, false));
                var registration = taskBuilder.Register();
                new TileBackground.TileBackground().Run(null);
            }
        }

        private const string taskName = "TileBackground";
        private const string taskEntryPoint = "TileBackground.TileBackground";

        private bool checkVersion()
        {
            if ((string)Data.AppDataHelper.GetValue("last_version") == Data.VersionHelper.GetThisAppVersionString().ToString()) return false;
            Data.AppDataHelper.SetValue("last_version", Data.VersionHelper.GetThisAppVersionString().ToString());
            return true;
        }

        private bool checkVote()
        {
            if ((string)Data.AppDataHelper.GetValue("vote_uid") == Data.Vote.VoteUID) return false;
            Data.AppDataHelper.SetValue("vote_uid", Data.Vote.VoteUID);
            return Data.Vote.NeedVote;
        }

        private void MainPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            if (!(MainFrame.Content is Pages.DetailPage.BlankPage))
            {
                var ib = MainFrame.Content as Pages.IBackable;
                if (ib != null && ib.GoBack() == false)
                {
                    //MainFrame.Navigate(typeof(Pages.DetailPage.BlankPage));
                    var handled = Goback();
                    if (e != null) e.Handled = handled;
                }
                else
                {
                    if (e != null) e.Handled = true;
                }
            }
            else
            {
                var handled = Goback();
                if (e != null) e.Handled = handled;
            }
        }

        public ObservableCollection<MenuItem> menuItems = new ObservableCollection<MenuItem>()
        {
            new MenuItem() {Symbol=""},
            new MenuItem() {Symbol=""},
            new MenuItem() {Symbol=""},
            new MenuItem() {Symbol=""},
            new MenuItem() {Symbol=""},
            new MenuItem() {Symbol=""}
        };
        public ObservableCollection<MenuItem> menuBottomItems = new ObservableCollection<MenuItem>()
        {
            new MenuItem() {Symbol=""},
            new MenuItem() {Symbol=""},
        };

        private void MenuToggle_Click(object sender, RoutedEventArgs e)
        {
            if(MenuToggle.IsChecked.Value)
            {
                Storyboard storyboard = new Storyboard();
                DoubleAnimationUsingKeyFrames animation1 = new DoubleAnimationUsingKeyFrames()
                {
                    EnableDependentAnimation = true
                };
                EasingDoubleKeyFrame f1 = new EasingDoubleKeyFrame();
                f1.Value = 0;
                f1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
                animation1.KeyFrames.Add(f1);
                EasingDoubleKeyFrame f2 = new EasingDoubleKeyFrame();
                f2.Value = 1;
                f2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
                animation1.KeyFrames.Add(f2);
                DoubleAnimationUsingKeyFrames animation2 = new DoubleAnimationUsingKeyFrames();
                animation2.EnableDependentAnimation = true;
                EasingDoubleKeyFrame f3 = new EasingDoubleKeyFrame();
                f3.Value = 1;
                f3.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
                animation2.KeyFrames.Add(f3);
                EasingDoubleKeyFrame f4 = new EasingDoubleKeyFrame();
                f4.Value = 0;
                f4.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
                animation2.KeyFrames.Add(f4);
                Storyboard.SetTarget(animation1, Searchbox);
                Storyboard.SetTarget(animation2, SearchBtn);
                Storyboard.SetTargetProperty(animation1, "Opacity");
                Storyboard.SetTargetProperty(animation2, "Opacity");
                storyboard.Children.Add(animation1);
                storyboard.Children.Add(animation2);
                storyboard.Completed += delegate
                  {
                      Searchbox.Opacity = 1;
                      Searchbox.Visibility = Visibility.Visible;
                      SearchBtn.Opacity = 0;
                      SearchBtn.Visibility = Visibility.Collapsed;
                  };
                Searchbox.Visibility = Visibility.Visible;
                storyboard.Begin();
            }
            contentroot.IsPaneOpen = MenuToggle.IsChecked.Value;
            //var _sender = sender as ToggleButton;
            //Storyboard storyboard = new Storyboard();
            //if(_sender.IsChecked==true)
            //{
            //    DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            //    animation.EnableDependentAnimation = true;
            //    EasingDoubleKeyFrame f1 = new EasingDoubleKeyFrame();
            //    f1.Value = 48;
            //    f1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            //    animation.KeyFrames.Add(f1);
            //    EasingDoubleKeyFrame f2 = new EasingDoubleKeyFrame();
            //    f2.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 4 };
            //    f2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3));
            //    f2.Value = 256;
            //    animation.KeyFrames.Add(f2);
            //    Storyboard.SetTarget(animation, MainMenu);
            //    Storyboard.SetTargetProperty(animation, "Width");
            //    storyboard.Children.Add(animation);
            //    storyboard.Begin();
            //}
            //else
            //{
            //    DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            //    animation.EnableDependentAnimation = true;
            //    EasingDoubleKeyFrame f1 = new EasingDoubleKeyFrame();
            //    f1.Value = 256;
            //    f1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            //    animation.KeyFrames.Add(f1);
            //    EasingDoubleKeyFrame f2 = new EasingDoubleKeyFrame();
            //    f2.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 4 };
            //    f2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3));
            //    f2.Value = 48;
            //    animation.KeyFrames.Add(f2);
            //    Storyboard.SetTarget(animation, MainMenu);
            //    Storyboard.SetTargetProperty(animation, "Width");
            //    storyboard.Children.Add(animation);
            //    storyboard.Begin();
            //}
        }

        private void MenuItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Data.TmpData.isBackTrigger)
            {
                Data.TmpData.isBackTrigger = false;
                return;
            }
            if (MenuItemList.SelectedIndex == -1)
            {
                return;
            }
            MenuBottomItemList.SelectedIndex = -1;
            var tmpInfo = (MainFrame.Content as Data.IBackHandlable)?.GenerateBackInfo();
            if (tmpInfo != null)
                Data.UniversalBackHandler.AddPage(MainFrame.Content.GetType(), tmpInfo);
            Data.TmpData.StopLoading();
            switch (MenuItemList.SelectedIndex)
            {
                case 0:
                    MainFrame.Navigate(typeof(Pages.pg_Main));
                    break;
                case 1:
                    MainFrame.Navigate(typeof(Pages.pg_Rank));
                    break;
                case 2:
                    MainFrame.Navigate(typeof(Pages.pg_Feeds));
                    break;
                case 3:
                    MainFrame.Navigate(typeof(Pages.pg_Mywork));
                    break;
                case 4:
                    MainFrame.Navigate(typeof(Pages.pg_Collection));
                    break;
                case 5:
                    MainFrame.Navigate(typeof(Pages.pg_Download));
                    break;
            }
            contentroot.IsPaneOpen = false;
        }

        private async void btn_Lock_Click(object sender, RoutedEventArgs e)
        {
            if (contentroot.Visibility == Visibility.Collapsed)
            {
                switch (await Windows.Security.Credentials.UI.UserConsentVerifier.RequestVerificationAsync("验证您的身份"))
                {
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.Verified:
                        break;
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.DeviceNotPresent:
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.NotConfiguredForUser:
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.DisabledByPolicy:
                        await new Windows.UI.Popups.MessageDialog("当前识别设备未配置或被系统策略禁用，将默认通过验证").ShowAsync();
                        break;
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.DeviceBusy:
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.RetriesExhausted:
                    case Windows.Security.Credentials.UI.UserConsentVerificationResult.Canceled:
                    default:
                        await new Windows.UI.Popups.MessageDialog("当前识别设备不可用").ShowAsync();
                        btn_Lock.IsChecked = true;
                        return;
                }
                btn_Lock.IsChecked = false;
                await setvis(Visibility.Visible);
            }
            else
            {
                await setvis(Visibility.Collapsed);
                btn_Lock.IsChecked = true;
            }
        }

        private async System.Threading.Tasks.Task setvis(Visibility vis)
        {
            contentroot.Visibility = vis;
            foreach (var one in CoreApplication.Views)
            {
                if (one != CoreApplication.MainView)
                {
                    await one.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Window.Current.Content.Visibility = vis;
                    });
                }
            }
        }

        private async void MenuBottomItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Data.TmpData.isBackTrigger)
            {
                Data.TmpData.isBackTrigger = false;
                return;
            }
            if (MenuBottomItemList.SelectedIndex == -1)
            {
                return;
            }
            var tmpInfo = (MainFrame.Content as Data.IBackHandlable).GenerateBackInfo();
            Data.UniversalBackHandler.AddPage(MainFrame.Content.GetType(), tmpInfo);
            Data.TmpData.StopLoading();
            MenuItemList.SelectedIndex = -1;
            switch (MenuBottomItemList.SelectedIndex)
            {
                case 0:
                    MainFrame.Navigate(typeof(Pages.pg_Settings));
                    break;
                case 1:
                    if(Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
                    {
                        await Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
                    }
                    else
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://git.oschina.net/PixeezPlus/PixivUniversal/issues", UriKind.Absolute));
                    }
                    goto case 0;
            }
            contentroot.IsPaneOpen = false;

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //Current = null;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            switch(Data.TmpData.jumpList)
            {
                case "viewcurrent":
                    break;
                case "markcurrent":
                    break;
                case "sharecurrent":
                    break;
                case "rank":
                    break;
                case "feed":
                    break;
                case "mywork":
                    break;
                case "collection":
                    break;
            }
            if (JumpList.IsSupported())
            {
                var list = await JumpList.LoadCurrentAsync();
                list.Items.Clear();
                if (list.Items.Count == 0)
                {
                    var item0 = JumpListItem.CreateWithArguments("viewcurrent", "查看当前磁贴图片");
                    item0.GroupName = "操作";
                    item0.Logo = new Uri("ms-appx:///Assets/JumplistIcons/ViewCurrent.png");
                    var item1 = JumpListItem.CreateWithArguments("markcurrent", "收藏当前磁贴图片");
                    item1.GroupName = "操作";
                    item1.Logo = new Uri("ms-appx:///Assets/JumplistIcons/MarkCurrent.png");
                    var item2 = JumpListItem.CreateWithArguments("sharecurrent", "分享当前磁贴图片");
                    item2.GroupName = "操作";
                    item2.Logo = new Uri("ms-appx:///Assets/JumplistIcons/ShareCurrent.png");
                    var item3 = JumpListItem.CreateWithArguments("rank", "热门作品");
                    item3.GroupName = "位置";
                    item3.Logo = new Uri("ms-appx:///Assets/JumplistIcons/Rank.png");
                    var item4 = JumpListItem.CreateWithArguments("feed", "最新动态");
                    item4.GroupName = "位置";
                    item4.Logo = new Uri("ms-appx:///Assets/JumplistIcons/Feed.png");
                    var item5 = JumpListItem.CreateWithArguments("mywork", "我的关注");
                    item5.GroupName = "位置";
                    item5.Logo = new Uri("ms-appx:///Assets/JumplistIcons/MyWork.png");
                    var item6 = JumpListItem.CreateWithArguments("collection", "我的收藏");
                    item6.GroupName = "位置";
                    item6.Logo = new Uri("ms-appx:///Assets/JumplistIcons/Collection.png");
                    list.Items.Add(item0);
                    list.Items.Add(item1);
                    list.Items.Add(item2);
                    list.Items.Add(item3);
                    list.Items.Add(item4);
                    list.Items.Add(item5);
                    list.Items.Add(item6);
                    list.Items.Clear();
                }
                await list.SaveAsync();
            }
            Data.TmpData.menuItem = MenuItemList;
            Data.TmpData.menuBottomItem = MenuBottomItemList;
            Data.TmpData.mainFrame = MainFrame;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            if (checkVersion())
                await new Windows.UI.Popups.MessageDialog(loader.GetString("PopupContent"), loader.GetString("PopupTitle")).ShowAsync();
            if (checkVote())
            {
                var md = new Windows.UI.Popups.MessageDialog(Data.Vote.Message, Data.Vote.Title);
                md.Commands.Add(new Windows.UI.Popups.UICommand("同意") { Id = "y" });
                md.Commands.Add(new Windows.UI.Popups.UICommand("否决") { Id = "n" });
                md.Commands.Add(new Windows.UI.Popups.UICommand("弃权") { Id = "a" });
                var res = await md.ShowAsync();
                switch (res.Id as string)
                {
                    case "y":
                        Data.CustomEventHelper.LogEvent(Data.Vote.Name + "_y", Data.CustomEventHelper.EventType.Vote);
                        break;
                    case "n":
                        Data.CustomEventHelper.LogEvent(Data.Vote.Name + "_n", Data.CustomEventHelper.EventType.Vote);
                        break;
                }
            }
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            MainPage_BackRequested(null, null);
        }

        private bool Goback()
        {
            var backInfo = Data.UniversalBackHandler.Back();
            if (backInfo == null) return false;
            Data.TmpData.StopLoading();
            MainFrame.Navigate(backInfo.page, new object[] { true, backInfo.info });
            return true;
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            //btn_Refresh.IsEnabled = false;
            //var obj = MainFrame.Content as Pages.DetailPage.IRefreshable;
            //try
            //{
            //    await obj.RefreshAsync();
            //}
            //finally
            //{
            //    btn_Refresh.IsEnabled = true;
            //}
            var tmpInfo = (MainFrame.Content as Data.IBackHandlable)?.GenerateBackInfo();
            if (tmpInfo != null)
                Data.UniversalBackHandler.AddPage(MainFrame.Content.GetType(), tmpInfo);
            Data.TmpData.StopLoading();
            if (MainFrame.Content is Pages.pg_Search)
            {
                MainFrame.Navigate(typeof(Pages.pg_Search), currentQueryString);
                return;
            }
            MainFrame.Navigate(MainFrame.Content.GetType());
        }

        private void contentroot_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            MenuToggle.IsChecked = false;
            Storyboard storyboard = new Storyboard();
            DoubleAnimationUsingKeyFrames animation1 = new DoubleAnimationUsingKeyFrames()
            {
                EnableDependentAnimation = true
            };
            EasingDoubleKeyFrame f1 = new EasingDoubleKeyFrame();
            f1.Value = 0;
            f1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            animation1.KeyFrames.Add(f1);
            EasingDoubleKeyFrame f2 = new EasingDoubleKeyFrame();
            f2.Value = 1;
            f2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
            animation1.KeyFrames.Add(f2);
            DoubleAnimationUsingKeyFrames animation2 = new DoubleAnimationUsingKeyFrames();
            animation2.EnableDependentAnimation = true;
            EasingDoubleKeyFrame f3 = new EasingDoubleKeyFrame();
            f3.Value = 1;
            f3.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            animation2.KeyFrames.Add(f3);
            EasingDoubleKeyFrame f4 = new EasingDoubleKeyFrame();
            f4.Value = 0;
            f4.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
            animation2.KeyFrames.Add(f4);
            Storyboard.SetTarget(animation2, Searchbox);
            Storyboard.SetTarget(animation1, SearchBtn);
            Storyboard.SetTargetProperty(animation1, "Opacity");
            Storyboard.SetTargetProperty(animation2, "Opacity");
            storyboard.Children.Add(animation1);
            storyboard.Children.Add(animation2);
            storyboard.Completed += delegate
            {
                Searchbox.Opacity = 0;
                Searchbox.Visibility = Visibility.Collapsed;
                SearchBtn.Opacity = 1;
                SearchBtn.Visibility = Visibility.Visible;
            };
            SearchBtn.Visibility = Visibility.Visible;
            storyboard.Begin();
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            MenuToggle.IsChecked = true;
            MenuToggle_Click(null, null);
        }

        string currentQueryString;

        private void Searchbox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.QueryText == "") return;
            Query(args.QueryText);
        }

        internal void Query(string text)
        {
            MenuToggle.IsChecked = false;
            MenuToggle_Click(null, null);
            MenuItemList.SelectedIndex = -1;
            var tmpInfo = (MainFrame.Content as Data.IBackHandlable).GenerateBackInfo();
            if (tmpInfo != null)
                Data.UniversalBackHandler.AddPage(MainFrame.Content.GetType(), tmpInfo);
            Data.TmpData.StopLoading();
            MainFrame.Navigate(typeof(Pages.pg_Search), text);
            currentQueryString = text;
        }

        private void btn_User_Click(object sender, RoutedEventArgs e)
        {
            Data.TmpData.StopLoading();
            var backinfo = (MainFrame.Content as Data.IBackHandlable).GenerateBackInfo();
            Data.UniversalBackHandler.AddPage(MainFrame.Content.GetType(), backinfo);
            MainFrame.Navigate(typeof(Pages.Win_UserInfo), Data.TmpData.CurrentAuth.Authorize.User);
            contentroot.IsPaneOpen = false;
        }
    }
}
