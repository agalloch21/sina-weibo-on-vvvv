using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using NetDimension.Weibo;


namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Publish",
                Category = "SinaWeibo",
                Version = "0.0.1",
                Help = "Publish a status",
                Author = "agalloch21",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class WeiboPublish : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("Client", IsSingle = true)]
        ISpread<NetDimension.Weibo.Client> client;

        [Input("Text", IsSingle = true)]
        ISpread<string> text;

        [Input("Publish", IsSingle = true, DefaultValue = 0)]
        IDiffSpread<bool> publish;

        [Import()]
        ILogger FLogger;
        #endregion fields & pins

       #region Evaluate

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (client[0] != null && publish.IsChanged && publish[0] == true)
            {
               client[0].API.Entity.Statuses.Update(text[0]);
               FLogger.Log(LogType.Debug, "publish");
            }
            
        }
        #endregion
    }
}