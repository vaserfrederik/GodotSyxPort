using System;

namespace Integrations
{
    internal sealed class RPCHandler
    {
        private static readonly long interval = 15 * 1000;
        private readonly Rpcer[] rpcs;
        private long lastUpdate = 0;

        public RPCHandler(SteamClient steam)
        {
            if (INTEGRATIONS.SteamRunning())
            {
                rpcs = new Rpcer[]
                {
                    new RPCDiscord(),
                    new SteamRPC(steam),
                };
            }
            else
            {
                rpcs = new Rpcer[]
                {
                    new RPCDiscord(),
                };
            }
        }

        public void Dispose()
        {
            foreach (Rpcer rpc in rpcs)
            {
                rpc.Dispose();
            }
        }

        public void Update(INTER_RPC rpc)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (now - lastUpdate > interval)
            {
                string state = rpc.rpcTitle();
                string[] ds = rpc.rpcDetails();
                string details = "";
                bool first = true;
                foreach (string d in ds)
                {
                    if (!first)
                    {
                        details += " | ";
                    }
                    else
                    {
                        first = false;
                    }
                    details += d;
                }

                foreach (Rpcer p in rpcs)
                {
                    p.Update(state, details);
                }
                lastUpdate = now;
            }
        }
    }
}