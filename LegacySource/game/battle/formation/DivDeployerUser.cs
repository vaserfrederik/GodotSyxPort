using System;
using System.Collections.Generic;
using game.battle.formation;
using game.GAME;
using game.battle.Armies;
using game.battle.Army;
using game.battle.div.Div;
using game.battle.formation.DivDeployer.DivDeployB;
using game.battle.thread.order.BattleOrderTask;
using init.constant.C;
using snake2d.SPRITE_RENDERER;
using snake2d.util.datatypes.RECTANGLE;
using snake2d.util.datatypes.VectorImp;
using snake2d.util.map.MAP_BOOLEAN;
using snake2d.util.sets.ArrayList;
using snake2d.util.sets.LIST;
using util.GUTIL;
using util.rendering.RenderData;

public class DivDeployerUser
{
    private readonly ArrayList<DivDeployB> all;
    private readonly ArrayList<DivDeployB> selection;
    public readonly DivDeployer deployer;
    private readonly int clampM = ~((C.TILE_SIZE - 1) >> 2);

    private ArrayList<DivDeployB> tmp = new ArrayList<DivDeployB>(Armies.DIVISIONS);
    private ArrayList<DivDeployB> selected = new ArrayList<DivDeployB>(Armies.DIVISIONS);
    private readonly VectorImp vec = new VectorImp();
    private static readonly ArrayList<Div> tmp2 = new ArrayList<Div>(1);
    private readonly DivFormationImp fTmp = new DivFormationImp();
    private readonly BattleOrderTask task = new BattleOrderTask();

    public DivDeployerUser(LIST<Army> armies)
    {
        int size = Armies.DIVISIONS;
        all = new ArrayList<DivDeployB>(size);
        selection = new ArrayList<DivDeployB>(size);
        foreach (Army a in armies)
        {
            foreach (Div d in a.divisions())
            {
                DivDeployB dep = new DivDeployB();
                dep.div = d;
                all.add(dep);
            }
        }
        this.deployer = new DivDeployer(GUTIL.pathTools())
        {
            protected override bool isDeployable(int px, int py, Army a)
            {
                return !blocked(px, py, a);
            }
        };
    }

    protected bool blocked(int x, int y, Army a)
    {
        return false;
    }

    public bool render(SPRITE_RENDERER ren, LIST<Div> divs, int x1, int x2, int y1, int y2, RenderData data)
    {
        x1 += (~clampM + 1) / 2;
        y1 += (~clampM + 1) / 2;
        x2 += (~clampM + 1) / 2;
        y2 += (~clampM + 1) / 2;

        x1 &= clampM;
        y1 &= clampM;
        x2 &= clampM;
        y2 &= clampM;

        selection.clear();
        foreach (Div d in divs)
        {
            if (d.menNrOf() == 0)
                continue;
            DivDeployB dep = all.get(d.index());
            dep.div = d;
            selection.add(dep);
        }

        LIST<DivDeployB> result = init(selection, x1, x2, y1, y2);
        bool dep = false;
        foreach (DivDeployB b in result)
        {
            DivFormationImp d = deployer.deploy(b.div.info, b.div.menNrOf(), b.div.settings().formation, b.x1, b.y1, b.dx, b.dy, b.width, GAME.ARMIES().player());
            if (d != null)
                dep = true;
            DivRenderer.render(ren, d, data);
        }
        return dep;
    }

    public void render(SPRITE_RENDERER ren, DivFormationImp d, RenderData data)
    {
        DivRenderer.render(ren, d, data);
    }

    public static void addSecretBlocker(MAP_BOOLEAN block)
    {

    }

