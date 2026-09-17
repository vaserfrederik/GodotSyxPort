using System;
using Codedisaster.Steamworks;

namespace Integrations
{
    internal class SteamRPC : Rpcer
    {
        private readonly SteamFriends friends;
        private readonly SteamClient steam;

        public SteamRPC(SteamClient steam)
        {
            this.steam = steam;
            friends = new SteamFriends(friendsCallback);
        }

        protected override void Dispose()
        {
            friends.Dispose();
        }

        public override void Update(string state, string details)
        {
            if (steam.Running)
            {
                try
                {
                    if (details != null)
                        friends.SetRichPresence("text", state + " | " + details);
                    else
                        friends.SetRichPresence("text", state);
                    friends.SetRichPresence("steam_display", "#StatusFull");
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                    throw new RuntimeException("Something is wrong with RPC. Please uncheck RPC in the launcher and see if it helps.");
                }
            }
        }

        private readonly SteamFriendsCallback friendsCallback = new SteamFriendsCallback
        {
            OnSetPersonaNameResponse = (success, localSuccess, result) =>
            {
            },

            OnPersonaStateChange = (steamID, change) =>
            {
            },

            OnGameOverlayActivated = active =>
            {
            },

            OnGameLobbyJoinRequested = (steamIDLobby, steamIDFriend) =>
            {
            },

            OnAvatarImageLoaded = (steamID, image, width, height) =>
            {
            },

            OnFriendRichPresenceUpdate = (steamIDFriend, appID) =>
            {
            },

            OnGameRichPresenceJoinRequested = (steamIDFriend, connect) =>
            {
            },

            OnGameServerChangeRequested = (server, password) =>
            {
            }
        };
    }
}