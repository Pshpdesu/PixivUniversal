using RePixivAPI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using static RePixivAPI.Helpers.AuthenticationConstants;

namespace RePixivAPI
{
    public class RePixivAPIClient : PixivAPICore
    {
        public static async Task<PixivAPICore> GetPixivApiClientAsync(GrantType grantType, Credentials credentials)
        {
            RePixivAPIClient client = new RePixivAPIClient();
            await client.Authorize(grantType, credentials);
            return client;
        }
        private RePixivAPIClient() : base() { }


        #region PixivApiRequests


        //TODO: add method to project responses by default by using generics
        public async Task<RecommendedRootobject> GetRecommendedWorks(
            bool authorizedContent = true,
            string filter = "for_ios",
            bool includeRankingLabel = true,
            //TODO: Idk what is this
            //string contentType = "illust",
            //string maxBookmarkId=null,
            //string minBookmarkId=null,
            //string offset = null,
            //bool? includeRankingIllusts = null,
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                { "content_type", "illust" },
                { "include_ranking_label", includeRankingLabel.ToString() },
                { "filter", filter },
            };
            Uri reqstUri = authorizedContent ?
                new Uri(baseuri + "v1/illust/recommended") :
                new Uri(baseuri + "v1/illust/recommended-nologin");

            return authorizedContent ?
                await this.SendAuthorizedApiRequestAsync<RecommendedRootobject>(HttpMethod.Get, reqstUri, parameters, cancellationToken) :
                await this.SendPublicApiRequestAsync<RecommendedRootobject>(HttpMethod.Get, reqstUri, parameters, cancellationToken);

        }

        //TODO: delete this
        //public async Task<IllustCommentObject> GetIllustComments(
        //    string illust_id,
        //    int offset = 0, bool includeTotalComments = true,
        //    CancellationToken cancellationToken=default)
        //{
        //    string baseuri = "https://app-api.pixiv.net/";
        //    var parameters = new Dictionary<string, string>()
        //    {
        //        {"illust_id", illust_id },
        //        {"offset", offset.ToString() },
        //        {"include_total_comments", includeTotalComments.ToString() }
        //    };

        //    using (var resp = await this.SendAuthorizedApiRequestAsync(HttpMethod.Get, new Uri(baseuri + "v1/illust/comments"), parameters, cancellationToken))
        //    {
        //        return await ProjectAsync<IllustCommentObject>(await resp.Content.ReadAsStringAsync());
        //    }
        //}
        public async Task<IllustCommentObject> GetIllustCommentsAsync(
            string illust_id,
            int offset = 0, bool includeTotalComments = true,
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                {"illust_id", illust_id },
                {"offset", offset.ToString() },
                {"include_total_comments", includeTotalComments.ToString() }
            };

