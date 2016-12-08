﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI;
using PixivUWP.Views;
using Windows.UI.Xaml.Media.Animation;
using Windows.Foundation.Metadata;
using System.Diagnostics;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Pixeez;
using System.Threading.Tasks;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace PixivUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoadingPage : Page
    {
        Storyboard storyboard = new Storyboard();
        Storyboard storyboard2 = new Storyboard();
        bool isLoaded = false;

        public LoadingPage() : this(Data.TmpData.Username, Data.TmpData.Password) { }

        public LoadingPage(string Username, string Password)
        {
            this.InitializeComponent();
            //settitlecolor();
            Loaded += async delegate
            {
                //Perform the animations
                BindableMargin margin = new Views.BindableMargin(image);
                DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
                animation.EnableDependentAnimation = true;
                EasingDoubleKeyFrame f1 = new EasingDoubleKeyFrame();
                f1.Value = 0;
                f1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
                animation.KeyFrames.Add(f1);
                EasingDoubleKeyFrame f2 = new EasingDoubleKeyFrame();
                f2.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = 4 };
                f2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.8));
                f2.Value = -Window.Current.Bounds.Height / 4;
                animation.KeyFrames.Add(f2);
                Storyboard.SetTarget(animation, margin);
                Storyboard.SetTargetProperty(animation, "Top");
                //DoubleAnimation animation2 = new DoubleAnimation();
                //Storyboard.SetTarget(animation2, image);
                //Storyboard.SetTargetProperty(animation2, "Opacity");
                //animation2.Duration = new Duration(TimeSpan.FromSeconds(1));
                //animation2.From = 0;
                //animation2.To = 100;
                //Windows Phones do not need the animation
                if (DeviceTypeHelper.GetDeviceFormFactorType() != DeviceFormFactorType.Phone)
                {
                    storyboard.Children.Add(animation);
                    //storyboard.Children.Add(animation2);
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
                storyboard.Completed += delegate
                {
                    //Main animation finish
                    BindableMargin margin2 = new Views.BindableMargin(image);
                    image.Opacity = 100;
                    margin2.Top = -Window.Current.Bounds.Height / 4;
                    ring.IsActive = true;
                    BeginLoading(Username, Password);
                };
                storyboard.Begin();
            };
            SizeChanged += delegate
            {
                if (!isLoaded)
                {
                    isLoaded = true;
                    return;
                }
                BindableMargin margin = new Views.BindableMargin(image);
                storyboard.Stop();
                image.Opacity = 100;
                margin.Top = -Window.Current.Bounds.Height / 4;
            };
        }

        private void settitlecolor()
        {
            var view = ApplicationView.GetForCurrentView();
            view.TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 175, 126, 255);
            view.TitleBar.ButtonInactiveBackgroundColor = Color.FromArgb(255, 175, 126, 255);
            view.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 159, 100, 255);
            view.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 198, 163, 255);
        }

        /// <summary>
        /// The function to load data and navigate to the next page.
        /// </summary>
        private async void BeginLoading(string username, string password)
        {
            try
            {
                var token = await Auth.AuthorizeAsync(username, password);
                Data.TmpData.CurrentToken = token;
                Frame.Navigate(typeof(MainPage));
                Debug.Write("Access token: ");
                Debug.WriteLine(token.AccessToken);
            }
            catch
            {
                status.Visibility = Visibility.Visible;
            }
        }

        private async void status_Click(object sender, RoutedEventArgs e)
        {
            await rollBackAnimation();
        }

        private async Task rollBackAnimation()
        {
            status.Visibility = Visibility.Collapsed;
            ring.IsActive = false;
            BindableMargin margin = new Views.BindableMargin(image);
            margin.Top = -Window.Current.Bounds.Height / 4;
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            animation.EnableDependentAnimation = true;
            EasingDoubleKeyFrame f1 = new EasingDoubleKeyFrame();
            f1.Value = -Window.Current.Bounds.Height / 4;
            f1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
            animation.KeyFrames.Add(f1);
            EasingDoubleKeyFrame f2 = new EasingDoubleKeyFrame();
            f2.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = 4 };
            f2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.8));
            f2.Value = -315;
            animation.KeyFrames.Add(f2);
            Storyboard.SetTarget(animation, margin);
            Storyboard.SetTargetProperty(animation, "Top");
            //Windows Phones do not need the animation
            if (DeviceTypeHelper.GetDeviceFormFactorType() != DeviceFormFactorType.Phone)
            {
                storyboard2.Children.Add(animation);
                //storyboard.Children.Add(animation2);
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
            storyboard2.Completed += delegate
            {
                //Main animation finish
                BindableMargin margin2 = new Views.BindableMargin(image);
                image.Opacity = 100;
                margin2.Top = -315;
                Frame.Navigate(typeof(LoginPage));
            };
            storyboard2.Begin();
        }
    }
}