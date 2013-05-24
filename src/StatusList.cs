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
    [PluginInfo(Name = "StatusList",
                Category = "SinaWeibo",
                Version = "0.0.1",
                Help = "Show Statuses",
                Author = "agalloch21")]
    #endregion PluginInfo
    public class WeiboStatusList : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("Client", IsSingle = true)]
        IDiffSpread<NetDimension.Weibo.Client> client_in;

        [Input("Status Count", IsSingle = true, DefaultValue = 20, StepSize = 0)]
        IDiffSpread<int> status_count_in;

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

        string ScreenName;
        string Location;
        string Description;
        string Gender;
        string Url;
        string ProfileImageUrl;
        int StatusesCount;
        int FriendsCount;
        int FollowersCount;
        bool FollowMe;
        bool OnlineStatus;

       #region Evaluate


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            Html_Out.SliceCount = 1;

            if ( (client_in.IsChanged || update_in.IsChanged || status_count_in.IsChanged) 
                && client_in[0] != null 
                && status_count_in[0] > 0)
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
                StringBuilder statusBuilder = new StringBuilder();
                var json = client_in[0].API.Entity.Statuses.FriendsTimeline(count: status_count);
                if (json.Statuses != null)
                {
                    int output_count = 0;
                    foreach (var status in json.Statuses)
                    {
                        if (status.User == null)
                        {
                            output_count++;
                            continue;
                        }
                            
                        if (status.RetweetedStatus != null && status.RetweetedStatus.User != null)
                        {
                            ScreenName_Out[output_count] = status.User.ScreenName;
                            Text_Out[output_count] = status.Text;
                            RepostsCount_Out[output_count] = status.RepostsCount;
                            CommentsCount_Out[output_count] = status.CommentsCount;
                            OriginalAuthor_Out[output_count] = status.RetweetedStatus.User.ScreenName;
                            OriginalText_Out[output_count] = status.RetweetedStatus.Text;
                            ThumbnailPictureUrl_Out[output_count] = string.IsNullOrEmpty(status.RetweetedStatus.ThumbnailPictureUrl) ? "" : status.RetweetedStatus.ThumbnailPictureUrl;
                            OriginalTextRepostsCount_Out[output_count] = status.RetweetedStatus.RepostsCount;
                            OriginalTextCommentsCount_Out[output_count] = status.RetweetedStatus.CommentsCount;

                            statusBuilder.AppendFormat(repostPattern,
                                status.User.ProfileImageUrl,
                                status.User.ScreenName,
                                status.Text,
                                status.RetweetedStatus.User.ScreenName,
                                status.RetweetedStatus.Text,
                                string.IsNullOrEmpty(status.RetweetedStatus.ThumbnailPictureUrl) ? "" :
                                string.Format(imageParttern, status.RetweetedStatus.ThumbnailPictureUrl),
                                status.RetweetedStatus.RepostsCount,
                                status.RetweetedStatus.CommentsCount,
                                status.RepostsCount, status.CommentsCount);

                        }
                        else
                        {
                            ScreenName_Out[output_count] = status.User.ScreenName;
                            Text_Out[output_count] = status.Text;
                            RepostsCount_Out[output_count] = status.RepostsCount;
                            CommentsCount_Out[output_count] = status.CommentsCount;
                            OriginalAuthor_Out[output_count] = "";
                            OriginalText_Out[output_count] = "";
                            ThumbnailPictureUrl_Out[output_count] = string.IsNullOrEmpty(status.ThumbnailPictureUrl) ? "" : status.ThumbnailPictureUrl;
                            OriginalTextRepostsCount_Out[output_count] = 0;
                            OriginalTextCommentsCount_Out[output_count] = 0;

                            statusBuilder.AppendFormat(statusPattern,
                                status.User.ProfileImageUrl,
                                status.User.ScreenName,
                                status.Text,
                                string.IsNullOrEmpty(status.ThumbnailPictureUrl) ? "" :
                                string.Format(imageParttern, status.ThumbnailPictureUrl),
                                status.RepostsCount, status.CommentsCount);
                        }
                        output_count++;

                    }
                }//end of output

                Html_Out[0] = htmlPattern.Replace("<!--StatusesList-->", statusBuilder.ToString());
            }//end of update


            if (client_in[0] == null)
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