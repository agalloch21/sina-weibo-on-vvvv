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


using NetDimension.OpenAuth.Sina;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

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


        //vvvv
        [Input("Client", IsSingle = true)]
        ISpread<SinaWeiboClient> client;

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
            if (client.SliceCount > 0 && client[0] != null && publish.IsChanged && publish.SliceCount > 0 && publish[0] == true)
            {
               
                //text status
                if (image_path[0].Length == 0 || File.Exists(image_path[0]) == false)
                {
                    // 调用发微博api
                    // 参考：http://open.weibo.com/wiki/2/statuses/update
                    client[0].HttpPostAsync("statuses/update.json", new
                    {
                        status = text[0]
                    }).ContinueWith(task =>
                    {
                        //这里用了个异步方法，发微博不阻塞主线程，任务完成后调用处理方法
                        StatusPosted(task);
                    });
                }
                else if (File.Exists(image_path[0]))
                {
                    byte[] data = System.IO.File.ReadAllBytes(image_path[0]);
                    if (data != null)
                    {
                        // 调用发图片微博api
                        // 参考：http://open.weibo.com/wiki/2/statuses/upload
                        client[0].HttpPostAsync("statuses/upload.json", new Dictionary<string, object> 
				        //当然，这里用匿名类也是可以的
				        /*
					        匿名类传参方式：
				         * new { status = txtStatus.Text, pic = imageFile }
				         */
				        {
					        {"status" ,text[0]},
					        {"pic" , data} //imgFile: 对于文件上传，这里可以直接传FileInfo对象
				        }).ContinueWith(task =>
                        {
                            //这里用了个异步方法，发微博不阻塞主线程，任务完成后调用处理方法
                            StatusPosted(task);
                        });
                    }
                    else
                    {
                        // 调用发微博api
                        // 参考：http://open.weibo.com/wiki/2/statuses/update
                        client[0].HttpPostAsync("statuses/update.json", new
                        {
                            status = text[0]
                        }).ContinueWith(task =>
                        {
                            //这里用了个异步方法，发微博不阻塞主线程，任务完成后调用处理方法
                            StatusPosted(task);
                        });
                    }
               
                }
            }
            
        }
        #endregion


        private void StatusPosted(Task<HttpResponseMessage> task)
        {
            var result = task.Result;
            if (result.IsSuccessStatusCode)
            {
                FLogger.Log(LogType.Debug, "Publish Status Done");
            }
            else
            {
                FLogger.Log(LogType.Debug, "Publish Status Failed");
                FLogger.Log(LogType.Debug, result.Content.ReadAsStringAsync().Result);
            }
        }
    }
}