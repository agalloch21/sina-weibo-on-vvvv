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
    [PluginInfo(Name = "Profile",
                Category = "SinaWeibo",
                Help = "Show Profile",
                Author = "agalloch21")]
    #endregion PluginInfo
    public class WeiboProfile : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("Client", IsSingle = true)]
        IDiffSpread<SinaWeiboClient> client_in;

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

        [Import()]
        ILogger FLogger;
        #endregion fields & pins

       #region Evaluate


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            ScreenName_Out.SliceCount = 1;
            Location_Out.SliceCount = 1;
            Description_Out.SliceCount = 1;
            Gender_Out.SliceCount = 1;
            Url_Out.SliceCount = 1;
            ProfileImageUrl_Out.SliceCount = 1;
            StatusesCount_Out.SliceCount = 1;
            FriendsCount_Out.SliceCount = 1;
            FollowersCount_Out.SliceCount = 1;


            if (client_in.IsChanged && client_in.SliceCount > 0 && client_in[0] != null)
            {
                var response = client_in[0].HttpGet("users/show.json", new
                {
                    //可以传入一个Dictionary<string,object>类型的对象，也可以直接传入一个匿名类。参数与官方api文档中的参数相对应
                    uid = client_in[0].UID
                });

                if (response.IsSuccessStatusCode)
                {
                    response.Content.ReadAsStringAsync().ContinueWith((task) =>
                    {
                        var json = JObject.Parse(task.Result);

                        ScreenName_Out[0] = json.Value<string>("screen_name");
                        Location_Out[0] = json.Value<string>("location");
                        Description_Out[0] = json.Value<string>("description");
                        Gender_Out[0] = json.Value<string>("gender");
                        Url_Out[0] = json.Value<string>("url");
                        ProfileImageUrl_Out[0] = json.Value<string>("profile_image_url");
                        StatusesCount_Out[0] = json.Value<int>("statuses_count");
                        FriendsCount_Out[0] = json.Value<int>("friends_count");
                        FollowersCount_Out[0] = json.Value<int>("followers_count");
                    });
                }
                else
                {
                    FLogger.Log(LogType.Error, response.Content.ReadAsStringAsync().Result);
                }
            }

            if(client_in.SliceCount == 0 || client_in[0] == null)
            {
                ScreenName_Out[0] = "";
                Location_Out[0] = "";
                Description_Out[0] = "";
                Gender_Out[0] = "";
                Url_Out[0] = "";
                ProfileImageUrl_Out[0] = "";
                StatusesCount_Out[0] = 0;
                FriendsCount_Out[0] = 0;
                FollowersCount_Out[0] = 0;
            }
        }
        #endregion
    }
}