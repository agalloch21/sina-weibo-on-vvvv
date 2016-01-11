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

using NetDimension.OpenAuth.Sina;
using NetDimension.OpenAuth.Winform;





namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Authorize",
                Category = "SinaWeibo",
                Help = "Authorize on Web",
                Author = "agalloch21",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class Authorize : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("AppKey", IsSingle = true)]
        IDiffSpread<string> app_key_in;

        [Input("AppSecret", IsSingle = true)]
        IDiffSpread<string> app_secret_in;

        [Input("Callback URL", IsSingle = true)]
        IDiffSpread<string> callback_url_in;

        [Input("Authorize", IsSingle = true, IsBang = true, DefaultValue = 0)]
        IDiffSpread<bool> authorize_in;

        [Output("Client")]
        ISpread<SinaWeiboClient> client_out;

        [Output("Succeed")]
        ISpread<bool> succeed_out;

        [Import()]
        ILogger FLogger;
        #endregion fields & pins

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            client_out.SliceCount = 1;
            succeed_out.SliceCount = 1;
            if(authorize_in.SliceCount > 0 && authorize_in[0] && app_key_in[0].Length > 0 && app_secret_in[0].Length > 0 && callback_url_in[0].Length > 0)
            {
                //weiboClient = new SinaWeiboClient("1402038860", "62e1ddd4f6bc33077c796d5129047ca2", "http://qcyn.sina.com.cn");
                SinaWeiboClient weiboClient = new SinaWeiboClient(app_key_in[0], app_secret_in[0], callback_url_in[0]);
                AuthenticationForm form = weiboClient.GetAuthenticationForm();

                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    client_out[0] = weiboClient;
                    succeed_out[0] = true;
                    FLogger.Log(LogType.Debug, "Authorize Succeed");
                    FLogger.Log(LogType.Debug, weiboClient.AccessToken);
                }
                else
                {
                    client_out[0] = null;
                    succeed_out[0] = false;
                    FLogger.Log(LogType.Debug, "Authorize Failed");
                }
            }
        }
        #endregion

       
    }
}
