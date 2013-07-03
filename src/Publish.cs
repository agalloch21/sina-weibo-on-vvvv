using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Diagnostics;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;


using NetDimension.Weibo;


namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Publish",
                Category = "SinaWeibo",
                Help = "Publish a status",
                Author = "agalloch21",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class WeiboPublish : IPluginEvaluate
    {
        #region fields & pins


        class Info
        {
            public int Slice;
            public int Width;
            public int Height;
            public double WaveCount;
            public byte[] Data;
        }

        //vvvv
        [Input("Client", IsSingle = true)]
        ISpread<NetDimension.Weibo.Client> client;

        [Input("Text", IsSingle = true)]
        ISpread<string> text;

        [Input("Image Path", IsSingle = true, StringType = StringType.Filename, FileMask = "Image Files|*.jpg;*.jpeg;*.png;*.gif")]
        ISpread<string> image_path;

        [Input("Publish", IsSingle = true, DefaultValue = 0, IsBang = true)]
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
               
                if(image_path[0].Length == 0)
                {
                    client[0].API.Entity.Statuses.Update(text[0]);
                    FLogger.Log(LogType.Debug, "publish text");
                }
                else
                {
                    if (image_path[0].Substring(0, 4) == "http")
                    {
                        client[0].API.Entity.Statuses.UploadUrlText(text[0], image_path[0]);
                        FLogger.Log(LogType.Debug, "publish text with url-image");
                    }
                    else
                    {
                        byte[] data = System.IO.File.ReadAllBytes(image_path[0]);
                        if (data != null)
                        {
                            client[0].API.Entity.Statuses.Upload(text[0], data);
                            FLogger.Log(LogType.Debug, "publish text with local-image");
                        }
                        else
                        {
                            client[0].API.Entity.Statuses.Update(text[0]);
                            FLogger.Log(LogType.Debug, "publish text");
                        }
                    }
                }
            }
            
        }
        #endregion
    }
}