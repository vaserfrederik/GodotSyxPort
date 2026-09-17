using System;
using System.Collections.Generic;
using System.Linq;
using settlement.stats.colls;
using game.GAME;
using game.battle.div;
using game.boosting;
using game.time;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.entity;
using settlement.stats;
using settlement.stats.stat;
using settlement.stats.util;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.gui.misc;
using util.text;
using util.keymap;
using util.updating;
using view.main;

namespace settlement.stats.colls
{
    public class StatsReproduction
    {
        private static readonly string ¤¤limit = "Births Allowed";
        private static readonly string ¤¤fertile = "Fertile Population";
        private static readonly string ¤¤children = "Children";
        private static readonly string ¤¤noProp = "This subject type does dot reproduce naturally.";
        private static readonly string ¤¤estimated = "Estimated births/year";
        private static readonly string ¤¤days = "A {0} infant is dependant on its parent for {1} days. After that, the child takes {2} days to become a productive adult, but this period can be prolonged by schooling.";
        private static readonly string ¤¤forced = "Forced Propagation";

        static StatsReproduction()
        {
            // Initialization if needed
        }

        public STAT<HTYPE, Race> fertile { get; private set; }
        public STAT<HTYPE, Race> forced { get; private set; }
        public RInt limit { get; private set; }

        public StatsReproduction()
        {
            fertile = new STAT<HTYPE, Race>();
            forced = new STAT<HTYPE, Race>();
            limit = new RInt(0, ENT.all().Count);
        }

        public void update()
        {
            // Update logic if needed
        }

        public void hover(GUI_BOX box, HCLASS cl, Race r)
        {
            GBox b = box as GBox;

            HTYPE pp = parent(cl);

            b.textLL(pp.names);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), pp == null ? 0 : STATS.POP.POP.type().get(HTYPE_RACE.get(r, pp))));
            b.NL();
            b.textLL(¤¤children);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), pp == null ? 0 : STATS.POP.POP.type().get(HTYPE_RACE.get(r, pp.child()))));
            b.NL();
            b.textL(Dic.¤¤Total);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), kidsIncoming(cl, r)));
            b.NL();

            if (!propagates(cl, r))
            {
                b.sep();
                b.warn(¤¤noProp);
                return;
            }

            {
                b.NL(4);
                GText t = b.text();
                t.add(¤¤days);
                t.insert(0, r.info.namePosessive);
                t.insert(1, r.physics.babyDays);
                t.insert(2, r.physics.childDays);
                b.add(t);
                b.sep();
            }

            BOOSTABLES.PHYSICS().REPRODUCTION_SPEED.hover(box, HCLASS_RACE.clP(r, cl), false);
            BOOSTABLES.PHYSICS().REPRODUCTION_AGE.hover(box, HCLASS_RACE.clP(r, cl), false);

            b.textLL(STATS.POP.POP.info().name);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), STATS.POP.POP.data(cl).get(r)));
            b.NL();
            b.textLL(¤¤fertile);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), fertile.data(cl).get(r)));
            b.NL();
            b.textLL(¤¤estimated);
            b.tab(6);
            b.add(GFORMAT.f(b.text(), kidsPerYear(cl, r), 2));
            b.NL();
            b.sep();

            b.textLL(¤¤limit);
            b.NL();
            b.textLL(Dic.¤¤limit);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), limit.get(HCLASS_RACE.clP(r, cl))));
            b.NL();

            b.textLL(Dic.¤¤Population);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), -POP.tot(cl, r)));
            b.NL();

            b.textLL(Dic.¤¤Inbound);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), -(POP.next(cl, r) - POP.tot(cl, r))));
            b.NL();

            b.textLL(Dic.¤¤Total);
            b.tab(6);
            b.add(GFORMAT.iIncr(b.text(), newInfantsAllowed(cl, r)));
            b.NL();

            b.NL();
        }

        public HTYPE parent(HCLASS cl)
        {
            if (cl == HCLASSES.CITIZEN())
                return HTYPES.PARENT();
            if (cl == HCLASSES.SLAVE())
                return HTYPES.PARENT_SLAVE();
            return null;
        }

        public HTYPE child(HCLASS cl)
        {
            if (cl == HCLASSES.CITIZEN())
                return HTYPES.CHILD();
            if (cl == HCLASSES.SLAVE())
                return HTYPES.CHILD_SLAVE();
            return null;
        }

        public int kidsIncoming(HCLASS cl, Race r)
        {
            HTYPE pp = parent(cl);
            if (pp == null)
                return 0;
            if (r == null)
            {
                int am = 0;
                foreach (Race race in RACES.all())
                    am += kidsIncoming(cl, race);
                return am;
            }

            return STATS.POP.POP.type().get(HTYPE_RACE.get(r, pp)) + STATS.POP.POP.type().get(HTYPE_RACE.get(r, pp.child()));
        }

        public double kidsPerYear(HCLASS cl, Race r)
        {
            if (parent(cl) == null)
                return 0;
            if (r == null)
            {
                int am = 0;
                foreach (Race race in RACES.all())
                    am += kidsPerYear(cl, race);
                return am;
            }
            return (fertile.data(cl).get(r) * BOOSTABLES.PHYSICS().REPRODUCTION_SPEED.get(HCLASS_RACE.clP(r, cl)));
        }

        public bool propagates(HCLASS cl, Race r)
        {
            if (parent(cl) == null)
                return false;
            if (r == null)
                return false;
            return r.physics.babyDays > 0;
        }

        public int newInfantsAllowed(HCLASS cl, Race r)
        {
            if (!propagates(cl, r))
                return 0;
            int am = POP.next(cl, r);
            return limit.get(HCLASS_RACE.clP(r, cl)) - am;
        }
    }
}