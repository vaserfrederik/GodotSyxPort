using System;
using System.Collections.Generic;
using System.IO;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.faction.player;
using game.faction.royalty;
using game.faction.royalty.opinion;
using game.time;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;
using util.updating;
using view.interrupter;
using view.main;
using view.ui.message;
using world;
using world.map.regions;
using world.region;

namespace game.faction.player.emmi
{
    public class Emissaries
    {
        public static CharSequence ¤¤name = "Emissary Points";
        public static CharSequence ¤¤desc = "Emissaries are used to influence foreign courts, or to increase support in regions outside of your realm. Emissaries are trained in an embassy, and can be assigned in the faction panel or the region panel.";
        private static CharSequence ¤¤low = "Emissary points Low";
        private static CharSequence ¤¤lowD = "We do no longer employ as many emissaries as are needed. As a result, all our diplomatic missions will suffer a penalty. We should cancel a few missions so that there is no shortage.";
        private static CharSequence ¤¤support = "Gather Support";
        private static CharSequence ¤¤supportD = "Gathers support in a region, so that it will be more inclined to be ruled by you in the future.";
        private static CharSequence ¤¤assasinate = "Assassinate";
        private static CharSequence ¤¤assasinateD = "Assassinate a the current ruler. Has a small chance of succeeding and failed attempts will decrease the royalty's opinion of you severely.";
        private static CharSequence ¤¤assasinateDex = "Assassination attempts per year: ";
        private static CharSequence ¤¤flatter = "Flatter";
        private static CharSequence ¤¤flatterD = "Flattering a royalty will increase their opinion of you and your faction.";
        private static CharSequence ¤¤sab = "Sabotage";
        private static CharSequence ¤¤sabD = "Sabotage and insult a royalty to decrease their opinion of you.";

        private int mDay = -60;
        static
        {
            D.ts(typeof(Emissaries));
        }

        private double penalty = 1;

        public readonly EmiTypeReg assimilate = new EmiTypeReg(
            UI.icons().s.fist.createColored(new ColorImp(40, 40, 120)),
            ¤¤support, ¤¤supportD
        );

        public readonly EmiTypeRoy assasinate = new EmiTypeRoy(
            UI.icons().s.death.createColored(new ColorImp(120, 20, 20)),
            ¤¤assasinate, ¤¤assasinateD
        )
        {
            public override void hover(Royalty t, GUI_BOX text)
            {
                base.hover(t, text);
                GBox b = (GBox)text;
                b.textLL(¤¤assasinateDex);
                b.add(GFORMAT.f(b.text(), ROPINION.EMMI().assasinationsPerYear(t, 1.0)));
            }
        };

        public readonly EmiTypeRoy flatter = new EmiTypeRoyOp(
            UI.icons().s.heart.createColored(new ColorImp(120, 40, 120)),
            ¤¤flatter, ¤¤flatterD
        );

        public readonly EmiTypeRoy sabotage = new EmiTypeRoyOp(
            UI.icons().s.cog.createColored(new ColorImp(120, 100, 20)),
            ¤¤sab, ¤¤sabD
        );

        public readonly LIST<EmiTypeRoy> roys = new ArrayList<EmiTypeRoy>(assasinate, flatter, sabotage);
        public readonly LIST<EmiTypeReg> regs = new ArrayList<EmiTypeReg>(assimilate);
        public readonly LIST<EmiType<?>> all = new ArrayList<EmiType<?>>(assimilate, assasinate, flatter, sabotage);

