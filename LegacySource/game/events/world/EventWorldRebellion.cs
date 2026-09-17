using System;
using System.IO;
using System.Collections.Generic;
using game;
using game.events.EVENTS;
using game.faction;
using game.time;
using init.constant;
using init.race;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using view.ui.message;
using view.world.panel;
using world;
using world.army;
using world.entity.army;
using world.map.regions;
using world.region;
using world.region.pop;

namespace game.events.world
{
    public class EventWorldRebellion : EventResource
    {
        private static readonly CharSequence ¤¤Rebellion = "¤Rebellion!";
        private static readonly CharSequence ¤¤Warning = "¤Rebellion Imminent!";
        private static readonly CharSequence ¤¤WarningD = "¤The region of {0} has very low public order, and could rebel any day now. A temporary fix would be to station troops there, deterring the troublemakers, but in the long run, we should look into fixing these problems permanently.";
        private static readonly CharSequence ¤¤RebellionD = "¤The region of {0} has had enough of your mistreatment and have declared independency from your tyrannical rule.";
        private static readonly CharSequence ¤¤RebellionArmy = "¤In fact they hate you so much that they have raised an army against you. You better deal with this problem before it spreads.";

        private double timer = 0;
        private int ri = 0;
        private readonly Bitmap1D warning;

        static EventWorldRebellion()
        {
            D.ts(typeof(EventWorldRebellion));
        }

        public EventWorldRebellion() : base("REBELLION")
        {
            warning = new Bitmap1D(WREGIONS.MAX, false);

            IDebugPanelWorld.Add("Rebellion", new ACTION
            {
                exe = () =>
                {
                    int ri = RND.rInt(FACTIONS.player().realm().all().Count);
                    if (ri >= 1)
                    {
                        Rebel(FACTIONS.player().realm().region(ri));
                    }
                }
            });

            new RD.RDOwnerChanger
            {
                change = (reg, oldOwner, newOwner) =>
                {
                    if (newOwner == FACTIONS.player())
                    {
                        timer = 0;
                        warning.Set(reg.index(), false);
                    }
                }
            };
        }

        protected override void Save(FilePutter file)
        {
            file.PutDouble(timer);
            file.PutInt(ri);
            warning.Save(file);
        }

        protected override void Load(FileGetter file) throws IOException
        {
            timer = file.GetDouble();
            ri = file.GetInt();
            warning.Load(file);
        }

        protected override void Clear()
        {
            ri = 0;
            timer = 0;
            warning.SetAll(false);
        }

        protected override void Update(double ds)
        {
            if (FACTIONS.player().realm().regions() == 0)
                return;

            int t = (int)timer;
            timer += ds;

            if (t != (int)timer)
            {
            }

            if (timer > TIME.secondsPerDay())
            {
                timer -= TIME.secondsPerDay();
                ri++;
                if (ri >= FACTIONS.player().realm().regions())
                    ri = 0;

                Region reg = FACTIONS.player().realm().region(ri);
                if (reg.capitol())
                    return;

                if (RD.RACES().loyaltyAll.GetD(reg) <= 0)
                {
                    if (!warning.Get(ri))
                    {
                        warning.Set(ri, true);
                        new MessageText(¤¤Warning).Paragraph(GText.TMP.Clear().Add(¤¤WarningD).Insert(0, reg.info.name())).Send();
                    }
                    else
                    {
                        Rebel(reg);
                    }
                }
                else if (RD.RACES().loyaltyAll.GetD(reg) > 0.5)
                {
                    warning.Set(ri, false);
                }
            }
        }

        public void Rebel(Region reg)
        {
            if (reg.capitol())
                return;

            RD.SetFaction(reg, null, true);

            int men = Men(reg);
            WArmy a = null;
            if (WORLD.ENTITIES().armies.CanCreate() && men > 10 && RND.rBoolean())
            {
                a = Army(reg, men);
            }

            MessageText m = new MessageText(¤¤Rebellion);
            m.Paragraph(Str.TMP.Clear().Add(¤¤RebellionD).Insert(0, reg.info.name()));

            if (a != null)
                m.Paragraph(¤¤RebellionArmy);

            m.Send();
        }

        public int Men(Region reg)
        {
            int men = RD.MILITARY().garrison.Get(reg);

            foreach (WArmy a2 in FACTIONS.player().armies().All())
            {
                if (a2.region() == reg)
                {
                    men += AD.men(null).Get(a2);
                }
                else if (a2.region() != null && a2.region().faction() == FACTIONS.player())
                    men += 0.25 * AD.men(null).Get(a2);
                else
                    men += 0.125 * AD.men(null).Get(a2);
            }

            men *= 1.0 + RND.rExpo();

            if (men < 30)
                men = 30 + RND.rInt(20);

            men = CLAMP.i(men, 0, Config.battle().MEN_PER_ARMY);

            return men;
        }

        public WArmy Army(Region reg, int men)
        {
            COORDINATE c = WORLD.PATH().Rnd(reg);

            WArmy a = WORLD.ENTITIES().armies.Create(c.x(), c.y(), null);
            if (a == null)
            {
                GAME.Notify(c.x() + " " + c.y());
                return null;
            }

            double raceTot = 0;
            Race biggest = RACES.all().Get(0);
            int b = 0;

            foreach (RDRace r in RD.RACES().all)
            {
                raceTot += r.pop.Get(reg);
                if (r.pop.Get(reg) > b)
                {
                    biggest = r.race;
                }
            }

            double menLeft = 0;

            foreach (RDRace r in RD.RACES().all)
            {
                menLeft += men * r.pop.Get(reg) / raceTot;

                while (menLeft > 2 && a.divs().CanAdd())
                {
                    int am = CLAMP.i((int)menLeft, 0, Config.battle().MEN_PER_DIVISION);

                    WDivRegional d = AD.regional().Create(r.race, (double)am / Config.battle().MEN_PER_DIVISION, a);
                    d.Randomize(RND.rExpo(), RND.rExpo());
                    d.MenSet(d.MenTarget());
                    menLeft -= am;
                }
            }

            a.name.Clear().Add(biggest.info.armyNames.Rnd());

            foreach (ADSupply s in AD.supplies().All)
            {
                s.current().Set(a, s.targetAmount(a));
            }

            return a;
        }
    }
}