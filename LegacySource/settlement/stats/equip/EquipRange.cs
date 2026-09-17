using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Settlement.Stats.Equip
{
    public class EquipRange : EquipBattle
    {
        public readonly Projectile Projectile;
        public readonly int AmmunitionMax;
        public readonly double AmmunitionReplenishHours;
        public readonly short TIndex;

        private readonly double[] drawInters = new double[Config.Battle.DIVISIONS_PER_BATTLE];
        private readonly int[] drawIntersI = new int[Config.Battle.DIVISIONS_PER_BATTLE];
        private readonly double[] ammoWasted = new double[Config.Battle.DIVISIONS_PER_BATTLE];
        public readonly Boostable Boostable;

        private static readonly string ¤¤ammoC = "Ammunition (Current)";
        private static readonly string ¤¤ammoR = "Am. Replenish Time (hours)";
        private static readonly string ¤¤ammoU = "Unlimited ammunition!";

        static EquipRange()
        {
            D.ts(typeof(EquipRange));
        }

        public EquipRange(string key, PATH path, LISTE<Equip> all, LISTE<EquipRange> type, LISTE<EquipBattle> mil, StatsInit init, KeyMap<TILE_SHEET> spriteMap)
            : base("RANGED", key, path, all, mil, init, spriteMap)
        {
            tIndex = (short)type.Add(this);
            Json data = JsonConvert.DeserializeObject<Json>(File.ReadAllText(path.Get(key)));

            AmmunitionMax = data.i("AMMUNITION_AMOUNT", 1, 255);
            AmmunitionReplenishHours = data.d("AMMUNITION_REPLENISH_TIME_HOURS");

            Boostable = BOOSTING.Push("RANGED_" + key, 0.1, Dic.¤¤Skill + ": " + resource.name,
                Dic.¤¤Skill + ": " + resource.name, resource.icon(), BOOSTABLES.BATTLE());
            Projectile = new Projectile.ProjectileImp(data, "EQUIP_" + key);

            IUpdater up = new IUpdater(Config.Battle.DIVISIONS_PER_BATTLE, 10.0)
            {
                Update = (i, timeSinceLast) =>
                {
                    Div d = GAME.ARMIES().division((short)i);
                    if (d.menNrOf() == 0)
                    {
                        ammoWasted[i] = 0;
                        return;
                    }

                    int men = d.men();

                    int m = men * AmmunitionMax;
                    if (ammoWasted[i] > m)
                        ammoWasted[i] = m;


                    if (d.player() && GAME.ARMIES().enemy().men() == 0)
                    {
                        ammoWasted[i] -= men * timeSinceLast * TIME.secondsPerDayI() * TIME.hoursPerDay() / AmmunitionReplenishHours;
                        if (ammoWasted[i] < 0)
                            ammoWasted[i] = 0;
                    }
                }
            };

            init.upers.Add(new StatUpdatable
            {
                Update = ds => up.Update(ds)
            });

            init.savers.Put("EQUIP_AMMO_" + key, new SAVABLE
            {
                Save = file =>
                {
                    up.Save(file);
                    file.Ds(ammoWasted);
                },

                Load = file =>
                {
                    up.Load(file);
                    file.Ds(ammoWasted);
                },

                Clear = () =>
                {
                    up.Clear();
                    ammoWasted.Fill(0.0);
                }
            });
        }

        public double AmmunitionD(Div div)
        {
            if (div.men() == 0)
                return 0;
            double m = div.men() * AmmunitionMax;
            double a = ammoWasted[div.index()];
            if (a > m)
                a = m;
            return (m - a) / m;
        }

        public double AmmunitionPerMan(Div div)
        {
            return AmmunitionD(div) * AmmunitionMax;
        }

        public void AmmunitionClear(Div div)
        {
            ammoWasted[div.index()] = 0;
        }

        public double Ref(Induvidual a)
        {
            return Ref((double)get(a) / equipMax, Boostable.Get(a));
        }

        public double Ref(Div div)
        {
            return Ref((double)stat.div().GetD(div), Boostable.Get(div));
        }

        public double Ref(double equip, double skill)
        {
            return equip * (0.2 + equip * 0.8) * skill;
        }

        public void Launch(Humanoid a, Trajectory j)
        {
            double refValue = Ref(a.indu());
            double ran = 1.0 - Projectile.Accuracy(refValue);
            int x = a.body().cX() + a.speed.dir().x() * C.TILE_SIZEH;
            int y = a.body().cY() + a.speed.dir().y() * C.TILE_SIZEH;
            int h = SProjectiles.ReleaseHeight(a.tc().x(), a.tc().y());
            SETT.PROJS().Launch(x, y, h, j, a.division().settings().ammo().projectile, ran, refValue, a);

            if (AmmunitionReplenishHours > 0)
                ammoWasted[a.division().index()]++;

            double dex = 2.0 * STATS.NEEDS().EXHASTION.indu().max(a.indu()) / AmmunitionMax;
            int ex = (int)dex;
            if (dex - ex > RND.rFloat())
                ex++;
            STATS.NEEDS().EXHASTION.indu().inc(a.indu(), ex);
        }

        public double DrawInter(Div div)
        {
            if ((GAME.updateI() & ~0b011) != drawIntersI[div.index()])
            {
                double reloadSeconds = Projectile.reloadSeconds(Ref(div));
                double t = TIME.currentSecond();
                double inter = reloadSeconds;
                double tt = t / inter;
                drawInters[div.index()] = tt - (int)tt;
                drawIntersI[div.index()] = GAME.updateI() & ~0b011;
            }

            return drawInters[div.index()];
        }

        public override void Hover(GUI_BOX box)
        {
            base.Hover(box);
            GBox b = (GBox)box;
            b.sep();
            Projectile.Hover(box, resource.name);
            b.NL(8);
            if (AmmunitionReplenishHours > 0)
            {
                b.textL(Dic.¤¤Ammunition);
                b.tab(6);
                b.Add(GFORMAT.i(b.text(), AmmunitionMax));
                b.tab(8);
                b.Add(UI.icons().s.clock);
                b.Add(GFORMAT.f(b.text(), AmmunitionReplenishHours));
                b.text(DicTime.¤¤Hours);
            }
            b.NL(8);
        }

        private void Hover(GUI_BOX box, double refValue, double ammo)
        {
            GBox b = (GBox)box;
            b.sep();
            Projectile.Hover(box, resource.name, refValue, 0);
            b.NL(8);

            b.sep();

            if (AmmunitionReplenishHours > 0)
            {
                b.textLL(¤¤ammoC);
                b.tab(7);
                b.Add(GFORMAT.fofkInv(b.text(), ammo, AmmunitionMax));
                b.NL();

                b.textL(¤¤ammoR);
                b.tab(7);
                b.Add(GFORMAT.f(b.text(), AmmunitionReplenishHours));
            }
            else
            {
                b.textLL(¤¤ammoU);
            }
        }

        public override void Hover(GUI_BOX box, Div div)
        {
            base.Hover(box, div);
            Hover(box, Ref(div), AmmunitionPerMan(div));
            box.NL();
            Boostable.HoverDetailed(box, div, "", true);
        }

        public override void Hover(GUI_BOX box, Induvidual i)
        {
            base.Hover(box, i);
            Hover(box, Ref(i), AmmunitionMax);
        }
    }
}