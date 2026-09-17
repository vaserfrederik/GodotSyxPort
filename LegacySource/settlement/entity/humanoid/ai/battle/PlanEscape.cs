using settlement.entity.humanoid.ai.battle;
using game.battle.div;
using init.constant;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.battle
{
    final class PlanEscape
    {
        public PlanEscape()
        {
        }

        private static CharSequence ¤¤Reforming = "¤Retreating";
        private final int cutDistance = 12;

        static
        {
            D.ts(typeof(PlanEscape));
        }

        public AIPLAN plan(Humanoid a, AIManager d)
        {
            Div div = a.division();
            if (div != null && div.settings().mustering() && div.morale() <= 0)
            {
                return AI.modules().battle.dessert;
            }

            if (!can(a, d))
                return null;

            return plan;
        }

        private bool can(Humanoid a, AIManager d)
        {
            if (!BattleUtil.shouldMoveIntoDivPosition(a, d))
                return false;

            Div div = a.division();

            if (div == null)
                return false;
            COORDINATE pos = div.reporter.getPixel(a);

            int dist = (int)pos.tileDistanceTo(a.body().cX(), a.body().cY());

            if (dist < C.TILE_SIZE)
            {
                if (!div.settings().guard)
                    return false;
                if (dist < C.TILE_SIZEH / 2)
                    return false;
            }

            for (DIR dir : DIR.ALLC)
            {
                if (SETT.PATH().finders.entity.getEnemies(a, a.tc().x() + dir.x(), a.tc().y() + dir.y()) > 0)
                    return true;
            }

            return false;
        }

        private final AIPLAN plan = new AIPLAN.PLANRES("BATTLE_ESCAPE")
        {
            private final VectorImp vec = new VectorImp();

            protected override AISubActivation init(Humanoid a, AIManager d)
            {
                Div div = a.division();
                if (div == null)
                    return fail.set(a, d);

                if (!div.reporter.posHas(a))
                    return fail.set(a, d);

                int startX = a.body().cX();
                int startY = a.body().cY();
                int endX = a.division().reporter.getPixel(a).x();
                int endY = a.division().reporter.getPixel(a).y();

                double m = vec.set(startX, startY, endX, endY);

                if (m > cutDistance)
                    return path.set(a, d);

                for (int i = C.TILE_SIZE; i < m; i += C.TILE_SIZE)
                {
                    int tx = (int)(startX + i * vec.nX() * C.ITILE_SIZE);
                    int ty = (int)(startX + i * vec.nX() * C.ITILE_SIZE);
                    int tx2 = (int)(startX + (i - C.TILE_SIZE) * vec.nX() * C.ITILE_SIZE);
                    int ty2 = (int)(startX + (i - C.TILE_SIZE) * vec.nX() * C.ITILE_SIZE);

                    if (SETT.PATH().solidity.is(tx, ty) || SETT.PATH().solidity.is(tx2, ty) || SETT.PATH().solidity.is(tx, ty2))
                        return path.set(a, d);
                }

                return cutTo.set(a, d);
            }

            private bool sett(Humanoid a, AIManager d)
            {
                Div div = a.division();
                if (div == null)
                    return false;

                double m = COORDINATE.tileDistance(a.body().cX(), a.body().cY(), a.division().reporter.getPixel(a));
                if (m > C.TILE_SIZEH)
                    return true;
                return false;
            }

            private Resumer cutTo = new Resumer(¤¤Reforming)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    if (sett(a, d))
                    {
                        AISTATE s = AI.STATES().PUSH_TO.move(a, d, a.division().reporter.getPixel(a).x(), a.division().reporter.getPixel(a).y(), 0.5 + RND.rFloat0(0.5), 0.75);
                        return AI.SUBS().single.activate(a, d, s);
                    }
                    return fail.set(a, d);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    if (BattleUtil.shouldMoveIntoDivPosition(a, d))
                        return init(a, d);
                    return null;
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                }

                public override bool event(Humanoid a, AIManager d, HEventData e)
                {
                    if (e.event == HEvent.MEET_ENEMY)
                    {
                        if (e.speedHasChanged)
                            a.speed.setPrevDir();
                        return true;
                    }

                    if (e.event == HEvent.COLLISION_SOFT)
                    {
                        a.speed.setPrevDir();
                        return true;
                    }

                    return InterBattle.listener.event(a, d, e);
                }
            };

            private Resumer path = new Resumer(¤¤Reforming)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    if (!d.path.request(a, a.division().reporter.getTile(a).x(), a.division().reporter.getTile(a).y()))
                        return fail.set(a, d);

                    return nextTile(a, d);
                }

                private AISubActivation nextTile(Humanoid a, AIManager d)
                {
                    if (a.division().reporter.getTile(a).tileDistanceTo(d.path.destX(), d.path.destY()) > 2)
                        return init(a, d);

                    if (COORDINATE.tileDistance(a.body().cX(), a.body().cY(), d.path().getSettCX(), d.path().getSettCY()) < C.SCALE * 4)
                    {
                        if (d.path.hasNext())
                            d.path.setNext();
                        else
                            return fail.set(a, d);
                    }

                    if (!d.path.isSuccessful())
                        return fail.set(a, d);

                    AISTATE s = AI.STATES().PUSH_TO.move(a, d, d.path.getSettCX(), d.path.getSettCY(), 0.5 + RND.rFloat0(0.5), 0.75);
                    return AI.SUBS().single.activate(a, d, s);
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    if (!BattleUtil.shouldMoveIntoDivPosition(a, d))
                        return null;
                    return nextTile(a, d);
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                }

                public override bool event(Humanoid a, AIManager d, HEventData e)
                {
                    if (e.event == HEvent.MEET_ENEMY)
                    {
                        if (a.division() != null)
                            a.speed.setDirCurrent(a.division().dir());
                        else
                            a.speed.setPrevDir();
                        return true;
                    }

                    if (e.event == HEvent.COLLISION_SOFT)
                    {
                        if (a.division() != null)
                            a.speed.setDirCurrent(a.division().dir());
                        else
                            a.speed.setPrevDir();
                        return true;
                    }

                    return InterBattle.listener.event(a, d, e);
                }
            };

            private Resumer fail = new Resumer(¤¤Reforming)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    if (a.division() != null)
                        a.speed.setDirCurrent(a.division().dir());
                    else
                        a.speed.setPrevDir();
                    return AI.SUBS().single.activate(a, d, AI.STATES().STAND_SWORD.activate(a, d, 0.5));
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    return null;
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                }

                public override bool event(Humanoid a, AIManager d, HEventData e)
                {
                    if (e.event == HEvent.MEET_ENEMY)
                    {
                        if (a.division() != null)
                            a.speed.setDirCurrent(a.division().dir());
                        else
                            a.speed.setPrevDir();
                        return true;
                    }

                    if (e.event == HEvent.COLLISION_SOFT)
                    {
                        if (a.division() != null)
                            a.speed.setDirCurrent(a.division().dir());
                        else
                            a.speed.setPrevDir();
                        return true;
                    }

                    return InterBattle.listener.event(a, d, e);
                }
            };

            public override double poll(Humanoid a, AIManager d, HPollData e)
            {
                if (e.type == HPoll.COLLIDES)
                    if (e.other is Humanoid)
                    {
                        return ((Humanoid)e.other).indu().hostile() != a.indu().hostile() ? 1 : 0;
                    }
                    else
                    {
                        return 1;
                    }
                return InterBattle.listener.poll(a, d, e);
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event == HEvent.MEET_ENEMY)
                    return true;
                if (e.event == HEvent.COLLISION_SOFT)
                {
                    return true;
                }

                return InterBattle.listener.event(a, d, e);
            }
        };
    }
}