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
    [PluginInfo(Name = "Authorize(Local)",
                Category = "SinaWeibo",
                Version = "0.0.1",
                Help = "Provide username_in & password_in to Authorize",
                Author = "agalloch21",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class WeiboAuthorizeOnLocal : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("AppKey", IsSingle = true)]
        ISpread<string> app_key_in;

        [Input("AppSecret", IsSingle = true)]
        ISpread<string> app_secret_in;

        [Input("Callback URL", IsSingle = true)]
        ISpread<string> callback_url_in;

        [Input("Username")]
        ISpread<string> username_in;

        [Input("Password")]
        ISpread<string> password_in;

        [Input("Authorize", IsSingle = true, DefaultValue = 0)]
        IDiffSpread<bool> authorize_in;

        [Output("Client")]
        ISpread<Client> client_out;

        [Import()]
        ILogger FLogger;
        #endregion fields & pins

       #region Evaluate

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (authorize_in.IsChanged && authorize_in[0] == true)
            {
                OAuth oauth = new OAuth(app_key_in[0], app_secret_in[0], callback_url_in[0]);
                oauth.ClientLogin(username_in[0], password_in[0]);
                if (!string.IsNullOrEmpty(oauth.AccessToken))
                {
                    client_out[0] = new Client(oauth);
                    FLogger.Log(LogType.Debug, "Login Weibo");
                }
                else
                {
                    client_out[0] = null;
                }
            }
            if (authorize_in[0] == false)
            {
                client_out[0] = null;
            }
        }
        #endregion
    }
}