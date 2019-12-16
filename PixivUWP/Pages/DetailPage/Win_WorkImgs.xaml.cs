//PixivUniversal
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
using Pixeez.Objects;
using System;
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
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace PixivUWP.Pages.DetailPage
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Win_WorkImgs : Windows.UI.Xaml.Controls.Page
    {
        public Win_WorkImgs()
        {
            this.InitializeComponent();
            urlBitmaps = new Dictionary<ImageUrls, (bool, BitmapImage)>();
        }
        Dictionary<ImageUrls, (bool, BitmapImage)> urlBitmaps;

        IllustWork work;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            work = e.Parameter as IllustWork;
            flipview.ItemsSource = work.meta_pages;

            foreach (var pic in work.meta_pages)
            {
                var newBitmap = new BitmapImage();
                //newBitmap.DownloadProgress += (s, eargs) =>
                //{
                //    if (eargs.Progress == 100)
                //    {
                //        urlBitmaps.TryGetValue(pic.ImageUrls, out var bm);
                //        urlBitmaps[pic.ImageUrls] = (true, bm.Item2);
                //    }
                //};
                urlBitmaps.Add(pic.ImageUrls, (false, newBitmap));
            }
            flipview.SelectedIndex = 0;
        }


        private async void Image_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var metapage = args.NewValue as MetaPages;
            if (metapage == null)
            {
                return;
            }
            var page = urlBitmaps[metapage.ImageUrls];
            Panel pl = sender as Panel;
            if (pl == null)
            {
                return;
            }

            Image img = pl.FindName("img") as Image;
            if (img == null)
            {
                return;

            }
            img.Source = null;
            ProgressRing progressRing = pl.FindName("pro") as ProgressRing;
            try
            {
                if (page.Item1 == false)
                {
                    ProgressBarVisualHelper.SetYFHelperVisibility(progressRing, true);
                    using (var stream = await Data.TmpData.CurrentAuth.Tokens.SendRequestAsync(
                                Pixeez.MethodType.GET,
                                metapage.ImageUrls.Original ?? metapage.ImageUrls.Large ?? metapage.ImageUrls.Medium))
                    {
                        await page.Item2.SetSourceAsync((await stream.GetResponseStreamAsync()).AsRandomAccessStream());
                        img.Source = page.Item2;
                        urlBitmaps[metapage.ImageUrls] = (true, page.Item2);

                    }
                }
                else
                {
                    img.Source = page.Item2;
                }
            }

            catch (Exception e)
            {

            }
            finally
            {
                ProgressBarVisualHelper.SetYFHelperVisibility(progressRing, false);
            }

        }

        private float GetMinimalZoomFactor(double imageSize, double scrollViewSize)
        {
            if (imageSize > scrollViewSize)
            {
                return (float)(scrollViewSize / imageSize);
            }
            else
            {
                return (float)(imageSize / scrollViewSize);
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as AppBarToggleButton).IsChecked == false)
            {
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
            else
            {
                if (!Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode())
                {
                    (sender as AppBarToggleButton).IsChecked = false;
                }
            }

        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton downloadbutton && flipview.SelectedValue is MetaPages sv)
            {
                downloadbutton.IsEnabled = false;
                try
                {
                    var filename = work.Id + "_p" + flipview.SelectedIndex.ToString();
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await Data.DownloadManager.AddTaskAsync(sv.ImageUrls.Original ?? sv.ImageUrls.Large ?? sv.ImageUrls.Medium, filename);
                    });
                }
                finally
                {
                    downloadbutton.IsEnabled = true;
                }
            }
        }

        private void Img_Loaded(object sender, RoutedEventArgs e)
        {
            var sv = flipview.SelectedItem;
            //if (page.Item2.PixelHeight > page.Item2.PixelWidth)
            //{
            //    //img.Height = sv.ActualHeight;
            //    sv.MinZoomFactor = GetMinimalZoomFactor(page.Item2.PixelHeight, sv.ViewportHeight);
            //}
            //else
            //{
            //    //img.Width = sv.ActualWidth;
            //    sv.MinZoomFactor = GetMinimalZoomFactor(page.Item2.PixelWidth, sv.ViewportWidth);
            //}
        }
    }
}
