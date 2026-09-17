using System;
using System.IO;
using game;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.faction.player;
using init.sprite.UI;
using init.type;
using settlement.main;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.util;
using settlement.room.water;
using settlement.stats;
using snake2d.util.color;
using snake2d.util.map;
using util.text;

namespace settlement.room.water
{
    public class ROOM_WATER
    {
        private static readonly string ¤¤irrigation = "Water Supply";
        static
        {
            D.ts(typeof(ROOM_WATER));
        }

        public readonly ROOM_PUMP pump;
        public readonly Canal canal;
        public readonly Drain drain;
        private readonly WSprite sprite;
        private readonly Updater updater = new Updater(this);

        public ROOM_WATER(RoomInitData init, RoomCategorySub cat) : base(init, cat)
        {
            pump = new ROOM_PUMP(init, cat);
            canal = new Canal(init, cat);
            drain = new Drain(init, cat);
            sprite = new WSprite(this, init);
        }

        private readonly MAP_OBJECT<RoomPumpable> pumpable = new MAP_OBJECT<RoomPumpable>()
        {
            public RoomPumpable get(int tile)
            {
                return get(tile % SETT.TWIDTH, tile / SETT.TWIDTH);
            }

            public RoomPumpable get(int tx, int ty)
            {
                RoomBlueprint p = SETT.ROOMS().map.blueprint.get(tx, ty);
                if (p != null && p is ROOM_PUMPABLE)
                    return ((ROOM_PUMPABLE)p).pumpable(tx, ty);
                return null;
            }
        };

        public static void pushBonus(ROOM_PUMPABLE blue, Boostable bo, double from, double to)
        {
            BSourceInfo in = new BSourceInfo(¤¤irrigation, UI.icons().s.drop.createColored(COLOR.BLUEISH));
            Booster bos = new BoosterImp(in, from, to, false)
            {
                public double vGet(Induvidual indu)
                {
                    RoomInstance ins = STATS.WORK().EMPLOYED.get(indu);
                    if (ins != null && ins.blueprint() == blue)
                    {
                        return blue.pumpable(ins.mX(), ins.mY()).irrigation(ins.mX(), ins.mY());
                    }
                    return 0;
                }

                private int ci = -120;
                private double c = 0;

                public double vGet(Player f)
                {
                    return vGet(HCLASS_RACE.clP());
                }

                public double vGet(HCLASS_RACE popTime)
                {
                    if (blue is RoomBlueprintIns<?>)
                    {
                        RoomBlueprintIns<?> p = (RoomBlueprintIns<?>)blue;
                        if (Math.Abs(GAME.updateI() - ci) >= 120)
                        {
                            ci = GAME.updateI();
                            c = 0;
                            int am = 0;
                            for (int i = 0; i < p.instancesSize(); i++)
                            {
                                RoomInstance ins = p.getInstance(i);
                                int e = ins.employees().employed();
                                c += e * blue.pumpable(ins.mX(), ins.mY()).irrigation(ins.mX(), ins.mY());
                                am += e;
                            }

                            if (am != 0)
                            {
                                c /= am;
                            }
                        }
                    }

                    return c;
                }

                public double vGet(FactionNPC f)
                {
                    return 0;
                }

                public double get(BOOSTABLE_O o)
                {
                    if (o is FactionNPC)
                        return 1.0;
                    return base.get(o);
                }

                public double vGet(Faction f)
                {
                    return 0;
                }
            };
            bos.add(bo);
        }
    }
}