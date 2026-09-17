using System;
using System.Collections.Generic;
using game;
using game.battle.div;
using game.battle.util.ArmyFormations;
using game.battle.util.DivGeneration;
using init.constant;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.military.artillery;
using settlement.stats;
using settlement.stats.equip;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.state
{
    internal static class BattleStateGenArmy
    {
        // private static readonly Tree<WDivId> sort = new Tree<WDivId>(Config.battle().DIVISIONS_PER_ARMY)
        // {
        //     protected override bool IsGreaterThan(WDivId current, WDivId cmp)
        //     {
        //         return !current.gen.isRange;
        //     }
        // };

        public static void GenArmy(ArrayList<DivGeneration> ss, bool player, DIR d, LIST<ROOM_ARTILLERY> art, Race artrace, double moraleBase)
        {
            GAME.ARMIES().factors.Init(player ? GAME.ARMIES().player() : GAME.ARMIES().enemy(), moraleBase);

            ArmyFormation ff = player ? GAME.battle().formations.player : GAME.battle().formations.all.Rnd();

            ArrayList<ArmyFormationDiv> divs = ff.GetFirstRow(ss);

            int w = 12;
            int cx = SETT.TILE_BOUNDS.cX() + d.x() * 120 + d.Next(2).x() * w / 2;
            int cy = SETT.TILE_BOUNDS.cY() + d.y() * 120 + d.Next(2).y() * w / 2;

            int depth = 0;
            for (; depth < 256 && !divs.IsEmpty(); depth += w + 1)
            {
                for (int width = 0; width < 220; width++)
                {
                    if (divs.IsEmpty())
                        break;
                    int x1 = cx + depth * d.x() + d.Next(2).x() * width;
                    int y1 = cy + depth * d.y() + d.Next(2).y() * width;
                    ArmyFormationDiv div = ff.Get(divs, 1.0 - width / 220.0, width / 220.0, 1.0 - depth / 256.0, depth / 256.0);
                    if (DivPlace(div, player, w, d, x1, y1))
                    {
                        width -= w;
                        if (width < 0)
                            width = 0;
                        divs.Remove(div);
                    }

                    if (divs.IsEmpty())
                        break;

                    x1 = cx + depth * d.x() - d.Next(2).x() * width;
                    y1 = cy + depth * d.y() - d.Next(2).y() * width;
                    div = ff.Get(divs, 1.0 - width / 220.0, width / 220.0, 1.0 - depth / 256.0, depth / 256.0);
                    if (DivPlace(div, player, w, d, x1, y1))
                    {
                        width -= w;
                        if (width < 0)
                            width = 0;
                        divs.Remove(div);
                    }
                }
            }

            depth += BattleStateArt.PlaceArt(cx, cy, depth, d, art, artrace, player);

            if (player)
            {
                PlaceThrone(cx, cy, depth, d);
            }
        }

        private static void PlaceThrone(int cx, int cy, int depth, DIR d)
        {
            depth += 15;
            int x1 = cx + depth * d.x();
            int y1 = cy + depth * d.y();
            int dd = 0;
            for (int di = 0; di < DIR.ORTHO.Size; di++)
            {
                if (DIR.ORTHO.Get(di) == d.Perpendicular())
                    break;
                dd++;
            }

            SETT.ROOMS().THRONE.Init.Place(x1, y1, dd);
        }

        private static int men;
        private static readonly Rec tmp = new Rec();
        private static readonly ArrayList<Div> tmpDivs = new ArrayList<Div>(1);

        private static bool DivPlace(ArmyFormationDiv wdiv, bool player, int widthMax, DIR d, int tx1, int ty1)
        {
            DivGeneration div = wdiv.g;

            if (div.indus.Length == 0)
                return true;

            int depthMax = (int)Math.Ceiling((double)div.indus.Length / widthMax);
            if (d.x() != 0)
            {
                int bi = depthMax;
                depthMax = widthMax;
                widthMax = bi;
            }

            d = d.Next(-1);

            tmp.Set(tx1, tx1 + d.x() * depthMax, ty1, ty1 + d.y() * widthMax);
            tmp.MakePositive();

            men = 0;

            foreach (COORDINATE c in tmp)
            {
                if (SETT.PATH().Availability.Get(c).player <= 0 || SETT.ENTITIES().HasAtTile(c.x(), c.y()))
                {
                    return false;
                }
            }

            Div adiv = (player ? GAME.ARMIES().player() : GAME.ARMIES().enemy()).divisions().Get(wdiv.divID);

            adiv.Settings().Clear();
            adiv.Settings().MusteringSet(true);
            adiv.Settings().FireAtWill = true;

            Race race = RACES.all().Get(div.race);
            adiv.Info.RaceSet(race);
            adiv.Info.MenSet(div.indus.Length);
            adiv.Info.BannerISet(div.bannerI);
            adiv.Info.Name().Clear().Add(div.name);

            HTYPE type = player ? HTYPES.SOLDIER() : HTYPES.ENEMY();

            int am = div.indus.Length;

            if (am > Config.battle().MEN_PER_DIVISION)
                throw new RuntimeException(div + " " + am);

            foreach (COORDINATE c in tmp)
            {
                if (men >= div.indus.Length)
                    break;

                Humanoid h = SETT.HUMANOIDS().Create(race, c.x(), c.y(), type, CAUSE_ARRIVES.SOLDIER_RETURN());
                if (!h.IsRemoved())
                {
                    STATS.NEEDS().Clear(h.indu());
                    h.indu().CopyFrom(div.indus[men]);
                    STATS.BATTLE().BasicTraining.SetD(h.indu(), 1.0);

                    foreach (EquipBattle m in STATS.EQUIP().BATTLE_ALL())
                    {
                        m.Set(h.indu(), m.Get(div.indus[men]));
                    }

                    men++;
                    h.SetDivision(adiv);
                }
            }

            if (men == 0)
                return false;

            adiv.Info.CopySettings(wdiv.g.target);
            foreach (EquipRange m in STATS.EQUIP().RANGED())
            {
                m.AmmoClear(adiv);
            }
            d = d.Next(-1);

            int x1 = tx1 * C.TILE_SIZE + C.TILE_SIZEH;
            int y1 = ty1 * C.TILE_SIZE + C.TILE_SIZEH;
            int x2 = x1 + d.x() * depthMax * C.TILE_SIZE;
            int y2 = y1 + d.y() * widthMax * C.TILE_SIZE;
            tmpDivs.Clear();
            tmpDivs.Add(adiv);
            GAME.ARMIES().placer.Deploy(tmpDivs, x1, x2, y1, y2);
            GAME.ARMIES().InitAndTeleport(tmpDivs);

            return true;
        }

        internal class WDivId
        {
            public readonly int di;
            public readonly DivGeneration gen;

            public WDivId(int id, DivGeneration gen)
            {
                this.di = id;
                this.gen = gen;
            }
        }
    }
}