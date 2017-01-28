﻿//PixivUniversal
//Copyright(C) 2017 Pixeez Plus Project

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
using PixivUWP.Data;
using PixivUWP.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace PixivUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        Storyboard storyboard = new Storyboard();

        public LoginPage()
        {
            this.InitializeComponent();
            if (AppDataHelper.GetValue("uname") != null)
                txt_UserName.Text = AppDataHelper.GetValue("uname") as string;
            if (AppDataHelper.GetValue("upasswd") != null)
                txt_Password.Password = AppDataHelper.GetValue("upasswd") as string;
            if (AppDataHelper.GetValue("isauto") != null)
                s_auto.IsOn = Convert.ToBoolean(AppDataHelper.GetValue("isauto") as string);
            if (AppDataHelper.GetValue("isrem") != null)
                s_remember.IsOn = Convert.ToBoolean(AppDataHelper.GetValue("isrem") as string);
            if (s_auto.IsOn)
                beginLoading();
            var curView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            curView.SetPreferredMinSize(new Windows.Foundation.Size(500, 630));
        }
        private async Task logoAnimation()
        {
            //Perform the animations
            BindableMargin margin = new Views.BindableMargin(logoimage_animated);
            margin.Top = -315;
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            animation.EnableDependentAnimation = true;
            EasingDoubleKeyFrame f1 = new EasingDoubleKeyFrame();
            f1.Value = -315;
            f1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
            animation.KeyFrames.Add(f1);
            EasingDoubleKeyFrame f2 = new EasingDoubleKeyFrame();
            f2.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = 4 };
            f2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.8));
            f2.Value = 0;
            animation.KeyFrames.Add(f2);
            Storyboard.SetTarget(animation, margin);
            Storyboard.SetTargetProperty(animation, "Top");
            //Windows Phones do not need the animation
            //if (DeviceTypeHelper.GetDeviceFormFactorType() != DeviceFormFactorType.Phone)
            {
                storyboard.Children.Add(animation);
            }
            //Only phones should have this step
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var appview = ApplicationView.GetForCurrentView();
                appview.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                var statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusbar.ForegroundColor = Colors.White;
                statusbar.BackgroundOpacity = 0;
                await statusbar.HideAsync();
            }
            storyboard.Completed += Storyboard_Completed;
            storyboard.Begin();
        }

        private void Storyboard_Completed(object sender, object e)
        {
            //Main animation finish
            BindableMargin margin2 = new Views.BindableMargin(logoimage_animated);
            logoimage_animated.Opacity = 100;
            margin2.Top = 0;
            Data.TmpData.Username = txt_UserName.Text;
            Data.TmpData.Password = txt_Password.Password;
            storeData();
            (Window.Current.Content as Frame).Navigate(typeof(LoadingPage));
        }


        private void storeData()
        {
            AppDataHelper.SetValue("uname", txt_UserName.Text);
            if (s_remember.IsOn)
                AppDataHelper.SetValue("upasswd", txt_Password.Password);
            else
                AppDataHelper.SetValue("upasswd", "");
            AppDataHelper.SetValue("isrem", s_remember.IsOn.ToString());
            AppDataHelper.SetValue("isauto", s_auto.IsOn.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            beginLoading();
        }

        private async void beginLoading()
        {
            logoimage_animated.Opacity = 100;
            controls.Visibility = Visibility.Collapsed;
            await logoAnimation();
        }

        private void s_auto_Toggled(object sender, RoutedEventArgs e)
        {
            if((sender as ToggleSwitch).IsOn)
                s_remember.IsOn = true;
        }

        private void s_remember_Toggled(object sender, RoutedEventArgs e)
        {
            if(!(sender as ToggleSwitch).IsOn)
                s_auto.IsOn = false;
        }
    }
}
