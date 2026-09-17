using System;
using System.Collections.Generic;
using game.battle.state;
using game.battle.util;
using init.constant;
using init.race;
using settlement.main;
using settlement.room.military.artillery;
using settlement.tilemap.generator;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util;
using world;
using world.map.regions.centre;

namespace game.battle.state
{
    public sealed class BattleStateGenerator
    {
        public SpecSide side1;
        public SpecSide side2;
        private readonly LinkedList<ROOM_ARTILLERY> a1 = new LinkedList<ROOM_ARTILLERY>();
        private readonly LinkedList<ROOM_ARTILLERY> a2 = new LinkedList<ROOM_ARTILLERY>();
        private Race r1;
        private Race r2;

        public void Generate(BattleState s, BattleStateSpec spec, Rec deploymentTiles)
        {
            side1 = makeSide(spec.player, true);
            side2 = makeSide(spec.enemy, false);

            DIR d = DIR.N;
            d = DIR.Get(spec.player.wCoo, spec.enemy.wCoo);

            if (d == DIR.C)
                d = DIR.N;

            if (!d.IsOrtho())
                d = d.Next(1);

            genMap(spec.player.wCoo.x(), spec.player.wCoo.y(), d);
            SETT.ROOMS().THRONE.init.place(SETT.TILE_BOUNDS.cX(), SETT.TILE_BOUNDS.cY(), 0);
            GAME.BATTLE_THREADS().pause();
            genArmies(s, spec.player, spec.enemy, deploymentTiles, d);
            SETT.init();
            Generator.paintMinimap();
        }

        private SpecSide makeSide(SpecSide side, bool sideA)
        {
            Race race = getRace(side.divs);
            if (sideA)
            {
                foreach (ROOM_ARTILLERY aa in SETT.ROOMS().ARTILLERY)
                {
                    for (int i = 0; i < side.artillery[aa.typeIndex()]; i++)
                        a1.add(aa);
                }
                r1 = race;
            }
            else
            {
                foreach (ROOM_ARTILLERY aa in SETT.ROOMS().ARTILLERY)
                {
                    for (int i = 0; i < side.artillery[aa.typeIndex()]; i++)
                        a2.add(aa);
                }
                r2 = race;
            }
            return side;
        }

        private static Race getRace(LIST<DivGeneration> divs)
        {
            int[] amount = Alloc.ii(RACES.all().size());
            foreach (DivGeneration d in divs)
            {
                amount[d.race] += d.indus.Length;
            }

            int best = 0;
            int bestV = 0;
            foreach (Race r in RACES.all())
            {
                if (amount[r.index] > bestV)
                {
                    best = r.index;
                    bestV = amount[r.index];
                }
            }
            return RACES.all().get(best);
        }

        private void genMap(int cx, int cy, DIR eDir)
        {
            cx = CLAMP.i(cx, 0, WORLD.TWIDTH() - 1);
            cy = CLAMP.i(cy, 0, WORLD.THEIGHT() - 1);

            GUTIL.flooder().init(this);
            GUTIL.flooder().pushSloppy(cx, cy, 0);
            int ts = 0;
            while (GUTIL.flooder().hasMore())
            {
                PathTile c = GUTIL.flooder().pollSmallest();
                ts++;
                if (BattleState.okWorldTile(c.x(), c.y(), eDir))
                {
                    GUTIL.flooder().done();
                    GAME.s().CreateFromWorldMap(c.x() - WCentre.TILE_DIM / 2, c.y() - WCentre.TILE_DIM / 2, true);
                    return;
                }
                for (int di = 0; di < DIR.ALL.size(); di++)
                {
                    DIR d = DIR.ALL.get(di);
                    if (WORLD.IN_BOUNDS(c, d))
                    {
                        GUTIL.flooder().pushSmaller(c, d, c.getValue() + d.tileDistance());
                    }
                }
            }

            GUTIL.flooder().done();
            throw new Errors.GameError("Unable to find location for battle " + cx + " " + cy + " " + ts);
        }

        private void genArmies(BattleState s, SpecSide a, SpecSide b, Rec tiles, DIR d)
        {
            for (int di = 0; di < Config.battle().DIVISIONS_PER_BATTLE; di++)
                GAME.ARMIES().division((short)di).info.menSet(0);

            tiles.set(
                SETT.TILE_BOUNDS.cX() + d.next(-2).x() * SETT.TWIDTH / 2, SETT.TILE_BOUNDS.cX() + d.next(3).x() * SETT.TWIDTH / 2,
                SETT.TILE_BOUNDS.cY() + d.next(-2).y() * SETT.TWIDTH / 2, SETT.TILE_BOUNDS.cY() + d.next(3).y() * SETT.TWIDTH / 2);
            tiles.makePositive();

            BattleStateGenArmy.genArmy(side1.divs, true, d.perpendicular(), a1, r1, side1.moraleBase);
            BattleStateGenArmy.genArmy(side2.divs, false, d, a2, r2, side2.moraleBase);

            GAME.ARMIES().initAndTeleport(GAME.ARMIES().divisions());
        }
    }
}