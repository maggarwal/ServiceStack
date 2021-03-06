using System;
using System.Net;

namespace ServiceStack.Auth
{
    public interface IAuthHttpGateway
    {
        string VerifyTwitterCredentials(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret);
        string DownloadTwitterUserInfo(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string twitterUserId);
        string DownloadFacebookUserInfo(string facebookCode, params string[] fields);
        string DownloadYammerUserInfo(string yammerUserId);
    }

    public class AuthHttpGateway : IAuthHttpGateway
    {
        public static string TwitterUserUrl = "https://api.twitter.com/1.1/users/lookup.json?user_id={0}";
        public static string TwitterVerifyCredentialsUrl = "https://api.twitter.com/1.1/account/verify_credentials.json?include_email=true";

        public static string FacebookUserUrl = "https://graph.facebook.com/v2.8/me?access_token={0}";

        public static string YammerUserUrl = "https://www.yammer.com/api/v1/users/{0}.json";

        public string DownloadTwitterUserInfo(string consumerKey, string consumerSecret,
            string accessToken, string accessTokenSecret, string twitterUserId)
        {
            twitterUserId.ThrowIfNullOrEmpty(nameof(twitterUserId));
            return GetJsonFromOAuthUrl(consumerKey, consumerSecret, accessToken, accessTokenSecret,
                TwitterUserUrl.Fmt(twitterUserId));
        }

        public string VerifyTwitterCredentials(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            return GetJsonFromOAuthUrl(consumerKey, consumerSecret, accessToken, accessTokenSecret, TwitterVerifyCredentialsUrl);
        }

        public static string GetJsonFromOAuthUrl(
            string consumerKey, string consumerSecret,
            string accessToken, string accessTokenSecret, 
            string url, string data = null)
        {
            var uri = new Uri(url);
            var webReq = (HttpWebRequest)WebRequest.Create(uri);
            webReq.Accept = MimeTypes.Json;

            if (accessToken != null)
            {
                webReq.Headers[HttpRequestHeader.Authorization] = OAuthAuthorizer.AuthorizeRequest(
                    consumerKey, consumerSecret, accessToken, accessTokenSecret, HttpMethods.Get, uri, data);
            }

            using (var webRes = PclExport.Instance.GetResponse(webReq))
                return webRes.ReadToEnd();
        }

        public string DownloadFacebookUserInfo(string facebookCode, params string[] fields)
        {
            facebookCode.ThrowIfNullOrEmpty("facebookCode");

            var url = FacebookUserUrl.Fmt(facebookCode);
            if (fields.Length > 0)
            {
                url = url.AddQueryParam("fields", string.Join(",", fields));
            }

            var json = url.GetStringFromUrl();
            return json;
        }

        /// <summary>
        /// Download Yammer User Info given its ID.
        /// </summary>
        /// <param name="yammerUserId">
        /// The Yammer User ID.
        /// </param>
        /// <returns>
        /// The User info in JSON format.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Yammer provides a method to retrieve current user information via
        /// "https://www.yammer.com/api/v1/users/current.json".
        /// </para>
        /// <para>
        /// However, to ensure consistency with the rest of the Auth codebase,
        /// the explicit URL will be used, where [:id] denotes the User ID: 
        /// "https://www.yammer.com/api/v1/users/[:id].json"
        /// </para>
        /// <para>
        /// Refer to: https://developer.yammer.com/restapi/ for full documentation.
        /// </para>
        /// </remarks>
        public string DownloadYammerUserInfo(string yammerUserId)
        {
            yammerUserId.ThrowIfNullOrEmpty("yammerUserId");

            var url = YammerUserUrl.Fmt(yammerUserId);
            var json = url.GetStringFromUrl();
            return json;
        }
    }
}
