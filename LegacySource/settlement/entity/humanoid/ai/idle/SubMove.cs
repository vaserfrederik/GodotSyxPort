using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.idle
{
    class SubMove : AISUB.Simple
    {
        public SubMove(string key) : base(key)
        {
        }

        public override AISTATE Resume(Humanoid a, AIManager d)
        {
            if (!a.speed.IsZero())
            {
                return AI.STATES().STOP.Instant(a, d);
            }

            switch (d.subByte)
            {
                case 0:
                    d.subByte = 1;
                    if (AI.STATES().WALK2.CTileNeeds(a, d))
                    {
                        return AI.STATES().WALK2.CTile(a, d);
                    }
                    return AI.STATES().STOP.Instant(a, d);
                case 1:
                    DIR di = DIR.ALL.Rnd();
                    int x1 = a.physics.TileC().x();
                    int y1 = a.physics.TileC().y();
                    d.subByte = 2;
                    double c = PATH().coster.player.GetCost(x1, y1, x1 + di.x(), y1 + di.y());
                    if (c > 0 && c <= 1 && PATH().finders.IsGoodTileToStandOn(x1 + di.x(), y1 + di.y(), a))
                    {
                        return AI.STATES().WALK2.DirTile(a, d, di);
                    }
                    else if (RND.RBoolean())
                    {
                        return AI.STATES().STAND.Activate(a, d, 1.0f + RND.RFloat(5.0f));
                    }
                case 2:
                    d.subByte = 100;
                    return AI.STATES().STOP.Activate(a, d);
                default:
                    return null;
            }
        }
    }
}