using System;
using System.Collections.Generic;
using game.battle.div;
using game.battle.thread.general;
using init.constant;
using settlement.main;
using settlement.path;
using settlement.room.main.throne;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    final class StepEarlyThrone
    {
        private readonly StrategosUtil util;
        private readonly Context context;
        private readonly ArrayList<Dep> dall = new ArrayList<Dep>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly ArrayList<Dep> deps = new ArrayList<Dep>(Config.battle().DIVISIONS_PER_ARMY);

        public StepEarlyThrone(StrategosUtil util, Context context)
        {
            this.util = util;
            this.context = context;
            while (dall.HasRoom())
                dall.Add(new Dep());
        }

        public void se2tToThrone()
        {
            if (context.blob.Is(THRONE.coo()))
                return;

            context.map.Clear();
            deps.ClearSloppy();
            context.block.Clear();

            bool has = false;

            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = util.GetArmy().Divisions().Get(di);
                if (d.Active() && !context.deployedToLine.Get(di) && !d.Status().IsFighting())
                {
                    context.map.Add(d);
                    has = true;
                }
            }

            if (!has)
                return;

            Flooder f = util.Flooder.GetFlooder();
            f.Init(this);
            for (int i = 0; i < DIR.ORTHO.Size(); i++)
                f.PushSloppy(THRONE.coo(), DIR.ORTHO.Get(i), 0);

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();

                LIST<Div> divs = context.map.Get(t.x(), t.y());
                if (divs.Size() > 0 && IsUnblobbed(t))
                {
                    foreach (Div div in divs)
                    {
                        PreDeploy(t, div);
                    }
                }

                for (int di = 0; di < DIR.ALL.Size(); di++)
                {
                    DIR dir = DIR.ALL.Get(di);
                    int dx = t.x() + dir.x();
                    int dy = t.y() + dir.y();
                    if (SETT.IN_BOUNDS(dx, dy))
                    {
                        double cost = Cost(util, dx, dy);
                        if (cost > 0 && !dir.IsOrtho())
                        {
                            cost = Math.Min(cost, Cost(util, dx, t.y()));
                            cost = Math.Min(cost, Cost(util, t.x(), dy));
                        }
                        if (cost > 0)
                        {
                            f.PushSmaller(dx, dy, t.GetValue() + dir.TileDistance() * cost, t);
                        }
                    }
                }
            }
            f.Done();

            int am = 0;

            foreach (Dep dep in deps)
            {
                Div div = util.GetArmy().Divisions().Get(dep.di);
                if (util.DivDeployer.DeployTile(div, dep.tx, dep.ty, dep.d) != null)
                {
                    am++;
                    context.deployedToLine.Set(div.IndexArmy(), true);
                    if (am > 3)
                        break;
                }
            }
        }

        private readonly Rec tiles = new Rec();

        private void PreDeploy(PathTile t, Div div)
        {
            while (t.Parent != null && !context.block.Is(t.Parent))
            {
                t = t.Parent;
            }
            if (context.block.Is(t))
                return;

            Dep res = dall.Get(deps.Size());
            res.di = div.IndexArmy();
            res.tx = t.x();
            res.ty = t.y();
            res.d = DIR.Get(THRONE.coo(), t);
            deps.Add(res);

            int w = (int)(Math.Sqrt(div.Men()) + 2);
            tiles.SetDim(w);
            tiles.MoveC(t);

            foreach (COORDINATE c in tiles)
            {
                context.block.Set(c, true);
            }
        }

        private bool IsUnblobbed(PathTile t)
        {
            while (t != null)
            {
                if (context.blob.Is(t))
                    return false;
                t = t.Parent;
            }
            return true;
        }

        public static double Cost(StrategosUtil context, int dx, int dy)
        {
            AVAILABILITY a = SETT.PATH().availability.Get(dx, dy);
            if (a.IsSolid(context.GetArmy()) || SETT.TERRAIN().Get(dx, dy) is TFortification.Tile)
            {
                return -1;
            }
            else
            {
                double res = 1; // ArmyAIUtil.map().hasEnemy.is(dx, dy, c.army) ? 1 : 10;
                double s = SETT.ENV().map.SPACE.Get(dx, dy);
                if (s < 0.5)
                    return res + 2 + a.movementSpeedI;
                return res + a.movementSpeedI;
            }
        }

        private class Dep
        {
            public int di;
            public int tx;
            public int ty;
            public DIR d;
        }
    }
}