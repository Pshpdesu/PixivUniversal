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
using PixivUWP.ViewModels;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using PixivUWP.Pages.DetailPage;
using System.Threading.Tasks;
using System.Diagnostics;
using PixivUWP.Data;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace PixivUWP.Pages
{
    public class PgCollectionBackInfo
    {
        public string NextPublicUrl { get; set; }
        public string NextPrivateUrl { get; set; }
    }

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    /// 
    public sealed partial class pg_Collection : Windows.UI.Xaml.Controls.Page, DetailPage.IRefreshable,IBackable,IBackHandlable
    {

        ItemViewList<IllustWork> list;
        public pg_Collection()
        {
            this.InitializeComponent();
            //list.LoadingMoreItems += List_LoadingMoreItems;
            //list.HasMoreItemsEvent += List_HasMoreItemsEvent;
            mdc.MasterListView = MasterListView;
        }

        int selectedindex = -1;

        public BackInfo GenerateBackInfo()
            => new BackInfo { list = this.list, param = new PgCollectionBackInfo{ NextPublicUrl= this.nextPublicUrl, NextPrivateUrl = this.nextPrivateUrl }, selectedIndex = MasterListView.SelectedIndex };

        private async Task firstLoadAsync()
        {
            while (scrollRoot.ScrollableHeight - 500 <= 10)
                if (await loadAsync() == false)
                    return;
        }

        //private void List_HasMoreItemsEvent(ItemViewList<IllustWork> sender, PackageTuple.WriteableTuple<bool> args)
        //{
        //    args.Item1 = nexturl != string.Empty;
        //}

        bool _isLoading = false;
        private async Task<bool> loadAsync()
        {
            if (_isLoading) return true;
            Debug.WriteLine("loadAsync() called.");
            _isLoading = true;
            try
            {
                Illusts publicRoot = new Illusts();
                Illusts privateRoot = new Illusts();
                if (nextPublicUrl == null)
                {
                    publicRoot = await Data.TmpData.CurrentAuth.Tokens.GetUserFavoriteWorksAsync(Data.TmpData.CurrentAuth.Authorize.User.Id.Value);
                }
                else
                {
                    if(nextPublicUrl!="")
                        publicRoot = await Data.TmpData.CurrentAuth.Tokens.AccessNewApiAsync<Illusts>(nextPublicUrl);
                }
                nextPublicUrl = publicRoot.next_url ?? string.Empty;


                if (nextPrivateUrl == null)
                {
                    privateRoot = await Data.TmpData.CurrentAuth.Tokens.GetUserFavoriteWorksAsync(Data.TmpData.CurrentAuth.Authorize.User.Id.Value, "private");
                }
                else {
                    if(nextPrivateUrl!="")
                        privateRoot = await Data.TmpData.CurrentAuth.Tokens.AccessNewApiAsync<Illusts>(nextPrivateUrl);
                }
                nextPrivateUrl = privateRoot.next_url ?? string.Empty;

                foreach (var one in publicRoot.illusts)
                {
                    if (!list.Contains(one, Data.WorkEqualityComparer.Default))
                        list.Add(one);
                }
                foreach (var one in privateRoot.illusts)
                {
                    if (!list.Contains(one, Data.WorkEqualityComparer.Default))
                        list.Add(one);
                }
                _isLoading = false;
                return true;
            }
            catch
            {
                _isLoading = false;
                return false;
            }
        }

        string nextPublicUrl = null;
        string nextPrivateUrl = null;
        //private async void List_LoadingMoreItems(ItemViewList<IllustWork> sender, Tuple<Yinyue200.OperationDeferral.OperationDeferral<uint>, uint> args)
        //{
        //    var nowcount = list.Count;
        //    try
        //    {
        //        var root = nexturl == null ? await Data.TmpData.CurrentAuth.Tokens.GetUserFavoriteWorksAsync(Data.TmpData.CurrentAuth.Authorize.User.Id.Value) : await Data.TmpData.CurrentAuth.Tokens.AccessNewApiAsync<Illusts>(nexturl);
        //        nexturl = root.next_url ?? string.Empty;
        //        foreach (var one in root.illusts)
        //        {
        //            if (!list.Contains(one, Data.WorkEqualityComparer.Default))
        //                list.Add(one);
        //        }
        //        nowpage++;
        //    }
        //    catch
        //    {
        //        nexturl = string.Empty;
        //    }
        //    finally
        //    {
        //        args.Item1.Complete((uint)(list.Count - nowcount));
        //    }
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if ((bool)((object[])e.Parameter)[0])
                {
                    Data.TmpData.isBackTrigger = true;
                    Data.TmpData.menuItem.SelectedIndex = 4;
                    Data.TmpData.menuBottomItem.SelectedIndex = -1;
                    list = ((BackInfo)((object[])e.Parameter)[1]).list as ItemViewList<IllustWork>;
                    var nextInfo = ((BackInfo)((object[])e.Parameter)[1]).param as PgCollectionBackInfo;
                    nextPublicUrl = nextInfo.NextPublicUrl;
                    nextPrivateUrl = nextInfo.NextPrivateUrl;
                    selectedindex = ((BackInfo)((object[])e.Parameter)[1]).selectedIndex;
                }
                else
                {
                    list = new ItemViewList<IllustWork>();
                }
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("NullException");
                list = new ItemViewList<IllustWork>();
            }
            finally
            {
                MasterListView.ItemsSource = list;
                var result = firstLoadAsync();
                if (selectedindex != -1)
                {
                    MasterListView.SelectedIndex = selectedindex;
                    mdc.MasterListView_ItemClick(typeof(DetailPage.WorkDetailPage), MasterListView.Items[selectedindex]);
                }
            }
        }


        private void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            mdc.MasterListView_ItemClick(typeof(DetailPage.WorkDetailPage), e.ClickedItem);
        }

        public Task RefreshAsync()
        {
            return ((IRefreshable)mdc).RefreshAsync();
        }

        public bool GoBack()
        {
            return ((IBackable)mdc).GoBack();
        }

        double _originHeight = 0;
        private void scrollRoot_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollRoot.VerticalOffset == _originHeight) return;
            _originHeight = scrollRoot.VerticalOffset;
            if (scrollRoot.VerticalOffset <= scrollRoot.ScrollableHeight - 500) return;
            var result = loadAsync();
        }
    }
}
