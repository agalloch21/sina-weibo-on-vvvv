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
    [PluginInfo(Name = "Profile",
                Category = "SinaWeibo",
                Version = "0.0.1",
                Help = "Show Profile",
                Author = "agalloch21")]
    #endregion PluginInfo
    public class WeiboProfile : IPluginEvaluate
    {
        #region fields & pins
        //vvvv
        [Input("Client", IsSingle = true)]
        IDiffSpread<NetDimension.Weibo.Client> client_in;

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

        string ScreenName;
        string Location;
        string Description;
        string Gender;
        string Url;
        string ProfileImageUrl;
        int StatusesCount;
        int FriendsCount;
        int FollowersCount;

       #region Evaluate


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (client_in.IsChanged && client_in[0] != null)
            {
                Client client = client_in[0];
                string uid = client.API.Entity.Account.GetUID();
                var entity_user_info = client.API.Entity.Users.Show(uid);
                ScreenName  = entity_user_info.ScreenName;
                Location = entity_user_info.Location;
                Description = entity_user_info.Description;
                Gender = entity_user_info.Gender;
                Url = entity_user_info.Url;
                ProfileImageUrl = entity_user_info.ProfileImageUrl;
                StatusesCount = entity_user_info.StatusesCount;
                FriendsCount = entity_user_info.FriendsCount;
                FollowersCount = entity_user_info.FollowersCount;
            }
            if(client_in[0] == null)
            {
                ScreenName = "";
                Location = "";
                Description = "";
                Gender = "";
                Url = "";
                ProfileImageUrl = "";
                StatusesCount = 0;
                FriendsCount = 0;
                FollowersCount = 0;
            }

            ScreenName_Out[0] = ScreenName;
            Location_Out[0] = Location;
            Description_Out[0] = Description;
            Gender_Out[0] = Gender;
            Url_Out[0] = Url;
            ProfileImageUrl_Out[0] = ProfileImageUrl;
            StatusesCount_Out[0] = StatusesCount;
            FriendsCount_Out[0] = FriendsCount;
            FollowersCount_Out[0] = FollowersCount;
        }
        #endregion
    }
}