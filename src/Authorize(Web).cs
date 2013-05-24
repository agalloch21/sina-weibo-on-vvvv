using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using NetDimension.Weibo;





namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Authorize(Web)",
                Category = "SinaWeibo",
                Version = "0.0.1",
                Help = "Authorize on Web",
                Author = "agalloch21",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class WeiboAuthorizeOnWeb : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("AppKey", IsSingle = true)]
        IDiffSpread<string> app_key_in;

        [Input("AppSecret", IsSingle = true)]
        IDiffSpread<string> app_secret_in;

        [Input("Callback URL", IsSingle = true)]
        IDiffSpread<string> callback_url_in;

        [Input("URL Code", IsSingle = true)]
        IDiffSpread<string> code_in;

        [Input("Authorize", IsSingle = true, IsBang = true, DefaultValue = 0)]
        IDiffSpread<bool> authorize_in;

        [Input("Login", IsSingle = true, DefaultValue = 0)]
        IDiffSpread<bool> login_in;

        [Input("Remember Me", IsSingle = true, DefaultValue = 0)]
        IDiffSpread<bool> remember_me_in;

        [Output("Client")]
        ISpread<Client> client_out;

        [Import()]
        ILogger FLogger;
        #endregion fields & pins

        private OAuth oauth = null;
        private bool is_first_run = true;

       #region Evaluate

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if ((app_key_in.IsChanged || app_secret_in.IsChanged || callback_url_in.IsChanged))
            {
                if (!string.IsNullOrEmpty(app_key_in[0]) && !string.IsNullOrEmpty(app_secret_in[0]) && !string.IsNullOrEmpty(callback_url_in[0]))
                    oauth = new OAuth(app_key_in[0], app_secret_in[0], callback_url_in[0]);
                else
                    oauth = null;
            }
            if (authorize_in.IsChanged && authorize_in[0] == true && oauth != null)
            {
                string authorize_url = oauth.GetAuthorizeURL();
                System.Diagnostics.Process.Start(authorize_url);
            }
            if (remember_me_in.IsChanged && remember_me_in[0] == false)
            {
                ClearData();
            }

            ////////////////////Login/////////////////////////////
            if (login_in[0] == true)
            {
                //Normal Login
                if (login_in.IsChanged && !string.IsNullOrEmpty(code_in[0]) && oauth != null && is_first_run == false)
                {
                    oauth.GetAccessTokenByAuthorizationCode(code_in[0]);
                    if (!string.IsNullOrEmpty(oauth.AccessToken))
                    {
                        client_out[0] = new Client(oauth);
                        FLogger.Log(LogType.Debug, "Login Weibo");

                        if (remember_me_in[0])
                            SaveData(app_key_in[0], app_secret_in[0], callback_url_in[0], oauth.AccessToken);
                        else
                            ClearData();
                    }
                    else
                    {
                        client_out[0] = null;
                    }
                }
                //Login using stored data
                else if (login_in.IsChanged && remember_me_in[0])
                {
                    
                    string app_key, app_secret, access_token;
                    app_key =  SinaWeibo.Properties.Settings.Default.AppKey;
                    app_secret = SinaWeibo.Properties.Settings.Default.AppSecrect;
                    access_token = SinaWeibo.Properties.Settings.Default.AccessToken;
                    if (!string.IsNullOrEmpty(app_key) && !string.IsNullOrEmpty(app_secret) && !string.IsNullOrEmpty(access_token))
                    {
                        oauth = new OAuth(app_key, app_secret, access_token, "");
                        TokenResult result = oauth.VerifierAccessToken();	//测试保存的AccessToken的有效性
                        if (result == TokenResult.Success)
                        {
                            client_out[0] = new Client(oauth);
                            FLogger.Log(LogType.Debug, "Login Weibo");
                        }
                        else
                        {
                            client_out[0] = null;
                        }
                    }
                    else
                    {
                        client_out[0] = null;
                    }      
                }
            }
            else
            {
                client_out[0] = null;
            }
            //说明一下, 这里这么多client_out[0] = null;的原因是:
            //Client client;
            //for(int i=0; i<10; i++)
            //  client_out[0] = client;
            //这段代码在vvvv中如果用IDiffSpread<T>.IsChanged检测的话会认为client_out改变了10次.


            is_first_run = false;
        }
        #endregion

        private void SaveData(string app_key, string app_secret, string callback_url, string access_token)
        {
            SinaWeibo.Properties.Settings.Default.AppKey = app_key;
            SinaWeibo.Properties.Settings.Default.AppSecrect = app_secret;
            SinaWeibo.Properties.Settings.Default.CallbackUrl = callback_url;
            SinaWeibo.Properties.Settings.Default.AccessToken = access_token;
            SinaWeibo.Properties.Settings.Default.Save();
        }
        private void ClearData()
        {
            SinaWeibo.Properties.Settings.Default.AppKey = string.Empty;
            SinaWeibo.Properties.Settings.Default.AppSecrect = string.Empty;
            SinaWeibo.Properties.Settings.Default.CallbackUrl = string.Empty;
            SinaWeibo.Properties.Settings.Default.AccessToken = string.Empty;
            SinaWeibo.Properties.Settings.Default.Save();
        }

    }
}