using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.royalty.opinion;
using game.time;
using init.constant;
using init.resources;
using init.trade;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.colors;
using util.gui.misc;
using util.text;
using view.main;
using world;
using world.army;
using world.entity.caravan;
using world.map.regions;
using world.region;
using world.region.pop;

namespace world.entity.army
{
    public abstract class WArmyState : INDEXED
    {
        private static LIST<WArmyState> all = new ArrayList<>(0);

        private static CharSequence ¤¤siege = "Are you sure you wish to besiege {0} and declare war on the faction of {0}?";
        
        static
        {
            D.ts(typeof(WArmyState));
        }

        public static LIST<WArmyState> all()
        {
            return all;
        }

        public final static WArmyState fortified = new WArmyState()
        {
            override WArmyState update(WArmy a, double ds)
            {
                checkTile(a);
                if (a.faction() == null && a.region().faction() == FACTIONS.player())
                {
                    return raiding;
                }
                return this;
            }

            override public GText info(WArmy a, GText box)
            {
                box.normalify();
                box.set(Dic.¤¤Fortified);
                return box;
            }

            override public CharSequence name(WArmy a)
            {
                return Dic.¤¤Fortified;
            }
        };

        public final static WArmyState fortifying = new WArmyState()
        {
            override WArmyState update(WArmy a, double ds)
            {
                checkTile(a);
                if (a.faction() == null && a.region().faction() == FACTIONS.player())
                {
                    return raiding;
                }
                a.stateFloat += ds;
                if (a.stateFloat > TIME.secondsPerDay() / 2)
                    return fortified;
                return this;
            }

            override public GText info(WArmy a, GText box)
            {
                box.normalify();
                box.set(Dic.¤¤Fortifying);
                return box;
            }

            override public CharSequence name(WArmy a)
            {
                return Dic.¤¤Fortifying;
            }
        };

        private static void checkTile(WArmy a)
        {
            if (!WORLD.PATH().map.is.is(a.ctx(), a.cty()))
            {
                for (int i = 0; i < GUTIL.circle().length(); i++)
                {
                    COORDINATE c = GUTIL.circle().get(i);
                    int x = a.body().cX() + c.x();
                    int y = a.body().cY() + c.y();
                    int tx = x >> C.T_SCROLL;
                    int ty = y >> C.T_SCROLL;
                    if (WORLD.PATH().map.is.is(tx, ty))
                    {
                        a.teleport(tx, ty);
                        return;
                    }
                }
            }
        }

        public final static WArmyState raiding = new WArmyState()
        {
            override WArmyState update(WArmy a, double ds)
            {
                Region reg = a.region();
                if (reg == null)
                {
                    a.stateFloat = 0;
                    return WArmyState.fortifying;
                }

                a.stateFloat += ds;

                if (a.stateFloat < 120)
                    return this;

                a.stateFloat -= 120;

                double rd = RD.RACES().popSize(reg);
                double ad = (double)AD.men(null).get(a) / (Config.battle().MEN_PER_ARMY);
                double dd = ad / rd;

                double d = 180 * dd / (TIME.secondsPerDay() * 4);

                double inc = RD.DEVASTATION().current.max(reg) * d;
                int iinc = (int)inc;
                if (inc - iinc > RND.rnd())
                    iinc++;

                if (iinc > 0)
                {
                    foreach (var resource in RESOURCES.all())
                    {
                        int amount = (int)(resource.amountInRegion(reg) * d);
                        resource.removeFromRegion(reg, amount);
                    }
                }

                if (iinc > 0)
                {
                    foreach (var resource in RESOURCES.all())
                    {
                        int amount = (int)(resource.amountInRegion(reg) * d);
                        resource.removeFromRegion(reg, amount);
                    }
                }

                if (iinc > 0)
                {
                    foreach (var resource in RESOURCES.all())
                    {
                        int amount = (int)(resource.amountInRegion(reg) * d);
                        resource.removeFromRegion(reg, amount);
                    }
                }

                if (iinc > 0)
                {
                    foreach (var resource in RESOURCES.all())
                    {
                        int amount = (int)(resource.amountInRegion(reg) * d);
                        resource.removeFromRegion(reg, amount);
                    }
                }

                return this;
            }

            override public GText info(WArmy a, GText box)
            {
                box.normalify();
                box.add(name(a));
                return box;
            }

            override public CharSequence name(WArmy a)
            {
                return Dic.¤¤Raiding;
            }
        };

        public final static WArmyState moving = new WArmyState()
        {
            override WArmyState update(WArmy a, double ds)
            {
                if (!a.path().move(a, WArmy.speed * ds))
                {
                    a.stateFloat = 0;
                    return fortifying;
                }
                return this;
            }

            override public GText info(WArmy a, GText box)
            {
                Region reg = WORLD.REGIONS().map.get(a.path().destX(), a.path().destY());
                if (reg == null)
                {
                    box.normalify();
                    box.add(name(a));
                }
                else
                {
                    GText text = box;
                    text.color(GCOLOR.MAP().get(reg.faction()));
                    text.add(Dic.¤¤MarchingTo).insert(0, reg.info.name());
                }
                return box;
            }

            override public CharSequence name(WArmy a)
            {
                return Dic.¤¤Moving;
            }
        };