        public Emissaries()
        {
            new FactionActivityListener
            {
                public override void remove(FactionNPC ff)
                {
                    foreach (EmiTypeRoy t in roys)
                        t.clear(ff);
                }

                public override void add(FactionNPC f)
                {
                    // TODO Auto-generated method stub
                }
            };

            new RD.RDOwnerChanger
            {
                public override void change(Region reg, Faction oldOwner, Faction newOwner)
                {
                    if (newOwner == FACTIONS.player())
                    {
                        foreach (EmiTypeReg t in regs)
                            t.set(reg, 0);
                    }
                }
            };

            new NPCCourt.RoyaltyEventListener
            {
                public override void change(int successionI, Royalty old, Royalty nn)
                {
                    if (old != null)
                        assasinate.set(old, 0);
                    if (successionI == 0)
                        return;
                    if (successionI == 0 && nn != null)
                    {
                        int o = old == null ? 0 : flatter.get(old);
                        int n = flatter.get(nn);
                        o = Math.Max(o, n);
                        flatter.set(successionI, o);
                        o = old == null ? 0 : sabotage.get(old);
                        n = sabotage.get(nn);
                        o = Math.Max(o, n);
                        sabotage.set(successionI, o);
                    }
                }
            };

            foreach (EmiType<?> t in all)
            {
                double max = -1000000;
                final double maxI = -1.0 / max;
                BValue v = new BValue.BValueFaction(BOOSTABLES.CIVICS().DIPLOMACY)
                {
                    public override double vGet(Player f)
                    {
                        return t.total() * maxI;
                    }

                    public override double vGet(FactionNPC f)
                    {
                        return 0;
                    }
                };

                new BoosterValue(v, new BSourceInfo(t.name, t.icon), 0, -1000000, false).add(BOOSTABLES.CIVICS().DIPLOMACY);
            }

            IDebugPanel.add("diplomacy + 10000", new ACTION
            {
                public override void exe()
                {
                    BValue v = new BValue.BValuePlayerOnly
                    {
                        public override double vGet(Player f)
                        {
                            return 10000;
                        }

                        public override double vGet(FactionNPC f)
                        {
                            return 0;
                        }
                    };

                    new BoosterValue(v, new BSourceInfo("Debug Boost", UI.icons().s.star), 0, 10000, false).add(BOOSTABLES.CIVICS().DIPLOMACY);
                }
            });
        }

        private int viewI = -1;

        public double penaltyMul()
        {
            if (VIEW.RI() != viewI)
            {
                viewI = VIEW.RI();
                double am = BOOSTABLES.CIVICS().DIPLOMACY.get(FACTIONS.player());
                if (am < 0)
                {
                    am = Math.Floor(am);
                    int tot = 0;
                    foreach (EmiType<?> t in all)
                    {
                        tot += t.total();
                    }

                    penalty = -am / tot;
                    penalty = CLAMP.d(penalty, 0, 1);
                    penalty = 1.0 - penalty;
                }
                else
                {
                    penalty = 1;
                }
            }
            return penalty;
        }

        public int available()
        {
            return (int)(BOOSTABLES.CIVICS().DIPLOMACY.get(FACTIONS.player()));
        }

        public int produced()
        {
            return (int)(BOOSTABLES.CIVICS().DIPLOMACY.get(FACTIONS.player())) + spent();
        }

        public int spent()
        {
            int am = 0;
            foreach (EmiType<?> t in all)
                am += t.total();
            return am;
        }

        public int spent(FactionNPC f)
        {
            int am = 0;
            foreach (EmiTypeRoy t in roys)
                am += t.total(f);
            return am;
        }

        public void update(double ds)
        {
            upReg.update(ds);
            upRoy.update(ds);

            if (penaltyMul() < 1 && Math.Abs(TIME.days().bitsSinceStart() - mDay) > 10)
            {
                new MessageText(¤¤low).paragraph(¤¤lowD).send();
                mDay = TIME.days().bitsSinceStart();
            }
        }

        public readonly SAVABLE saver = new SAVABLE
        {
            public void save(FilePutter file)
            {
                file.i(mDay);
                foreach (EmiType<?> t in all)
                    t.save(file);
            }

            public void load(FileGetter file)
            {
                mDay = file.i();
                foreach (EmiType<?> t in all)
                    t.load(file);
            }

            public void clear()
            {
                mDay = -60;
                foreach (EmiType<?> t in all)
                    t.clear();
            }
        };
    }
}