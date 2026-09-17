using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game;
using game.battle.util;
using game.debug;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.time;
using init.constant;
using init.paths;
using init.race;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.military.artillery;
using settlement.stats.colls;
using settlement.stats.equip;
using settlement.stats.standing;
using settlement.thing.projectiles;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;
using view.sett;
using world.entity.army;

namespace settlement.battle.invasion
{
    public sealed class Invador : SettResource
    {
        private List<Invasion> active = new List<Invasion>(16);
        private Projectile proj;

        private static readonly CharSequence ¤¤inv = "¤Pending Invasion";
        static
        {
            D.ts(typeof(Invador));
        }

        public Invador() : base("INVADOR", false)
        {
            IDebugPanelSett.add("Invade Small", new ACTION
            {
                exe = () => invade(20, 0.2)
            });

            IDebugPanelSett.add("Invade Medium", new ACTION
            {
                exe = () => invade(200, 0.2)
            });

            IDebugPanelSett.add("Invade Huge", new ACTION
            {
                exe = () => invade(2000, 0.2)
            });

            proj = new Projectile.ProjectileImp(new Json(PATHS.CONFIG().init.gets("DefaultProjectile")), "_DefaultProjectile");

            IDebugPanelSett.add("Invasion Finish", new ACTION
            {
                exe = () =>
                {
                    if (active.Count > 0)
                    {
                        active[0].fastForward();
                    }
                }
            });

            new DIP.DipActivityListener
            {
                change = (Faction faction, Faction other, DipStance old, DipStance nn) =>
                {
                    if (old == DIP.WAR() && nn != DIP.WAR())
                    {
                        FactionNPC o = null;
                        if (faction == FACTIONS.player())
                        {
                            o = (FactionNPC)other;
                        }
                        else
                        {
                            o = (FactionNPC)faction;
                        }
                        for (int i = 0; i < active.Count; i++)
                        {
                            if (active[i].spec.fi == o.index())
                            {
                                cancel(active[i].spec.ref);
                            }
                        }
                    }
                }
            };
        }

        private void invade(int amount, double quality)
        {
            if (active.Count == 0)
            {
                GAME.ARMIES().factors.init(GAME.ARMIES().enemy(), 1.0);
            }

            if (!active.HasRoom())
                return;

            int menPerDivision = amount / Config.battle().DIVISIONS_PER_ARMY;
            if (menPerDivision < 50)
                menPerDivision = 50;
            if (amount < menPerDivision)
                menPerDivision = amount;

            int divisions = amount / menPerDivision;

            InvasionSpec sp = new InvasionSpec();
            sp.wx = RND.rInt(1000);
            sp.wy = RND.rInt(1000);

            Race race = FACTIONS.player().race();
            DIV_SETTINGImp spec = new DIV_SETTINGImp();
            for (int i = 0; i < divisions; i++)
            {
                spec.copySettings(GAME.battle().types.rnd(race, FACTIONS.player(), RND.rFloat()), menPerDivision, Math.Pow(RND.rFloat(), 1.5), Math.Pow(RND.rFloat(), 1.5));
                double experience = quality / 2;
                sp.add(make(menPerDivision, race, spec, experience));
            }

            active.Add(new Invasion(sp));
        }

        private DivGeneration make(int men, Race race, DIV_SETTINGImp spec, double ex)
        {
            var name = race.info.armyNames.rnd();
            var bannerI = RND.rInt(GAME.ARMIES().banners.size());
            DIV_SPEC dd = new DIV_SPEC
            {
                training = tr => spec.training(tr),
                equip = e => spec.equip(e),
                race = () => race,
                men = () => men,
                faction = () => null,
                experience = () => ex,
                name = () => name,
                bannerI = () => bannerI
            };
            return new DivGeneration(dd, dd);
        }

        private int ref = 1;

        public int invade(InvasionSpec spec, WArmy a)
        {
            if (spec.divs.Count <= 0)
            {
                GAME.Notify("nope");
                return -1;
            }

            spec.ref = ref;
            ref++;

            active.Add(new Invasion(spec));

            foreach (var l in InvasionListener.all)
            {
                l.register(a, ref);
            }

            return spec.ref;
        }

        protected override void update(double ds, Profiler profiler)
        {
            if (active.Count == 0)
                return;

            Invasion in = active[0];

            STANDINGS.emergency(HCLASSES.CITIZEN(), TIME.secondsPerDay() * 6);

            if (in.update(ds))
                return;

            active.RemoveOrdered(0);

            foreach (var e in SETT.ENTITIES().getAllEnts())
            {
                if (e is Humanoid)
                {
                    var h = (Humanoid)e;
                    if (h.indu().hType() == HTYPES.ENEMY())
                    {
                        h.helloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
                    }
                }
            }

            foreach (var b in SETT.ROOMS().ARTILLERY)
            {
                for (int i = 0; i < b.instancesSize(); i++)
                {
                    var ins = b.getInstance(i);
                    if (ins.army() == GAME.ARMIES().enemy())
                    {
                        ins.destroyTile(ins.mX(), ins.mY());
                        i--;
                    }
                }
            }
        }

        public bool invading(FactionNPC f)
        {
            foreach (var i in active)
            {
                if (i.invador() == f)
                    return true;
            }
            return false;
        }

        public bool invading()
        {
            return active.Count > 0;
        }

        public double invadingPower()
        {
            double p = 0;
            foreach (var i in active)
            {
                p += i.spec.power;
            }
            return p;
        }

        public bool invadingPending()
        {
            if (active.Count > 0)
            {
                for (int i = 0; i < active.Count; i++)
                    if (active[i].spec.canBeAttacked)
                        return true;
            }
            return false;
        }

        protected override void save(FilePutter file)
        {
            file.i(active.Count);
            foreach (var i in active)
            {
                i.save(file);
            }
            file.i(ref);
        }

        protected override void load(FileGetter file) throws IOException
        {
            active.Clear();
            int am = file.i();
            for (int i = 0; i < am; i++)
                active.Add(new Invasion(file));
            ref = file.i();
        }

        protected override void clear()
        {
            active.Clear();
        }

        public void hover(GUI_BOX text)
        {
            GBox b = (GBox)text;
            if (active.Count > 0)
            {
                b.title(¤¤inv);
                foreach (var i in active)
                {
                    int men = 0;
                    foreach (var s in i.spec.divs)
                        men += s.indus.Length;
                    b.textLL(Dic.¤¤Soldiers);
                    b.tab(6);
                    b.add(GFORMAT.i(b.text(), men));
                    b.NL();
                    b.textL(i.spot.dir.perpendicular().getName());
                    b.NL(8);
                }
            }
        }

        public InvasionSpec spec(int ref)
        {
            foreach (var i in active)
            {
                if (i.spec.ref == ref)
                    return i.spec;
            }
            return null;
        }

        public void cancel(int ref)
        {
            foreach (var i in active)
            {
                if (i.spec.ref == ref)
                {
                    active.RemoveOrdered(i);
                    return;
                }
            }
        }
    }
}