            return await this.SendAuthorizedApiRequestAsync<IllustCommentObject>(
                HttpMethod.Get,
                new Uri(baseuri + "v1/illust/comments"),
                parameters,
                cancellationToken);
        }

        //TODO: change GetBookmarkedDetailAsync to GetBookmarkedDetailByRestrictionAsync
        //TODO: change all "string restrict" params to enum
        //TODO: figure out, what this api method do
        public async Task<BookmarkDetailRootobject> GetBookMarkedDetailAsync(
            int illust_id,
            string restrict = "public",
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                {"illust_id", illust_id.ToString() },
                { "restrict", restrict }
            };

            return await this.SendAuthorizedApiRequestAsync<BookmarkDetailRootobject>(HttpMethod.Get, new Uri(baseuri + "v2/illust/bookmark/detail"), parameters, cancellationToken);
        }

        //TODO: change all "string restrict" params to enum
        public async Task<BookmarkDetailRootobject> GetBookMarkedTagsAsync(string restrict = "public",
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                {"restrict", restrict },
            };
            return await this.SendAuthorizedApiRequestAsync<BookmarkDetailRootobject>(HttpMethod.Get, new Uri(baseuri + "v1/user/bookmark-tags/illust"), parameters, cancellationToken);
        }

        //TODO: rename method
        //FollowUser ???
        //TODO: same as "string restrict"
        [Deprecated("Need to find more fresh api for it", DeprecationType.Deprecate, 1)]
        public async Task AddFavouriteUser(long userId, string publicity = "public", CancellationToken cancellationToken = default)
        {
            string baseuri = "https://public-api.secure.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                {"target_user_id",userId.ToString() },
                {"publicity", publicity },
            };
            await SendAuthorizedApiRequestAsync(HttpMethod.Post, new Uri(baseuri + "v1/me/favorite-users.json"), parameters, cancellationToken);

        }

        [Deprecated("Need to find more fresh api for it", DeprecationType.Deprecate, 1)]
        public async Task DeleteFavouriteUser(long userId, string publicity = "public", CancellationToken cancellationToken = default)
        {
            string baseuri = "https://public-api.secure.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                {"delete_ids", userId.ToString() },
                {"publicity", publicity },
            };
            await SendAuthorizedApiRequestAsync(HttpMethod.Delete, new Uri(baseuri + "v1/me/favorite-users.json"), parameters, cancellationToken);

        }

        public async Task<Illusts> GetRelatedWorks(long illustId, string filter = "for_ios", CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                {"illust_id", illustId.ToString() },
                {"filter", filter },
            };
            return await SendAuthorizedApiRequestAsync<Illusts>(HttpMethod.Get, new Uri(baseuri + "v2/illust/related"), parameters, cancellationToken);
        }

        [Deprecated("Need to find more fresh api for it", DeprecationType.Deprecate, 1)]
        public async Task<List<NormalWork>> GetWorksAsync(long illustId, CancellationToken cancellationToken = default)
        {
            string baseuri = "https://public-api.secure.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                { "profile_image_sizes", "px_170x170,px_50x50" },
                { "image_sizes", "px_128x128,small,medium,large,px_480mw" },
                { "include_stats", "true" }
            };
            //TODO: maybe need to add method to access public authorized api idk
            return await SendAuthorizedApiRequestAsync<List<NormalWork>>(
                HttpMethod.Get,
                new Uri(baseuri + "v1/works/" + illustId.ToString() + ".json"),
                parameters, cancellationToken);
        }

        [Deprecated("Need to find more fresh api for it", DeprecationType.Deprecate, 1)]
        public async Task<List<NormalWork>> GetUsersAsync(long authorId, CancellationToken cancellationToken = default)
        {
            string baseuri = "https://public-api.secure.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                { "profile_image_sizes", "px_170x170,px_50x50" } ,
                { "image_sizes", "px_128x128,small,medium,large,px_480mw" } ,
                { "include_stats", "1" } ,
                { "include_profile", "1" } ,
                { "include_workspace", "1" } ,
                { "include_contacts", "1" } ,
            };
            //TODO: maybe need to add method to access public authorized api idk
            return await SendAuthorizedApiRequestAsync<List<NormalWork>>(
                HttpMethod.Get,
                new Uri(baseuri + "v1/users/" + authorId.ToString() + ".json"),
                parameters, cancellationToken);
        }

        public async Task<Illusts> GetUserFavoriteWorksAsync(
            long userId,
            string restrict = "public",
            string filter = "for_ios",
            //string maxBookmarkId=null,
            //string minBookmarkId=null,
            //string tag = null,
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                { "user_id", userId.ToString() },
                { "restrict", restrict },
                { "filter", filter },
            };
            return await SendAuthorizedApiRequestAsync<Illusts>(HttpMethod.Get, new Uri(baseuri + "v1/user/bookmarks/illust"), parameters, cancellationToken);
        }

        //TODO: publicity same as "string restrict" 
        //TODO: rename to AddFavoriteWorksAsync
        public async Task AddMyFavoriteWorksAsync(
            long illustId,
            string publicity = "public",
            IEnumerable<string> tags = null,
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                { "illust_id", illustId.ToString() },
                { "restrict", publicity },
            };
            if (tags != null)
            {
                parameters.Add("tags[]", string.Join(",", tags));
            }
            (await SendAuthorizedApiRequestAsync(HttpMethod.Post, new Uri(baseuri + "/v2/illust/bookmark/add"), parameters, cancellationToken)).Dispose();
        }

        //TODO: publicity same as "string restrict" 
        //TODO: rename to DeleteFavoriteWorksAsync
        public async Task<Paginated<UsersFavoriteWork>> DeleteMyFavoriteWorksAsync(
            IEnumerable<long> illustId,
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                { "illust_id", string.Join(",",illustId) },
            };
            return await SendAuthorizedApiRequestAsync<Paginated<UsersFavoriteWork>>(
                HttpMethod.Post,
                new Uri(baseuri + "/v2/illust/bookmark/delete"),
                parameters,
                cancellationToken);
        }

        public async Task<RecommendedRootobject> GetMyFollowingWorksAsync(
            string restrict = "public",
            int offset = 0, Uri nextUri = null,
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                { "restrict", restrict },
            };
            if (offset != 0)
            {
                parameters.Add("offset", offset.ToString());
            }
            async Task<RecommendedRootobject> getResultFunc(Uri uri) =>
                await SendAuthorizedApiRequestAsync<RecommendedRootobject>(HttpMethod.Get, uri, parameters, cancellationToken);
            return nextUri == null ?
                await getResultFunc(new Uri(baseuri + "/v2/illust/follow")) :
                await getResultFunc(nextUri);
        }

        //TODO: find out possible types of "type" and make a enum
        //TODO: find out possible types of "filter" and make a enum
        public async Task<Illusts> GetUserWorksAsync(
            long userId,
            string type = "illust",
            string filter = "for_ios",
            int offset = 0,
            Uri nextUri = null,
            CancellationToken cancellationToken = default)
        {
            string baseuri = "https://app-api.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                { "user_id", userId.ToString() },
                { "type", type },
                { "filter", filter }
            };

            if (offset != 0)
            {
                parameters.Add("offset", offset.ToString());
            }
            async Task<RecommendedRootobject> getResultFunc(Uri uri) =>
                await SendAuthorizedApiRequestAsync<RecommendedRootobject>(HttpMethod.Get, uri, parameters, cancellationToken);
            return nextUri == null ?
                await getResultFunc(new Uri(baseuri + "/v1/user/illust")) :
                await getResultFunc(nextUri);
        }

        [Deprecated("Need to find more fresh api for it", DeprecationType.Deprecate, 1)]
        public async Task<List<Feed>> GetUsersFeedsAsync(
            long authorId,
            long maxId = 0,
            bool showR18 = false,
            CancellationToken cancellationToken = default)
        {
            var baseuri = "https://public-api.secure.pixiv.net/";

            var parameters = new Dictionary<string, string>
            {
                { "relation", "all" } ,
                { "type", "touch_nottext" } ,
                { "show_r18", Convert.ToInt32(showR18).ToString() } ,
            };

            if (maxId != 0)
                parameters.Add("max_id", maxId.ToString());

            return await SendAuthorizedApiRequestAsync<List<Feed>>(
                HttpMethod.Get,
                new Uri(baseuri + "v1/users/" + authorId.ToString() + "/feeds.json"),
                parameters,
                cancellationToken);
        }

        //TODO: enum for mode (daily, weekly, monthly, male, female, rookie, daily_r18, weekly_r18, male_r18, female_r18, r18g)
        public async Task<Paginated<Rank>> GetRankingsAllAsync(
            string mode = "daily",
            int page = 1,
            int perpage = 30,
            bool includeSanityLevel = true,
            string date = "",
            CancellationToken cancellationToken = default)
        {
            var baseuri = "https://public-api.secure.pixiv.net/";
            var parameters = new Dictionary<string, string>()
            {
                {"mode", mode },
            { "page", page.ToString() },
                {"per_page",perpage.ToString() },
            };
            if (!string.IsNullOrEmpty(date))
            {
                parameters.Add("date", date);
            }
            return await SendAuthorizedApiRequestAsync<Paginated<Rank>>(HttpMethod.Get,
                new Uri(baseuri + "/v1/ranking/all"),
                parameters,
                cancellationToken);
        }

        #endregion


    }
}
