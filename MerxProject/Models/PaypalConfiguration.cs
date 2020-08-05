using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MerxProject.Models
{
    public class PaypalConfiguration
    {
        public readonly static string clientId;
        public readonly static string clientSecret;

        static PaypalConfiguration()
        {
            var config = getconfig();
            clientId = "AbfElrBsyfTCShSX2456cja0JTJYEC4O9i0g4ja9A4bMbhRq6RvypequPrIcJKG-9ef5l8uThhsqBhWo";
            clientSecret = "ENMn6DReUPxY4ERQ1DGFH3JYsKkQDYvOH3Drlq9Aspj5Jaap-vfinb5EosyTCJJavOdDIN7_vun087wX";
        }

        private static Dictionary<string, string> getconfig()
        {
            return PayPal.Api.ConfigManager.Instance.GetProperties();
        }

        private static string GetAccessToken()
        {
            string accessToken = new OAuthTokenCredential(clientId, clientSecret, getconfig()).GetAccessToken();
            return accessToken;
        }

        public static APIContext GetAPIContext()
        {
            APIContext aPIContext = new APIContext(GetAccessToken());
            aPIContext.Config = getconfig();
            return aPIContext;
        }
    }
}