using Microsoft.Identity.Client;
using System;
using Xamarin.Forms;

namespace UserDetailsClient
{
    public class App : Application
    {
        public static IPublicClientApplication PCA = null;

        /// <summary>
        /// The ClientID is the Application ID found in the portal (https://go.microsoft.com/fwlink/?linkid=2083908). 
        /// You can use the below id however if you create an app of your own you should replace the value here.
        /// </summary>
        public static string ClientID = "a0789689-6dd6-4f8f-bacd-bef3bfbfcf4d"; //msidentity-samples-testing tenant

        public static string[] Scopes = { "offline_access", "openid", "https://msoisalesstaging.onmicrosoft.com/msoisales/mongodb-realm-sync" };
        public static string Username = string.Empty;

        public static object ParentWindow { get; set; }

        public App(string specialRedirectUri = null)
        {
            PCA = PublicClientApplicationBuilder.Create(ClientID)
                                                .WithB2CAuthority("https://msoisalesstaging.b2clogin.com/tfp/msoisalesstaging.onmicrosoft.com/B2C_1_SignIn/")
                                                .WithRedirectUri(specialRedirectUri ?? $"msal{ClientID}://auth")
                                                .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                                                .Build();

            MainPage = new NavigationPage(new UserDetailsClient.MainPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}