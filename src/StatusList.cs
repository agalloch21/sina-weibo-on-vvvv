using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net.Http;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;


using NetDimension.OpenAuth.Sina;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "StatusList",
                Category = "SinaWeibo",
                Help = "Show Statuses",
                Author = "agalloch21")]
    #endregion PluginInfo
    public class WeiboStatusList : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("Client", IsSingle = true)]
        IDiffSpread<SinaWeiboClient> client_in;

        [Input("Count", IsSingle = true, DefaultValue = 20, StepSize = 0, MinValue = 1)]
        IDiffSpread<int> status_count_in;

        [Input("Page", IsSingle = true, DefaultValue = 1, StepSize = 0, MinValue = 1)]
        IDiffSpread<int> status_page_in;

        [Input("Update", IsSingle = true, IsBang = true)]
        IDiffSpread<bool> update_in;

        [Output("ScreenName")]
        ISpread<string> ScreenName_Out;

        [Output("Text")]
        ISpread<string> Text_Out;

        [Output("RepostsCount")]
        ISpread<int> RepostsCount_Out;

        [Output("CommentsCount")]
        ISpread<int> CommentsCount_Out;

        [Output("OriginalAuthor")]
        ISpread<string> OriginalAuthor_Out;

        [Output("OriginalText")]
        ISpread<string> OriginalText_Out;

        [Output("ThumbnailPictureUrl")]
        ISpread<string> ThumbnailPictureUrl_Out;

        [Output("OriginalTextRepostsCount")]
        ISpread<int> OriginalTextRepostsCount_Out;

        [Output("OriginalTextCommentsCount")]
        ISpread<int> OriginalTextCommentsCount_Out;

        [Output("Html")]
        ISpread<string> Html_Out;

        [Import()]
        ILogger FLogger;
        #endregion fields & pins

       #region Evaluate


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            Html_Out.SliceCount = 1;

            if ((client_in.IsChanged || update_in.IsChanged || status_count_in.IsChanged || status_page_in.IsChanged) 
                && client_in.SliceCount > 0 && client_in[0] != null 
                && status_count_in.SliceCount > 0 && status_count_in[0] > 0)
            {

                //initialize
                int status_count = status_count_in[0];
                ScreenName_Out.SliceCount = status_count;
                Text_Out.SliceCount = status_count;
                RepostsCount_Out.SliceCount = status_count;
                CommentsCount_Out.SliceCount = status_count;
                OriginalAuthor_Out.SliceCount = status_count;
                OriginalText_Out.SliceCount = status_count;
                ThumbnailPictureUrl_Out.SliceCount = status_count;
                OriginalTextRepostsCount_Out.SliceCount = status_count;
                OriginalTextCommentsCount_Out.SliceCount = status_count;

                for (int i = 0; i < status_count; i++ )
                {
                    ScreenName_Out[i] = "";
                    Text_Out[i] = "";
                    RepostsCount_Out[i] = 0;
                    CommentsCount_Out[i] = 0;
                    OriginalAuthor_Out[i] = "";
                    OriginalText_Out[i] = "";
                    ThumbnailPictureUrl_Out[i] = "";
                    OriginalTextRepostsCount_Out[i] = 0;
                    OriginalTextCommentsCount_Out[i] = 0;
                }
                Html_Out[0] = "";

                //Output
                // 调用获取当前登录用户及其所关注用户的最新微博api
                // 参考：http://open.weibo.com/wiki/2/statuses/friends_timeline
                client_in[0].HttpGetAsync("statuses/home_timeline.json", new Dictionary<string, object>
			    {	
				    //可以传入一个Dictionary<string,object>类型的对象，也可以直接传入一个匿名类。参数与官方api文档中的参数相对应
				    {"count",status_count}
			    }).ContinueWith(task =>
                {
                    if (task.Result.IsSuccessStatusCode)
                    {
                        StringBuilder statusBuilder = new StringBuilder();

                        //解析微博开放平台返回的json数据
                        var json = JObject.Parse(task.Result.Content.ReadAsStringAsync().Result);
                        int output_count = 0;
                        foreach (JObject status in json.Value<JArray>("statuses"))
                        {
                            if (status["user"] == null)
                            {
                                output_count++;
                                continue;
                            }
                                

                            if (status["retweeted_status"] != null && status["retweeted_status"]["user"] != null)
                            {
                                JObject user = status.Value<JObject>("user");
                                JObject retweeted = status.Value<JObject>("retweeted_status");
                                JObject retweeted_user = retweeted.Value<JObject>("user");

                                ScreenName_Out[output_count] = user.Value<string>("screen_name");
                                Text_Out[output_count] = retweeted.Value<string>("text");
                                RepostsCount_Out[output_count] = retweeted.Value<int>("reposts_count");
                                CommentsCount_Out[output_count] = retweeted.Value<int>("comments_count");
                                OriginalAuthor_Out[output_count] = retweeted_user.Value<string>("screen_name");
                                OriginalText_Out[output_count] = status.Value<string>("text");
                                ThumbnailPictureUrl_Out[output_count] = retweeted.Value<string>("thumbnail_pic");
                                OriginalTextRepostsCount_Out[output_count] = status.Value<int>("reposts_count");
                                OriginalTextCommentsCount_Out[output_count] = status.Value<int>("comments_count");

                                statusBuilder.AppendFormat(repostPattern,
                                    status["user"]["profile_image_url"],
                                    status["user"]["screen_name"],
                                    status["text"],
                                    status["retweeted_status"]["user"]["screen_name"],
                                    status["retweeted_status"]["text"],
                                    status["retweeted_status"]["thumbnail_pic"] == null ? "" : string.Format(imageParttern, status["retweeted_status"]["thumbnail_pic"]),
                                    status["retweeted_status"]["reposts_count"],
                                    status["retweeted_status"]["comments_count"],
                                    status["reposts_count"],
                                    status["comments_count"]);
                            }
                            else
                            {
                                JObject user = status.Value<JObject>("user");
 
                                ScreenName_Out[output_count] = user.Value<string>("screen_name");
                                Text_Out[output_count] = status.Value<string>("text");
                                RepostsCount_Out[output_count] = status.Value<int>("reposts_count");
                                CommentsCount_Out[output_count] = status.Value<int>("comments_count");
                                OriginalAuthor_Out[output_count] = "";
                                OriginalText_Out[output_count] = "";
                                ThumbnailPictureUrl_Out[output_count] = status.Value<string>("thumbnail_pic");
                                OriginalTextRepostsCount_Out[output_count] = 0;
                                OriginalTextCommentsCount_Out[output_count] = 0;

                                statusBuilder.AppendFormat(statusPattern,
                                    status["user"]["profile_image_url"],
                                    status["user"]["screen_name"],
                                    status["text"],
                                    status["thumbnail_pic"] == null ? "" : string.Format(imageParttern, status["thumbnail_pic"]),
                                    status["reposts_count"],
                                    status["comments_count"]);
                            }

                            output_count++;

                        }

                        var html = htmlPattern.Replace("<!--StatusesList-->", statusBuilder.ToString());
                        Html_Out[0] = html;
                    }
                    else
                    {
                        FLogger.Log(LogType.Debug, task.Result.Content.ReadAsStringAsync().Result);
                    }

                });
            }//end of update


            if (client_in.SliceCount == 0 || client_in[0] == null)
            {
                ScreenName_Out.SliceCount = 0;
                Text_Out.SliceCount = 0;
                RepostsCount_Out.SliceCount = 0;
                CommentsCount_Out.SliceCount = 0;
                OriginalAuthor_Out.SliceCount = 0;
                OriginalText_Out.SliceCount = 0;
                ThumbnailPictureUrl_Out.SliceCount = 0;
                OriginalTextRepostsCount_Out.SliceCount = 0;
                OriginalTextCommentsCount_Out.SliceCount = 0;
                Html_Out[0] = "";
            }
        }
        #endregion

        #region 微博列表的模板
        const string htmlPattern = @"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
	<title></title>
	<style type=""text/css"">
		html, body {
			font-size: 12px;
			cursor: default;
			padding: 5px;
			margin: 0;
			font-family:微软雅黑,Tahoma;
		}

		div.status {
			padding-left: 60px;
			position: relative;
			margin-bottom: 10px;
			min-height:80px;
			_height:80px;
		}

			div.status p {
				margin: 0 0 5px 0;
				line-height: 1.5;
				padding: 0;
			}

				div.status p span.name {
					color: #369;
				}

				div.status p.status-content {
					color: #333;
				}

				div.status p.status-count {
					color:#999;
				}

			div.status .face {
				position: absolute;
				left: 0;
				top: 0;
			}

			div.status div.repost {
				border: solid 1px #ACD;
				background: #F0FAFF;
				padding: 10px 10px 0 10px;
			}

		div.repost p.repost-content {
			color: #666 !important;
		}
	</style>
</head>
<body>
<!--StatusesList-->
</body>
</html>";
        const string imageParttern = @"<img src=""{0}"" alt=""图片"" class=""inner-pic"" />";
        const string statusPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-content""><span class=""name"">{1}</span>：{2}</p>
		{3}
		<p class=""status-count"">转发({4}) 评论({5})</p>
	</div>
";
        const string repostPattern = @"	<div class=""status"">
		<img src=""{0}"" alt=""{1}"" class=""face"" />
		<p class=""status-content""><span class=""name"">{1}</span>：{2}</p>
		<div class=""repost"">
			<p class=""repost-cotent""><span class=""name"">@{3}</span>：{4}</p>
			{5}
			<p class=""status-count"">转发({6}) 评论({7})</p>
		</div>
		<p class=""status-count"">转发({8}) 评论({9})</p>
	</div>
";
        #endregion
    }
}