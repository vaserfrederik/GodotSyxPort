using System;
using DiscordRPC;

namespace Integrations
{
    class RPCDiscord : Rpcer
    {
        private DiscordRpcClient rpcClient;
        // to set time elapsed to total gametime since start
        private long startTime;

        public RPCDiscord()
        {
            rpcClient = new DiscordRpcClient("618471189722955807");
            startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            rpcClient.Initialize();
        }

        public override void Update(string state, string details)
        {
            var presence = new RichPresence
            {
                State = state,
                Details = details,
                StartTimestamp = startTime,
                SmallImageKey = "city4"
            };

            rpcClient.SetPresence(presence);
        }

        // call this whenever the game is closed
        public override void Dispose()
        {
            rpcClient.Dispose();
        }
    }
}