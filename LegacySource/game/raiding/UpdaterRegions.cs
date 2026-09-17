using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.faction;
using game.faction.diplomacy;
using game.raiding;
using game.time;
using init.race;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using util.updating;
using view.interrupter;
using view.main;
using view.ui.message;
using world;
using world.entity.army;
using world.map.regions;
using world.region;
using world.region.pop;

namespace game.raiding
{
    internal class UpdaterRegions : IUpdater, SAVABLE
    {
        private static readonly string ¤¤nameRace = "¤{0} Raiders";
        private static readonly string ¤¤nameRegion = "¤Raiders of {0}";

        private static readonly string ¤¤title = "Raiders!";
        private static readonly string ¤¤desc = "Mi lord, bandits have been spotted raiding our border settlements!";

        static UpdaterRegions()
        {
            D.ts(typeof(UpdaterRegions));
        }

        private readonly double[] counts = new double[WREGIONS.MAX];
        private readonly double II = 1.0 / (TIME.secondsPerDay() * 4.0);

        public UpdaterRegions() : base(WREGIONS.MAX, TIME.secondsPerDay())
        {
            IDebugPanel.Add("Raider region spawn", new ACTION
            {
                exe = () =>
                {
                    if (GAME.raiders().entry.entryRegions().size() > 0)
                    {
                        raid(GAME.raiders().entry.entryRegions().rnd().r());
                    }
                }
            });
        }

        public void save(FilePutter file)
        {
            file.dsE(counts);
            base.save(file);
        }

        public void load(FileGetter file)
        {
            file.dsE(counts);
            base.load(file);
        }

        public void clear()
        {
            Array.Fill(counts, 0);
            base.clear();
        }

        protected override void update(int i, double timeSinceLast)
        {
            RaidRegion r = GAME.raiders().entry.get(WORLD.REGIONS().all().get(i));
            double d = counts[i];
            if (r.r().faction() != FACTIONS.player() || r.r().capitol())
            {
                d -= timeSinceLast * II;
            }
            else
            {
                double c = 1 - r.security();

                d += timeSinceLast * II * CLAMP.d(c, -1, 1);
                if (d >= 1)
                {
                    raid(r.r());
                    d -= (int)d;
                }
            }
            d = CLAMP.d(d, 0, 1);
            counts[i] = d;
        }

        private readonly ArrayList<RaidEntryPoint> tmp = new ArrayList<RaidEntryPoint>(16);

        private void raid(Region reg)
        {
            if (reg.besieged())
                return;
            foreach (WArmy a in WORLD.ENTITIES().armies.fill(reg))
            {
                if (a.faction() == null || (a.faction() != FACTIONS.player() && DIP.get(a.faction(), FACTIONS.player()) == DIP.WAR()))
                    return;
            }

            tmp.clearSloppy();
            foreach (RaidEntryPoint c in GAME.raiders().entry.entrySpots())
            {
                if (reg.is(c.c()) && tmp.hasRoom())
                {
                    tmp.add(c);
                }
            }

            if (tmp.size() <= 0)
                return;

            RaidEntryPoint e = tmp.rnd();

            double power = RD.MILITARY().power.getD(reg) + 20;
            power += GAME.raiders().entry.get(reg).army();
            power *= 1.25 + RND.rExpo() * 2.0;
            Race race = race(e);

            RaiderArmy a = new RaiderArmy(race, power, 0.2 + RND.rFloat() * 0.5);
            Str.TMP.clear();
            if (e.from() != null)
            {
                Str.TMP.add(¤¤nameRegion).insert(0, e.from().info.name());
            }
            else
            {
                Str.TMP.add(¤¤nameRace).insert(0, race.info.namePosessives);
            }
            a.spawn(e.c().x(), e.c().y(), Str.TMP);
            new M(e.c().x(), e.c().y()).send();
        }

        private Race race(RaidEntryPoint e)
        {
            if (e.from() != null)
            {
                double tot = 0;
                foreach (RDRace r in RD.RACES().all)
                {
                    tot += r.pop.get(e.from()) * r.race.physics.raiding;
                }

                tot *= RND.rFloat();
                foreach (RDRace r in RD.RACES().all)
                {
                    tot -= r.pop.get(e.from()) * r.race.physics.raiding;
                    if (tot <= 0)
                        return r.race;
                }
            }

            double tot = 0;
            foreach (RDRace r in RD.RACES().all)
            {
                tot += r.race.physics.raiding;
            }

            tot *= RND.rFloat();
            foreach (RDRace r in RD.RACES().all)
            {
                tot -= r.race.physics.raiding;
                if (tot <= 0)
                    return r.race;
            }

            return RD.RACES().all.rnd().race;
        }

        private class M : MessageSection
        {
            private readonly int x, y;

            public M(int x, int y) : base(¤¤title)
            {
                this.x = x;
                this.y = y;
            }

            protected override void make(GuiSection section)
            {
                paragraph(¤¤desc);

                GButt b = new GButt.ButtPanel(UI.icons().m.crossair)
                {
                    clickA = () =>
                    {
                        VIEW.world().activate();
                        VIEW.world().window.centererTile.set(x, y);
                    }
                };

                section.addRelBody(16, DIR.S, b);
            }
        }
    }
}