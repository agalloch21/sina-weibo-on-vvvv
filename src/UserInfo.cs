using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;


using NetDimension.OpenAuth.Sina;
using NetDimension.OpenAuth.Winform;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "UserInfo",
                Category = "SinaWeibo",
                Help = "Show Account Info",
                Author = "agalloch21")]
    #endregion PluginInfo
    public class WeiboUserInfo : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("Client", IsSingle = true)]
        IDiffSpread<SinaWeiboClient> client_in;

        [Input("UserName")]
        IDiffSpread<string> username_in;

        [Output("ScreenName")]
        ISpread<string> ScreenName_Out;

        [Output("Location")]
        ISpread<string> Location_Out;

        [Output("Description")]
        ISpread<string> Description_Out;

        [Output("Gender")]
        ISpread<string> Gender_Out;

        [Output("Url", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> Url_Out;

        [Output("ProfileImageUrl", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> ProfileImageUrl_Out;

        [Output("StatusesCount", Visibility = PinVisibility.OnlyInspector)]
        ISpread<int> StatusesCount_Out;

        [Output("FriendsCount", Visibility = PinVisibility.OnlyInspector)]
        ISpread<int> FriendsCount_Out;

        [Output("FollowersCount", Visibility = PinVisibility.OnlyInspector)]
        ISpread<int> FollowersCount_Out;

        [Output("FollowMe", Visibility = PinVisibility.OnlyInspector)]
        ISpread<bool> FollowMe_Out;

        [Output("OnlineStatus", Visibility = PinVisibility.OnlyInspector)]
        ISpread<bool> OnlineStatus_Out;

        [Import()]
        ILogger FLogger;
        #endregion fields & pins

       #region Evaluate


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if ((client_in.IsChanged || username_in.IsChanged) && client_in.SliceCount > 0 && client_in[0] != null && username_in.SliceCount > 0)
            {
                ScreenName_Out.SliceCount = SpreadMax;
                Location_Out.SliceCount = SpreadMax;
                Description_Out.SliceCount = SpreadMax;
                Gender_Out.SliceCount = SpreadMax;
                Url_Out.SliceCount = SpreadMax;
                ProfileImageUrl_Out.SliceCount = SpreadMax;
                StatusesCount_Out.SliceCount = SpreadMax;
                FriendsCount_Out.SliceCount = SpreadMax;
                FollowersCount_Out.SliceCount = SpreadMax;
                FollowMe_Out.SliceCount = SpreadMax;
                OnlineStatus_Out.SliceCount = SpreadMax;
                for (int i = 0; i < username_in.SliceCount; i++)
                {
                    ScreenName_Out[i] = "";
                    Location_Out[i] = "";
                    Description_Out[i] = "";
                    Gender_Out[i] = "";
                    Url_Out[i] = "";
                    ProfileImageUrl_Out[i] = "";
                    StatusesCount_Out[i] = 0;
                    FriendsCount_Out[i] = 0;
                    FollowersCount_Out[i] = 0;
                    FollowMe_Out[i] = false;
                    OnlineStatus_Out[i] = false;
                }
                for (int i = 0; i < username_in.SliceCount; i++)
                {
                    var response = client_in[0].HttpGet("users/show.json", new
                    {
                        //可以传入一个Dictionary<string,object>类型的对象，也可以直接传入一个匿名类。参数与官方api文档中的参数相对应
                        screen_name = username_in[i]
                    });


                    if (response.IsSuccessStatusCode)
                    {
                        response.Content.ReadAsStringAsync().ContinueWith((task) =>
                        {
                            var json = JObject.Parse(task.Result);
                            for (int j = 0; j < username_in.SliceCount; j++ )
                            {
                                if (username_in[j] == json.Value<string>("screen_name"))
                                {
                                    ScreenName_Out[j] = json.Value<string>("screen_name");
                                    Location_Out[j] = json.Value<string>("location");
                                    Description_Out[j] = json.Value<string>("description");
                                    Gender_Out[j] = json.Value<string>("gender");
                                    Url_Out[j] = json.Value<string>("url");
                                    ProfileImageUrl_Out[j] = json.Value<string>("profile_image_url");
                                    StatusesCount_Out[j] = json.Value<int>("statuses_count");
                                    FriendsCount_Out[j] = json.Value<int>("friends_count");
                                    FollowersCount_Out[j] = json.Value<int>("followers_count");
                                    FollowMe_Out[j] = json.Value<bool>("follow_me");
                                    OnlineStatus_Out[j] = json.Value<bool>("online_status");
                                }
                            }
                               
                        });
                    }
                    else
                    {
                        FLogger.Log(LogType.Debug, response.Content.ReadAsStringAsync().Result);
                    }
                }
            }

            if (client_in.SliceCount == 0 || client_in[0] == null || username_in.SliceCount <= 0)
            {
                ScreenName_Out.SliceCount = 0;
                Location_Out.SliceCount = 0;
                Description_Out.SliceCount = 0;
                Gender_Out.SliceCount = 0;
                Url_Out.SliceCount = 0;
                ProfileImageUrl_Out.SliceCount = 0;
                StatusesCount_Out.SliceCount = 0;
                FriendsCount_Out.SliceCount = 0;
                FollowersCount_Out.SliceCount = 0;
                FollowMe_Out.SliceCount = 0;
                OnlineStatus_Out.SliceCount = 0;
            }
        }
        #endregion
    }
}