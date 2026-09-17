using System;
using System.Collections.Generic;
using settlement.main;
using game.audio;
using init.constant;
using init.race;
using init.sprite;
using init.type;
using settlement.entity;
using settlement.entity.humanoid.ai.main;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.gui.misc;
using view.sett;
using view.tool;

namespace settlement.entity.humanoid
{
    public sealed class Humanoids : SettResource
    {
        static Humanoids()
        {
            AI.Init();
        }

        public readonly SoundRace sound = AUDIO.Race("BODY_EXPLODE");

        public readonly DRAGGABLE_HOLDER draggable = new DRAGGABLE_HOLDER()
        {
            Draggable = index =>
            {
                ENTITY e = SETT.ENTITIES().GetByID(index);
                if (e != null && e is Humanoid)
                {
                    Humanoid a = (Humanoid)e;
                    return a;
                }

                return null;
            }
        };

        public Humanoids() : base("SETT_HUMANOIDS", false)
        {
            LinkedList<PLACABLE> all = new LinkedList<PLACABLE>();
            HTYPE[] tt = new HTYPE[]
            {
                HTYPES.SUBJECT(), HTYPES.PRISONER(), HTYPES.SLAVE(), HTYPES.CHILD(), HTYPES.DERANGED(), HTYPES.ENEMY(), HTYPES.RIOTER()
            };

            foreach (HTYPE t in tt)
            {
                foreach (Race r in RACES.All())
                {
                    Placer p = new Placer(r, t);
                    all.Add(p);
                }
            }

            PlacableSimple death = new PlacableSimple("kill", "")
            {
                Cause = CAUSE_LEAVES.AGE(),
                Butts = new ArrayList<CLICKABLE>(CAUSE_LEAVES.ALL().Count),

                {
                    foreach (CAUSE_LEAVE l in CAUSE_LEAVES.ALL())
                    {
                        Butts.Add(new Panel(SPRITES.Icons().Dot)
                        {
                            ClickA = () => Cause = l,
                            RenAction = () => SelectedSet(Cause == l)
                        }.HoverInfoSet(l.Desc));
                    }
                }
            };

            death.GetUndo = () => null;
            death.GetIcon = () => SPRITES.Icons().M.Cancel;
            death.Place = (x, y) =>
            {
                foreach (ENTITY e in ENTITIES().GetAtPointL(x, y))
                {
                    if (e is Humanoid)
                    {
                        ((Humanoid)e).Kill(false, Cause);
                        return;
                    }
                }
            };

            death.IsPlacable = (x, y) =>
            {
                foreach (ENTITY e in ENTITIES().GetAtPointL(x, y))
                {
                    if (e is Humanoid)
                    {
                        return null;
                    }
                }
                return E;
            };

            death.GetAdditionalButt = () => Butts;

            all.Add(death);

            PlacableSimple attack = new PlacableSimple("attack", "")
            {
                Cause = CAUSE_LEAVES.SLAYED(),
                Coll = new ECollision()
            };

            attack.GetUndo = () => null;
            attack.GetIcon = () => SPRITES.Icons().M.Cancel;
            attack.Place = (x, y) =>
            {
                foreach (ENTITY e in ENTITIES().GetAtPointL(x, y))
                {
                    if (e is Humanoid)
                    {
                        Kill((Humanoid)e, x, y);
                        return;
                    }
                }
            };

            attack.Kill = (e, x, y) =>
            {
                VectorImp vec = new VectorImp();
                double m = vec.Set(x, y, e.Body().CX(), e.Body().CY());

                e.Speed.SetRaw(vec.NX() * m * C.TILE_SIZEH + e.Speed.X(), vec.NY() * m * C.TILE_SIZEH + e.Speed.Y());

                Coll.DamageTileStrength = 0;
                Coll.TileMomentum = 0;
                for (int i = 0; i < Coll.Damage.Length; i++)
                    Coll.Damage[i] = 0;
                Coll.DirDot = 1;
                Coll.DirDotOther = 1;
                Coll.NorX = 0.5;
                Coll.NorY = 0.5;
                Coll.SpeedHasChanged = true;
                Coll.Other = null;
                e.Collide(Coll);
                if (!e.IsRemoved())
                    e.Kill(true, Cause);
            };

            attack.IsPlacable = (x, y) =>
            {
                foreach (ENTITY e in ENTITIES().GetAtPointL(x, y))
                {
                    if (e is Humanoid)
                    {
                        return null;
                    }
                }
                return E;
            };

            all.Add(attack);

            PlacableSimple explode = new PlacableSimple("explode", "")
            {
                Cause = CAUSE_LEAVES.SLAYED()
            };

            explode.GetUndo = () => null;
            explode.GetIcon = () => SPRITES.Icons().M.Cancel;
            explode.Place = (x, y) =>
            {
                foreach (ENTITY e in ENTITIES().GetAtPointL(x, y))
                {
                    if (e is Humanoid)
                    {
                        ((Humanoid)e).InflictDamage(10, Cause);
                        return;
                    }
                }
            };

            explode.IsPlacable = (x, y) =>
            {
                foreach (ENTITY e in ENTITIES().GetAtPointL(x, y))
                {
                    if (e is Humanoid)
                    {
                        return null;
                    }
                }
                return E;
            };

            all.Add(explode);

            IDebugPanelSett.Add("humanoids", new ArrayList<PLACABLE>(all));
        }

        public Humanoid Create(Race r, int tx, int ty, HTYPE t, CAUSE_ARRIVE cause)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                throw new RuntimeException(tx + " " + ty);
            int x = tx * C.TILE_SIZE + C.TILE_SIZEH;
            int y = ty * C.TILE_SIZE + C.TILE_SIZEH;
            return new Humanoid(x, y, r, t, cause);
        }

        private class Placer : PlacableSimple
        {
            private readonly Race r;
            private readonly HTYPE f;

            public Placer(Race r, HTYPE f) : base(r.Info.Name + " " + f.Name, "")
            {
                this.r = r;
                this.f = f;
            }

            public override SPRITE GetIcon() => r.Appearance().Icon;

            public override void Place(int x, int y)
            {
                if (IsPlacable(x, y) == null)
                {
                    Humanoid a = new Humanoid(x, y, r, f, CAUSE_ARRIVES.IMMIGRATED());
                    if (a != null && f == HTYPES.PRISONER())
                        STATS.LAW().PrisonerType.Set(a.Indu(), CRIMES.All(a.Indu().Clas()).Rnd());
                    return;
                }
            }

            public override CharSequence IsPlacable(int x, int y)
            {
                int x1 = (x - r.Physics.HitBoxsize() / 2) / C.TILE_SIZE;
                int x2 = (x + r.Physics.HitBoxsize() / 2) / C.TILE_SIZE;
                int y1 = (y - r.Physics.HitBoxsize() / 2) / C.TILE_SIZE;
                int y2 = (y + r.Physics.HitBoxsize() / 2) / C.TILE_SIZE;
                if (!IN_BOUNDS(x1, y1) || !IN_BOUNDS(x2, y2))
                    return E;
                return !PATH().Solidity.Is(x1, y1) && !PATH().Solidity.Is(x2, y1) &&
                       !PATH().Solidity.Is(x1, y2) && !PATH().Solidity.Is(x2, y2) &&
                       ENTITIES().GetAtPoint(x, y) == null ? null : E;
            }

            public override PLACABLE GetUndo() => null;
        }
    }
}