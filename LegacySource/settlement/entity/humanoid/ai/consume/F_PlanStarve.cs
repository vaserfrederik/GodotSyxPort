using System;
using System.Collections.Generic;
using settlement.main;
using init.type;
using settlement.entity.animal;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AIData;
using settlement.room.main;
using settlement.stats;
using settlement.thing;
using settlement.tilemap.terrain;
using util.text;

namespace settlement.entity.humanoid.ai.consume
{
    internal class F_PlanStarve : AIPLAN.PLANRES
    {
        private readonly AISUB sub;
        private readonly AIDataSuspender suspender;

        private static readonly string ¤¤cannibal = "Eating a corpse";
        private static readonly string ¤¤starving = "Starving";
        private static readonly string ¤¤eating = "Eating Dirt";

        static F_PlanStarve()
        {
            D.ts(typeof(F_PlanStarve));
        }

        public F_PlanStarve(AISUB sub, AIDataSuspender suspender) : base("dangerStarve")
        {
            this.sub = sub;
            this.suspender = suspender;
        }

        public readonly FinderMiscWithoutDest edible = new FinderMiscWithoutDest(32)
        {
            protected override bool has() => SETT.WEATHER().growthRipe.cropsAreRipe(),
            public override bool isTile(int tx, int ty)
            {
                Room r = SETT.ROOMS().map.get(tx, ty);
                if (r != null && r is ANIMAL_ROOM_RUINER)
                {
                    return ((ANIMAL_ROOM_RUINER)r).canBeGraced(tx, ty);
                }
                return TERRAIN().get(tx, ty) is TGrowable && ((TGrowable)TERRAIN().get(tx, ty)).isEdible(tx, ty) && ((TGrowable)TERRAIN().get(tx, ty)).size.get(tx, ty) > 0;
            }
        };

        private Corpse corpse(int tx, int ty)
        {
            foreach (Thing t in SETT.THINGS().get(tx, ty))
            {
                if (t is Corpse c)
                {
                    if (c.hasMeat())
                    {
                        return c;
                    }
                }
            }
            return null;
        }

        public readonly FinderMiscWithoutDest corpses = new FinderMiscWithoutDest(32)
        {
            protected override bool has() => true,
            public override bool isTile(int tx, int ty) => corpse(tx, ty) != null
        };

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            if (NEEDS.TYPES().HUNGER.stat().stat().indu().isMax(a.indu()))
            {
                AIManager.dead = CAUSE_LEAVES.STARVED();
            }

            if (!suspender.is(d))
            {
                if (edible.find(a.physics.tileC(), d.path))
                {
                    return goEatTerrain.set(a, d);
                }
                if (corpses.find(a.physics.tileC(), d.path))
                {
                    return goEatCorpse.set(a, d);
                }
                suspender.suspend(d);
            }

            // misery
            return actCrazy.set(a, d);
        }

        private readonly Resumer goEatCorpse = new Resumer(¤¤cannibal)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d) => AI.SUBS().walkTo.pathRun(a, d),
            public override AISubActivation res(Humanoid a, AIManager d) => eatCorpse.set(a, d),
            public override bool con(Humanoid a, AIManager d) => corpse(d.path.destX(), d.path.destY()) != null,
            public override void can(Humanoid a, AIManager d) { }
        };

        private readonly Resumer eatCorpse = new Resumer(¤¤cannibal)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d) => sub.activate(a, d),
            public override AISubActivation res(Humanoid a, AIManager d)
            {
                Corpse c = corpse(d.path.destX(), d.path.destY());
                if (c != null)
                {
                    SETT.ROOMS().CANNIBAL.reportCannibal(c.race());
                    c.removeMeat();
                    STATS.FOOD().eat(a, 0, 0);
                    NEEDS.TYPES().HUNGER.stat().fix(a.indu());
                    return null;
                }
                // kill other here
                return null;
            },
            public override bool con(Humanoid a, AIManager d) => true,
            public override void can(Humanoid a, AIManager d) { }
        };

        private readonly Resumer goEatTerrain = new Resumer(¤¤eating)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d) => AI.SUBS().walkTo.pathRun(a, d),
            public override AISubActivation res(Humanoid a, AIManager d) => eatTerrain.set(a, d),
            public override bool con(Humanoid a, AIManager d) => edible.isTile(d.path.destX(), d.path.destY()),
            public override void can(Humanoid a, AIManager d) { }
        };

        private readonly Resumer eatTerrain = new Resumer(¤¤eating)
        {
            public override AISubActivation setAction(Humanoid a, AIManager d) => sub.activate(a, d),
            public override AISubActivation res(Humanoid a, AIManager d)
            {
                if (edible.isTile(d.path.destX(), d.path.destY()))
                {
                    STATS.FOOD().eat(a, 0, 0);
                    NEEDS.TYPES().HUNGER.stat().fix(a.indu());
                    TERRAIN().get(d.path.destX(), d.path.destY()).clearing().clear1(d.path.destX(), d.path.destY());
                    Room r = SETT.ROOMS().map.get(d.path.destX(), d.path.destY());
                    if (r != null && r.destroyTileCan(d.path.destX(), d.path.destY()))
                    {
                        r.destroyTile(d.path.destX(), d.path.destY());
                    }

                    return null;
                }
                // kill other here
                return null;
            },
            public override bool con(Humanoid a, AIManager d) => true,
            public override void can(Humanoid a, AIManager d) { }
        };

        private readonly Resumer actCrazy = new Resumer(¤¤starving)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d) => AI.SUBS().desperate.activate(a, d),
            protected override AISubActivation res(Humanoid a, AIManager d) => null,
            public override bool con(Humanoid a, AIManager d) => true,
            public override void can(Humanoid a, AIManager d) { }
        };
    }
}