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
    [PluginInfo(Name = "UserInfo",
                Category = "SinaWeibo",
                Version = "0.0.1",
                Help = "Show Account Info",
                Author = "agalloch21")]
    #endregion PluginInfo
    public class WeiboUserInfo : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("Client", IsSingle = true)]
        IDiffSpread<NetDimension.Weibo.Client> client_in;

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
            if ((client_in.IsChanged || username_in.IsChanged) && client_in[0] != null && username_in.SliceCount > 0)
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
                    try
                    {
                        var entity_user_info = client_in[0].API.Entity.Users.Show("", username_in[i]);
                        ScreenName_Out[i] = entity_user_info.ScreenName;
                        Location_Out[i] = entity_user_info.Location;
                        Description_Out[i] = entity_user_info.Description;
                        Gender_Out[i] = entity_user_info.Gender;
                        Url_Out[i] = entity_user_info.Url;
                        ProfileImageUrl_Out[i] = entity_user_info.ProfileImageUrl;
                        StatusesCount_Out[i] = entity_user_info.StatusesCount;
                        FriendsCount_Out[i] = entity_user_info.FriendsCount;
                        FollowersCount_Out[i] = entity_user_info.FollowersCount;
                        FollowMe_Out[i] = entity_user_info.FollowMe;
                        OnlineStatus_Out[i] = entity_user_info.OnlineStatus == 1 ? true : false;
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            if (client_in[0] == null || username_in.SliceCount <= 0)
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