using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace UserDetailsClient
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        /*
         {"ClientId":"a0789689-6dd6-4f8f-bacd-bef3bfbfcf4d",
         "SignInPolicy":"B2C_1_SignIn",
         "ResetPasswordPolicy":"B2C_1_PasswordReset",
         "Scopes":["offline_access","openid","https://msoisalesstaging.onmicrosoft.com/mongodb-realm/sync"],
         "RedirectUri":"msala0789689-6dd6-4f8f-bacd-bef3bfbfcf4d://auth",
         "AuthoritySignIn":"https://msoisalesstaging.b2clogin.com/tfp/msoisalesstaging.onmicrosoft.com/B2C_1_SignIn/",
         "AuthorityPasswordReset":"https://msoisalesstaging.b2clogin.com/tfp/msoisalesstaging.onmicrosoft.com/B2C_1_PasswordReset",
         "IosKeyChainGroup":"com.microsoft.adalcache"}
         */
        async void OnSignInSignOut(object sender, EventArgs e)
        {
            AuthenticationResult authResult = null;
            IEnumerable<IAccount> accounts = await App.PCA.GetAccountsAsync().ConfigureAwait(false);
            try
            {
                if (btnSignInSignOut.Text == "Sign in")
                {
                    try
                    {
                        IAccount firstAccount = accounts.FirstOrDefault(account =>
                        {
                            string userIdentifier = account.HomeAccountId.ObjectId.Split('.')[0];
                            return userIdentifier.EndsWith("B2C_1_SignIn".ToLower());
                        });
                        authResult = await App.PCA.AcquireTokenSilent(App.Scopes, firstAccount)
                                              .WithB2CAuthority("https://msoisalesstaging.b2clogin.com/tfp/msoisalesstaging.onmicrosoft.com/B2C_1_SignIn/")
                                              .ExecuteAsync()
                                              .ConfigureAwait(false);
                    }
                    catch (MsalUiRequiredException)
                    {
                        try
                        { 
                            var builder = App.PCA.AcquireTokenInteractive(App.Scopes)
                                             .WithAuthority("https://msoisalesstaging.b2clogin.com/tfp/msoisalesstaging.onmicrosoft.com/B2C_1_SignIn/")
                                                                       .WithParentActivityOrWindow(App.ParentWindow);

                            if (Device.RuntimePlatform != "UWP")
                            {
                                // on Android and iOS, prefer to use the system browser, which does not exist on UWP
                                SystemWebViewOptions systemWebViewOptions = new SystemWebViewOptions()
                                {                            
                                    iOSHidePrivacyPrompt = true,
                                };

                                builder.WithSystemWebViewOptions(systemWebViewOptions);
                                builder.WithUseEmbeddedWebView(false);
                            }

                            authResult = await builder.ExecuteAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex2)
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                await DisplayAlert("Acquire token interactive failed. See exception message for details: ", ex2.Message, "Dismiss");
                            });
                        }
                    }

                    if (authResult != null)
                    {
                        var content = await GetHttpContentWithTokenAsync(authResult.AccessToken);
                        UpdateUserContent(content);
                    }
                }
                else
                {
                    while (accounts.Any())
                    {
                        await App.PCA.RemoveAsync(accounts.FirstOrDefault()).ConfigureAwait(false);
                        accounts = await App.PCA.GetAccountsAsync().ConfigureAwait(false);
                    }

                    
                    Device.BeginInvokeOnMainThread(() => 
                    {
                        slUser.IsVisible = false;
                        btnSignInSignOut.Text = "Sign in"; 
                    });
                }
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Authentication failed. See exception message for details: ", ex.Message, "Dismiss");
                });
            }
        }
        
        private void UpdateUserContent(string content)
        {
            if(!string.IsNullOrEmpty(content))
            {
                JObject user = JObject.Parse(content);

                Device.BeginInvokeOnMainThread(() =>
                {
                    slUser.IsVisible = true;

                    lblDisplayName.Text = user["displayName"].ToString();
                    lblGivenName.Text = user["givenName"].ToString();
                    lblId.Text = user["id"].ToString();
                    lblSurname.Text = user["surname"].ToString();
                    lblUserPrincipalName.Text = user["userPrincipalName"].ToString();

                    btnSignInSignOut.Text = "Sign out";
                });
            }
        }

        public async Task<string> GetHttpContentWithTokenAsync(string token)
        {
            try
            {
                //get data from API
                HttpClient client = new HttpClient();
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.SendAsync(message).ConfigureAwait(false);
                string responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return responseString;
            }
            catch(Exception ex)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("API call to graph failed: ", ex.Message, "Dismiss").ConfigureAwait(false);
                });
                return ex.ToString();
            }
        }
    }
}
