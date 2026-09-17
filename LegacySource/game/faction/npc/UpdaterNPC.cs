using game.faction;
using game.faction.trade;
using game.time;
using init.sprite;
using world.region;

namespace game.faction.npc
{
    public sealed class UpdaterNPC
    {
        public void Init(TradeManager trade)
        {
            SPRITES.Loader().Print("Simulating factions...");

            int a = 50;

            for (int i = 0; i < a; i++)
            {
                SPRITES.Loader().Print("Simulating factions " + (int)(100 * ((i * 2 + a * 2) / (double)(a * 4))) + "%");

                foreach (FactionNPC f in FACTIONS.NPCs())
                {
                    if (!f.IsActive())
                        continue;
                    RD.UPDATER().ShipAll(f, 1.0);
                    f.Stockpile.Update(f, TIME.SecondsPerDay());
                }

                if (i % 4 == 0)
                {
                    trade.Prime();
                }
            }
            trade.Prime();
        }
    }
}