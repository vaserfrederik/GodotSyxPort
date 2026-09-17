using System;
using System.Collections.Generic;
using System.IO;
using game.faction;
using game.faction.diplomacy;
using init.sprite.UI;
using settlement.entry;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.sets;
using util.gui.misc;
using util.text;
using view.main;
using view.ui.message;
using world;
using world.entity;
using world.entity.army;
using world.map.regions;

namespace settlement.entry
{
    internal class EntryUpdater : SAVABLE
    {
        private static readonly CharSequence ¤¤open = "Capital Open!";
        private static readonly CharSequence ¤¤openD = "Your capital is now open for immigrants and trade.";
        private static readonly CharSequence ¤¤closed = "Capital Closed!";
        private static readonly CharSequence ¤¤closedD = "Your throne can not be reached from the outside world. As a consequence, no immigration or trade can occur. Clear a path from the throne to a country road as soon as possible.";

        private static readonly CharSequence ¤¤mTitle = "City Isolated!";
        private static readonly CharSequence ¤¤mDesc = "One or several of our city's entry points have been blocked off, and as a result, outsiders will have a problem reaching us. This will have many negative consequences and should be fixed as quickly as possible.";

        static EntryUpdater()
        {
            D.ts(typeof(EntryUpdater));
        }

        private double checkTimer = 0;
        private bool isClosed = false;
        private bool besieged;
        private double besigeTime = 0;

        public void Update(double ds, EntryPoints points)
        {
            if (VIEW.b().IsActive())
                return;

            if (FACTIONS.player().capitolRegion() == null)
                return;

            int oldReach = points.reachable().size();

            points.update();

            if (oldReach > points.reachable().size())
            {
                new Mess(points);
            }

            if (!SETT.INVADOR().invading())
            {
                checkTimer += ds;
                if (checkTimer > 5)
                {
                    checkTimer -= 5;
                    Region c = FACTIONS.player().capitolRegion();

                    besieged = false;

                    if (SETT.INVADOR().invading())
                    {
                        besieged = true;
                    }
                    else
                    {
                        foreach (WEntity e in WORLD.ENTITIES().fillTiles(c.cx() - 3, c.cx() + 3, c.cy() - 3, c.cy() + 3))
                        {
                            if (e is WArmy)
                            {
                                WArmy a = (WArmy)e;
                                if (DIP.WAR().is(a.faction(), FACTIONS.player()) && a.besieging(FACTIONS.player().capitolRegion()))
                                {
                                    besieged = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (besieged)
                        isClosed = true;
                    else if (isClosed && points.hasAny())
                    {
                        isClosed = false;
                        if (!VIEW.b().IsActive())
                            new MessageText(¤¤open, ¤¤openD).send();
                    }
                    else if (!isClosed && !points.hasAny())
                    {
                        if (!VIEW.b().IsActive())
                            new MessageText(¤¤closed, ¤¤closedD).send();
                        isClosed = true;
                    }
                }
            }

            if (besieged)
                besigeTime += ds;
            else
                besigeTime = 0;
        }

        public void Save(FilePutter file)
        {
            file.bool(isClosed);
            file.d(checkTimer);
            file.bool(besieged);
            file.d(besigeTime);
        }

        public void Load(FileGetter file)
        {
            isClosed = file.bool();
            checkTimer = file.d();
            besieged = file.bool();
            besigeTime = file.d();
        }

        public void Clear()
        {
            isClosed = false;
            checkTimer = 0;
            besieged = false;
            besigeTime = 0;
        }

        public bool IsClosed()
        {
            return isClosed;
        }

        public bool Beseiged()
        {
            return besieged;
        }

        public double BesigeTime()
        {
            return besigeTime;
        }

        private class Mess : MessageSection
        {
            private ArrayCooShort coos;

            public Mess(EntryPoints po) : base(¤¤mTitle)
            {
                coos = new ArrayCooShort(po.active().size() - po.reachable().size());
                foreach (EntryPoint p in po.active())
                {
                    if (!p.reachable())
                    {
                        coos.get().set(p.coo());
                        coos.inc();
                    }
                }
            }

            protected override void make(GuiSection section)
            {
                paragraph(¤¤mDesc);

                coos.set(0);

                while (true)
                {
                    section.addRelBody(8, DIR.S, new GButt.ButtPanel(UI.icons().m.crossair)
                    {
                        private readonly int tx = coos.get().x();
                        private readonly int ty = coos.get().y();

                        protected override void clickA()
                        {
                            VIEW.s().activate();
                            VIEW.s().getWindow().centerAtTile(tx, ty);
                        }
                    }.pad(20, 2));

                    if (!coos.hasNext())
                        return;
                    coos.next();
                }
            }
        }
    }
}