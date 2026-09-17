using System;
using System.Collections.Generic;
using game;
using game.event.engine;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;

namespace game.event.actions
{
    final class _DESTRUCTION : EventActionConstructor
    {
        _DESTRUCTION() : base("DESTRUCTION") { }

        public override EventAction Action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public final class Imp : EventAction
        {
            public final double death;
            public final double destruction;
            public final double degrade;

            Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                death = data.dTry("DEATH", 0, 1000, 0);
                destruction = data.dTry("DESTRUCTION", 0, 1000, 0);
                degrade = data.dTry("DEGRADE", 0, 1000, 0);
                data.checkUnused();
            }

            public override void setContext(Event event, EContext data)
            {
                dacc = 0;
                kacc = 0;
                accd = 0;
            }

            double dacc;
            double kacc;
            double accd;

            public override void update(Event event, EContext e, double ds, double second)
            {
                dacc += ds * destruction;
                while (dacc > 0)
                {
                    dacc -= 1;
                    int tx = RND.rInt(SETT.TWIDTH);
                    int ty = RND.rInt(SETT.THEIGHT);
                    GAME.ARMIES().map.breakIt(tx, ty);
                }
                kacc += ds * death;
                while (kacc > 0)
                {
                    kacc -= 1;
                    int tx = RND.rInt(SETT.TWIDTH);
                    int ty = RND.rInt(SETT.THEIGHT);

                    foreach (ENTITY ent in SETT.ENTITIES().getAtTile(tx, ty))
                    {
                        if (ent is Humanoid)
                            _SUBJECTS_KILL.slap(e, ent, 1.0, CAUSE_LEAVES.SLAYED());
                    }
                }
                accd += ds * degrade;
                while (accd > 0)
                {
                    accd -= 1;
                    int tx = RND.rInt(SETT.TWIDTH);
                    int ty = RND.rInt(SETT.THEIGHT);
                    SETT.MAINTENANCE().vandalise(tx, ty);
                }
            }
        }
    }
}