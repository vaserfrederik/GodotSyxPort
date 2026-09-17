using System;
using System.Collections.Generic;
using game;
using game.faction;
using game.faction.npc;
using game.faction.npc.stockpile;
using game.faction.player.emmi;
using game.faction.royalty;
using game.time;
using init.sprite.UI;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.text;
using view.ui.message;
using view.world.ui.faction;

namespace game.faction.royalty.opinion
{
    public static class OpsEmi
    {
        private static readonly string ¤¤flattery = "Flattery";
        private static readonly string ¤¤dflatteryD = "Flattery From your Emissaries.";

        private static readonly string ¤¤sabotage = "Sabotage";
        private static readonly string ¤¤sabotageD = "Sabotage from your Emissaries.";

        private static readonly string ¤¤assasination = "Assassinations";
        private static readonly string ¤¤assasinationD = "Assassinations of court members.";

        private static readonly string ¤¤assasinated = "Assassinated!";
        private static readonly string ¤¤assasinatedSucc = "Our emissaries report, their mission is done. The great lord of {FACTION} slipped last night on their nightdress, leading to a fall down the stone stairs of {HIS} bed chamber. Once down, a chandelier happened to fall on top of {NAME}'s head, crushing the skull completely. What a tragedy!";

        private static readonly string ¤¤assasinatedFail = "Busted!";
        private static readonly string ¤¤assasinatedFailD = "One of our emissaries serving in the court of {FACTION} was arrested and tortured. Unfortunately, our plans have been compromised. {NAME} knows this, and is not too happy about it. Our 'attempts' will continue, but it will be harder now.";

        static OpsEmi()
        {
            D.ts(typeof(OpsEmi));
        }

        private readonly ROpper good;
        private readonly ROpper bad;
        private readonly ROpper assas;

        public OpsEmi()
        {
            double year = 16 * TIME.secondsPerDay();

            good = new ROpper("EMMI_GOOD", ¤¤flattery, ¤¤dflatteryD, UI.icons().s.gift, 80, false)
            {
                Increase = (roy) =>
                {
                    double v = value.getD(roy);

                    double target = ptarget(roy);
                    if (target > v)
                    {
                        return 1.0 / (year * 2);
                    }
                    else if (target < v)
                    {
                        return -1.0 / (year * 0.5);
                    }
                    return 0;
                },

                Ptarget = (bo) => vv(bo, FACTIONS.player().emissaries.flatter, this, FACTIONS.player().emissaries.penaltyMul())
            };

            bad = new ROpper("EMMI_BAD", ¤¤sabotage, ¤¤sabotageD, UI.icons().s.gift, -160, false)
            {
                Increase = (roy) =>
                {
                    double v = value.getD(roy);

                    double target = ptarget(roy);
                    if (target > v)
                    {
                        return 1.0 / (year * 2);
                    }
                    else if (target < v)
                    {
                        return -1.0 / (year * 0.5);
                    }
                    return 0;
                },

                Ptarget = (bo) => vv(bo, FACTIONS.player().emissaries.sabotage, this, FACTIONS.player().emissaries.penaltyMul())
            };

            assas = new ROpper.ROpperDown("EMMI_ASSES", ¤¤assasination, ¤¤assasinationD, UI.icons().s.death, -10, false, year * 4)
            {
                Update = (roy, time) =>
                {
                    double t = assasinationsPerYear(roy, FACTIONS.player().emissaries.penaltyMul());
                    t = time * t / year;
                    int a = (int)state.getD(roy);
                    state.incD(roy, t);
                    int n = (int)state.getD(roy);
                    if (a != n)
                    {
                        state.incD(roy, -n);
                        long ran = STATS.RAN().getL(roy.induvidual, a % 32);
                        if ((ran & 0b11) == 0)
                        {
                            assasinate(roy, true);
                            GAME.count().ROYALTIES_KILLED.inc(1);
                        }
                        else
                        {
                            assasinate(roy, false);
                        }
                    }
                    base.Update(roy, time);
                }
            };
        }

        private double vv(Royalty roy, EmiTypeRoy em, ROpper op, double eff)
        {
            return em.get(roy) * valuePerEmissary(roy.court.faction) * eff / Math.Abs(op.to());
        }

        private double valuePerEmissary(FactionNPC f)
        {
            return 160.0 * 12.0 * NPCStockpile.AVERAGE_PRICE / FACTIONS.WORTH().faction(f);
        }

        public void assasinate(Royalty roy, bool kill)
        {
            assas.value.incD(roy, 0.25);
            if (kill)
            {
                roy.kill(false);
                new Mess(¤¤assasinated, ¤¤assasinatedSucc, roy).send();
            }
            else
            {
                new Mess(¤¤assasinatedFail, ¤¤assasinatedFailD, roy).send();
            }
        }

        public double assasinationsPerYear(Royalty roy, double efficiency)
        {
            double t = FACTIONS.player().emissaries.assasinate.get(roy) * valuePerEmissary(roy.court.faction) * FACTIONS.player().emissaries.penaltyMul();
            t /= 1 + assas.value.getD(roy) * 4.0;
            return t;
        }

        public double opinionTarget(Royalty roy, double efficiency)
        {
            double oldg = good.value.getD(roy);
            double oldb = bad.value.getD(roy);

            good.value.setD(roy, vv(roy, FACTIONS.player().emissaries.flatter, good, efficiency));
            bad.value.setD(roy, vv(roy, FACTIONS.player().emissaries.sabotage, bad, efficiency));
            double res = ROPINION.get(roy);
            good.value.setD(roy, oldg);
            bad.value.setD(roy, oldb);
            return res;
        }

        public double trustTarget(Royalty roy, double efficiency)
        {
            double oldg = good.value.getD(roy);
            double oldb = bad.value.getD(roy);

            good.value.setD(roy, vv(roy, FACTIONS.player().emissaries.flatter, good, efficiency));
            bad.value.setD(roy, vv(roy, FACTIONS.player().emissaries.sabotage, bad, efficiency));
            double res = ROPINION.trust().get(roy.court.faction);
            good.value.setD(roy, oldg);
            bad.value.setD(roy, oldb);
            return res;
        }

        private class Mess : MessageSection
        {
            private readonly string desc;
            private readonly Induvidual indu;
            private readonly string name;
            private readonly string fName;
            private int sI;

            public Mess(CharSequence title, CharSequence desc, Royalty roy) : base(title)
            {
                this.desc = "" + desc;
                this.name = "" + roy.name();
                this.indu = roy.induvidual;
                sI = roy.successionI();
                fName = "" + roy.court.faction.name;
            }

            protected override void Make(GuiSection section)
            {
                paragraph(Str.TMP.clear().add(desc).insert("NAME", name).insert("FACTION", fName).insert("HIS", indu.race().info.pHIS.get(indu, false)));
                section.addRelBody(8, DIR.N, new UIRoyalty.PortraitAbs(4)
                {
                    Succ = () => sI,
                    Indu = () => indu
                });
            }
        }
    }
}