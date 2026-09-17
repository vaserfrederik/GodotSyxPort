using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Battle.Thread.General.Offence
{
    using Game.Battle.Div;
    using Game.Battle.Formation;
    using Game.Battle.Thread.General;
    using Game.Battle.Thread.Order;
    using Game.Battle.Thread.Trajectory;
    using Init.Constant;
    using Init.Constant.Config;
    using Init.Sprite;
    using Init.Sprite.UI;
    using Snake2D.Renderer;
    using Snake2D.Util.Color;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using Util.Rendering;
    using Util.Rendering.ShadowBatch;

    public class Strategos2000UpdaterOffense : Strategos2000Updater
    {
        private readonly LIST<State> states;
        private readonly StrategosUtil u;
        private int state;
        private readonly Context c;

        public Strategos2000UpdaterOffense(StrategosUtil u)
        {
            this.u = u;
            this.c = new Context();
            states = attack();
        }

        public override void Clear()
        {
            state = 0;
            c.Clear();
        }

        public override void Save(FilePutter file)
        {
            file.I(state);
            c.Save(file);
        }

        public override void Load(FileGetter file)
        {
            state = file.I();
            c.Load(file);
        }

        public override bool Update()
        {
            if (state >= states.Size)
            {
                state = 0;
                return false;
            }

            int s = state;
            long millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (!states.Get(state).Is())
            {
                state++;
            }

            millis = DateTimeOffset.Now.ToUnixTimeMilliseconds() - millis;
            //LOG.Ln(states.Get(s).Name + " " + millis);
            return true;
        }

        public override void Render(Renderer r, RenderIterator it)
        {
            if (c.blob.Is(it.Tile()))
            {
                COLOR.ORANGE100.Bind();
                SPRITES.Cons().BIG.Outline.Render(r, 0, it.X(), it.Y());
            }
            if (c.block.Is(it.Tile()))
            {
                COLOR.WHITE50.Bind();
                UI.Icons().S.Cancel.RenderScaled(r, it.X(), it.Y(), C.SCALE);
            }

            COLOR.Unbind();
            //context.preLines.Render(r, it);
        }

        public override void Render(Renderer r, ShadowBatch shadowBatch, RenderData data)
        {
            for (int li = 0; li < c.Lines.Lines(); li++)
            {
                Line l = c.Lines.Get(li);
                int ox = data.OffX1();
                int oy = data.OffY1();

                for (int s = 0; s <= l.Length; s += C.TILE_SIZEH)
                {
                    int cx = (int)(l.Sx + l.Dx * s - ox);
                    int cy = (int)(l.Sy + l.Dy * s - oy);

                    UI.Icons().S.Dot.RenderC(r, cx, cy);
                }
            }
        }

        private LIST<State> attack()
        {
            ArrayListGrower<State> states = new ArrayListGrower<State>();

            states.Add(new State("clear")
            {
                private readonly BattleOrderTask task = new BattleOrderTask();

                public override bool Is()
                {
                    c.deployedToLine.Clear();

                    for (int di = 0; di < Config.Battle().DIVISIONS_PER_ARMY; di++)
                    {
                        Div d = u.GetArmy().Divisions.Get(di);
                        if (d.Active() && d.Settings().Ammo() != null && BattleTrajectories.Trajectories(d) > d.Men() / 2)
                        {
                            d.Settings().FireAtWill = true;
                            d.Settings().Formation = DIV_FORMATION.LOOSE;
                            task.Stop(d);
                            d.Order().Task.Set(task);
                            c.deployedToLine.Set(di, true);
                        }
                    }

                    return false;
                }
            });

            states.Add(new State("bombard")
            {
                private readonly StepArtilleryBombard b = new StepArtilleryBombard(u);

                public override bool Is()
                {
                    b.Bombard();
                    return false;
                }
            });

            states.Add(new State("blob")
            {
                private readonly StepBlob s = new StepBlob(u);

                public override bool Is()
                {
                    s.Update(c.blob, 24);
                    return false;
                }
            });

            states.Add(new State("throne")
            {
                public override bool Is()
                {
                    //t.setToThrone();
                    return false;
                }
            });

            states.Add(new State("make lines")
            {
                private readonly StepLinesMaker s = new StepLinesMaker(u, c);

                public override bool Is()
                {
                    s.Make();
                    return false;
                }
            });

            StepLinesChecker check = new StepLinesChecker(u, c);
            states.Add(new State("checkLines init")
            {
                public override bool Is()
                {
                    check.Init();
                    return false;
                }
            });

            states.Add(new State("checkLines")
            {
                public override bool Is()
                {
                    return check.Check();
                }
            });

            states.Add(new State("blockLines")
            {
                private readonly StepLinesBlocker s = new StepLinesBlocker(u, c);

                public override bool Is()
                {
                    s.Make();
                    return false;
                }
            });

            StepLinesBacker back = new StepLinesBacker(u, c);
            states.Add(new State("back lines")
            {
                public override bool Is()
                {
                    back.Init();
                    return false;
                }
            });

            states.Add(new State("back lines 2")
            {
                public override bool Is()
                {
                    return back.RetreatThroneLine();
                }
            });

            StepLinesMoveTo line = new StepLinesMoveTo(u, c);
            states.Add(new State("line move init")
            {
                public override bool Is()
                {
                    line.Init();
                    return false;
                }
            });

            states.Add(new State("line move 1")
            {
                public override bool Is()
                {
                    return line.DeployDivsToLine();
                }
            });

            states.Add(new State("line move 2")
            {
                public override bool Is()
                {
                    return line.DeployDivsToLineRanged();
                }
            });

            states.Add(new State("line move3")
            {
                public override bool Is()
                {
                    line.SetSpeedAndFormation();
                    return false;
                }
            });

            StepLineCharge charge = new StepLineCharge(u, c);
            states.Add(new State("charge 1")
            {
                public override bool Is()
                {
                    charge.Init();
                    return false;
                }
            });

            states.Add(new State("charge 2")
            {
                public override bool Is()
                {
                    return charge.Charge();
                }
            });

            StepMoveToThrone throne = new StepMoveToThrone(u, c);
            states.Add(new State("throne init")
            {
                public override bool Is()
                {
                    throne.Init();
                    return false;
                }
            });

            states.Add(new State("throne")
            {
                public override bool Is()
                {
                    return throne.setToThrone();
                }
            });

            states.Add(new State("attack")
            {
                private readonly StepAttackEnemyNear s = new StepAttackEnemyNear(u, c);

                public override bool Is()
                {
                    return s.AttackEnemies();
                }
            });

            StepAttackOthers kite = new StepAttackOthers(u, c);
            states.Add(new State("kite init")
            {
                public override bool Is()
                {
                    kite.Init();
                    return false;
                }
            });

            states.Add(new State("kite")
            {
                public override bool Is()
                {
                    return kite.Attack();
                }
            });

            return states;
        }

        private abstract class State
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1822:Member should be static", Justification = "Required for base class structure")]
            private readonly string name;

            protected State(string name)
            {
                this.name = name;
            }

            public abstract bool Is();
        }
    }
}