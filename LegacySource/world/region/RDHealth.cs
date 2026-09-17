using System;
using System.Collections.Generic;
using System.Linq;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.time;
using init.sprite.UI;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.info;
using util.text;
using view.ui.message;
using world.map.regions;
using world.region.RD;
using world.region.RDOutputs;
using world.region.RData;

namespace world.region
{
    public class RDHealth : RDataE
    {
        public static CharSequence ¤¤name = "¤Health";
        public static CharSequence ¤¤desc = "¤Health must be maintained at over 50% in a region, else there is a risk of disease.";

        private static CharSequence ¤¤pos = "¤You have health moving towards more than 50% and need not fear an outbreak of disease.";
        private static CharSequence ¤¤neg = "¤You have health plummeting  below 50%, and an outbreak of disease is to be expected.";
        private static CharSequence ¤¤out = "¤This region is currently suffering from an outbreak of disease. You must increase health quickly.";
        private static CharSequence ¤¤targetE = "¤At pop Target";

        static CharSequence ¤¤epidemic = "¤Outbreak";
        private static CharSequence ¤¤epidemicD = "¤The region of {0} has suffered from low health and as a result there has been an outbreak of disease. While the epidemic is lasting, the region will suffer big penalties across the board. You must increase the health to over 50% in order to save the settlement.";

        static
        {
            D.ts(typeof(RDHealth));
        }

        public readonly Boostable boostablee;
        public readonly RDataE outbreak;
        private bool btoggle = true;

        private static double dTime = 1.0 / (TIME.secondsPerDay() * 2);

        private CharSequence eDesc(Region reg)
        {
            Str.TMP.Clear().Add(¤¤epidemicD).Insert(0, reg.info.name());
            return Str.TMP;
        }

        public void hover(GBox b, Region reg)
        {
            b.title(RD.HEALTH().boostablee.name);
            b.text(RD.HEALTH().boostablee.desc);
            b.NL(4);

            if (outbreak.isMax(reg))
            {
                b.error(b.text().Clear().Add(¤¤out).Insert(0, reg.info.name()));
            }
            else if (boostablee.get(reg) < 0.5)
            {
                b.add(b.text().warnify().Add(¤¤neg));
            }
            else
            {
                b.add(b.text().normalify2().Add(¤¤pos));
            }
            b.NL(4);

            b.textLL(Dic.¤¤Current);
            b.tab(6);
            b.add(GFORMAT.perc(b.text(), getD(reg)));
            b.NL();
            b.textLL(Dic.¤¤Target);
            b.tab(6);
            b.add(GFORMAT.perc(b.text(), CLAMP.d(boostablee.get(reg), 0, 1)));
            b.NL();
            b.textLL(¤¤targetE);
            bool bb = btoggle;
            btoggle = false;
            b.tab(6);
            b.add(GFORMAT.perc(b.text(), CLAMP.d(boostablee.get(reg), 0, 1)));
            b.NL();
            btoggle = bb;

            b.sep();
            boostablee.hover(b, reg, true);
        }

        public void problem(GBox b, Region reg)
        {
            if (outbreak.isMax(reg))
            {
                b.NL();
                b.error(b.text().Clear().Add(¤¤out).Insert(0, reg.info.name()));
                b.NL();
            }
        }

        RDHealth(RDInit init) : base("HEALTH", init.count.new DataByte("HEALTH"), init, ¤¤name)
        {
            boostablee = BOOSTING.push("HEALTH", 1, ¤¤name, ¤¤desc, UI.icons().s.heart, BoostableCat.ALL().WORLD);

            new RBooster(new BSourceInfo(Dic.¤¤Population, UI.icons().s.human), 0, -8, false)
            {
                public override double get(Region t)
                {
                    if (btoggle)
                        return RD.RACES().capacityCurrent.get(t) / RD.RACES().maxPop(t);
                    return RD.RACES().capacity(t) / RD.RACES().maxPop(t);
                }
            }.add(boostablee);

            outbreak = new RDataE("OUTBREAK", init.count.new DataBit("OUTBREAK"), init, ¤¤epidemic);

            BOOSTING.connecter(new ACTION()
            {
                public override void exe()
                {
                    RBooster bo = new RBooster(new BSourceInfo(¤¤epidemic, UI.icons().s.death), 1, 0, true)
                    {
                        protected override double get(Region reg)
                        {
                            return outbreak.get(reg);
                        }
                    };
                    foreach (RDOutput o in RD.OUTPUT().ALL)
                    {
                        bo.add(o.boost);
                        bo.add(o.boostYearlyPart);
                    }
                    bo = new RBooster(new BSourceInfo(¤¤epidemic, UI.icons().s.death), 1, 0, true)
                    {
                        protected override double get(Region reg)
                        {
                            return outbreak.get(reg);
                        }
                    };
                    bo.add(RD.RACES().capacity);
                }
            });

            init.upers.add(new RDUpdatable()
            {
                public void update(Region reg, double time)
                {
                    if (reg.faction() == null || reg.faction() is FactionNPC)
                    {
                        set(reg, 255);
                        return;
                    }

                    double b = boostablee.get(reg);
                    bool bb = btoggle;
                    btoggle = true;
                    b = Math.Max(b, boostablee.get(reg));

                    int target = CLAMP.i((int)(255 * b), 0, 255);
                    btoggle = bb;

                    moveTo(reg, time * dTime, target);

                    if (reg.faction() == FACTIONS.player() && !reg.capitol())
                    {
                        if (get(reg) < 120 && outbreak.get(reg) == 0 && target < 120)
                        {
                            outbreak.set(reg, 1);
                            new MessageText(¤¤epidemic).paragraph(eDesc(reg)).send();
                        }
                        else if (outbreak.get(reg) == 1 && (get(reg) > 128) && target > 128)
                        {
                            outbreak.set(reg, 0);
                        }
                    }
                }

                public void init(Region reg)
                {
                    setD(reg, 1.0);
                }
            });

            new RD.RDOwnerChanger()
            {
                public void change(Region reg, Faction oldOwner, Faction newOwner)
                {
                    if (newOwner == FACTIONS.player())
                        setD(reg, 1.0);
                }
            };
        }
    }
}