        public final static WArmyState intercepting = new WArmyState()
        {
            override WArmyState update(WArmy a, double ds)
            {
                WArmy aa = intercepting(a);
                if (aa == null)
                {
                    a.stateFloat = 0;
                    return fortifying;
                }

                if (!a.path().move(a, WArmy.speed * ds))
                {
                    a.stateFloat = 0;
                    return fortifying;
                }
                return this;
            }

            override public GText info(WArmy a, GText box)
            {
                WArmy aa = intercepting(a);
                if (aa == null)
                {
                    box.normalify();
                    box.add(name(a));
                }
                else
                {
                    GText text = box;
                    text.color(GCOLOR.MAP().get(aa.faction()));
                    text.add(Dic.¤¤Intercepting).insert(0, aa.name);
                }
                return box;
            }

            override public CharSequence name(WArmy a)
            {
                return Dic.¤¤Intercepting;
            }

            private WArmy intercepting(WArmy a)
            {
                if (a.stateShort != -1)
                {
                    WArmy aa = WORLD.ENTITIES().armies.get(a.stateShort);
                    if (aa == null || !aa.added())
                    {
                        a.stateShort = -1;
                        return null;
                    }
                    return aa;
                }
                return null;
            }
        };

        public final static WArmyState besieging = new WArmyState()
        {
            Region aReg;
            WArmy aa;
            private ACTION besiege = new ACTION()
            {
                override public void exe()
                {
                    if (aReg.faction() != null && aReg.faction() is FactionNPC)
                    {
                        ROPINION.STANCE().setNewStance((FactionNPC)aReg.faction(), DIP.WAR(), aa.faction() == FACTIONS.player());
                        aa.besiege(aReg);
                    }
                }
            };

            override WArmyState update(WArmy a, double ds)
            {
                Region reg = WORLD.REGIONS().getByIndex(a.stateShort);

                if (a.faction() == reg.faction() || !a.path().isValid())
                {
                    if (!a.besieging(reg))
                    {
                        a.path().clear();
                        a.stateFloat = 0;
                        return fortifying;
                    }
                    return this;
                }

                if (a.path().move(a, WArmy.speed * ds))
                {
                    return this;
                }
                else
                {
                    a.path().clear();
                    if (a.faction() == FACTIONS.player() && reg.faction() != a.faction() && !DIP.WAR().is(reg.faction(), a.faction()) && reg.faction() != null)
                    {
                        aReg = reg;
                        aa = a;
                        VIEW.inters().yesNo.activate(Str.TMP.clear().add(¤¤siege).insert(0, reg.info.name()).insert(0, reg.faction().name), besiege, ACTION.NOP, true);
                        return this;
                    }
                    else
                    {
                        WORLD.BATTLES().besige(a, reg);
                    }
                }

                return this;
            }

            override public GText info(WArmy a, GText box)
            {
                Region reg = WORLD.REGIONS().getByIndex(a.stateShort);
                if (reg == null)
                {
                    box.normalify();
                    box.add(name(a));
                }
                else
                {
                    GText text = box;
                    text.color(GCOLOR.MAP().get(reg.faction()));
                    text.add(Dic.¤¤BesiegingSomething).insert(0, reg.info.name());
                }
                return box;
            }

            override public CharSequence name(WArmy a)
            {
                return Dic.¤¤Besieging;
            }
        };

        public final static WArmyState movingRaid = new WArmyState()
        {
            override WArmyState update(WArmy a, double ds)
            {
                if (!a.path().move(a, WArmy.speed * ds))
                {
                    a.stateFloat = 0;
                    if (a.canRaid())
                        return raiding;
                    return fortifying;
                }
                return this;
            }

            override public GText info(WArmy a, GText box)
            {
                Region reg = WORLD.REGIONS().map.get(a.path().destX(), a.path().destY());
                if (reg == null)
                {
                    box.normalify();
                    box.add(name(a));
                }
                else
                {
                    GText text = box;
                    text.color(GCOLOR.MAP().get(reg.faction()));
                    text.add(Dic.¤¤MarchingTo).insert(0, reg.info.name());
                }
                return box;
            }

            override public CharSequence name(WArmy a)
            {
                return Dic.¤¤Moving;
            }
        };

        public static bool canBesiege(WArmy a, Region reg)
        {
            return (reg != null && a.faction() != reg.faction());
        }

        private readonly int index;

        private WArmyState()
        {
            all = all.join(this);
            index = all.size() - 1;
        }

        abstract WArmyState update(WArmy a, double ds);

        public abstract GText info(WArmy a, GText text);
        public abstract CharSequence name(WArmy a);

        public override int index()
        {
            return index;
        }
    }
}