    public bool deploy(LIST<Div> divs, int x1, int x2, int y1, int y2)
    {
        selection.clear();

        x1 += (~clampM + 1) / 2;
        y1 += (~clampM + 1) / 2;
        x2 += (~clampM + 1) / 2;
        y2 += (~clampM + 1) / 2;

        x1 &= clampM;
        y1 &= clampM;
        x2 &= clampM;
        y2 &= clampM;

        foreach (Div d in divs)
        {
            if (d.menNrOf() == 0)
                continue;
            DivDeployB dep = all.get(d.index());
            dep.div = d;
            selection.add(dep);
        }

        LIST<DivDeployB> result = init(selection, x1, x2, y1, y2);
        bool dep = false;
        foreach (DivDeployB b in result)
        {
            DivFormationImp d = deployer.deploy(b.div.info, b.div.menNrOf(), b.div.settings().formation, b.x1, b.y1, b.dx, b.dy, b.width, b.div.army());

            if (d != null)
            {
                dep = true;
                b.div.order().dest.set(d);
                task.move(b.div);
                b.div.order().task.set(task);
            }
        }
        return dep;
    }

    public void deploy(Div div, int x1, int x2, int y1, int y2)
    {
        tmp2.clear();
        tmp2.add(div);
        deploy(tmp2, x1, x2, y1, y2);
    }

    public void deploy(Div div, int dx, int dy)
    {
        div.order().dest.get(fTmp);
        DivFormationImp d = deployer.deploy(
            div.info,
            div.menNrOf(),
            fTmp.formation(),
            fTmp.start().x() + dx, fTmp.start().y() + dy,
            fTmp.dx(), fTmp.dy(), fTmp.width(), GAME.ARMIES().player());

        if (d != null && d.deployed() != 0)
        {
            task.move(div);
            div.order().dest.set(d);
            div.order().task.set(task);
        }
    }

    public bool isBlocked(int x, int y, int tileSize, Army a)
    {
        return DivPlacability.pixelIsBlocked(x, y, tileSize, GAME.ARMIES().player()) && !blocked(x, y, a);
    }

    private LIST<DivDeployB> init(LIST<DivDeployB> divs, int x1, int x2, int y1, int y2)
    {
        selected.clear();
        double distFull = vec.set(x1, y1, x2, y2);
        {
            selected.clear();
            if (divs.size() * C.TILE_SIZE > distFull)
            {
                return selected;
            }
        }

        double menTotal = 0;
        {
            tmp.clear();
            foreach (DivDeployB d in divs)
            {
                menTotal += d.div.menNrOf();
                if (d.div.menNrOf() == 0)
                    continue;
                tmp.add(d);
            }
        }
        {
            while (tmp.size() > 0)
            {
                double smallesD = double.MaxValue;
                int s = -1;
                for (int i = 0; i < tmp.size(); i++)
                {
                    RECTANGLE d = tmp.get(i).div.position().body();
                    double ddx = d.cX() - x1;
                    double ddy = d.cY() - y1;
                    double dist = Math.Sqrt(ddx * ddx + ddy * ddy);
                    if (dist < smallesD)
                    {
                        smallesD = dist;
                        s = i;
                    }
                }
                DivDeployB dr = tmp.get(s);
                selected.add(dr);
                tmp.remove(s);
            }
        }
        if (selected.isEmpty())
            return selected;
        {
            DIV_FORMATION lastF = selected.get(0).div.settings().formation;
            double distGaps = 0;
            foreach (DivDeployB d in selected)
            {
                if (d.div.settings().formation.Equals(lastF))
                    continue;
                lastF = d.div.settings().formation;
                distGaps++;
            }

            distFull -= distGaps * C.TILE_SIZE;
        }

        foreach (DivDeployB d in selected)
        {
            double dist = distFull * (d.div.menNrOf() / menTotal);
            d.width = (int)dist;
            d.x1 = x1;
            d.y1 = y1;
            d.dx = vec.nX();
            d.dy = vec.nY();

            x1 += (int)(dist / d.div.settings().formation.size(d.div)) * vec.nX();
            y1 += (int)(dist / d.div.settings().formation.size(d.div)) * vec.nY();
        }

        return selected;
    }

    public void stop(LIST<Div> divs)
    {
    }
}