using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.service;
using settlement.main;
using settlement.misc.util;
using settlement.room.service.lavatory;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace Settlement.Entity.Humanoid.AI.Service
{
    class M_PlanLavatory : MPlan<ROOM_LAVATORY>
    {
        public M_PlanLavatory() : base("Lav", SETT.ROOMS().LAVATORIES, true)
        {
        }

        protected override AISubActivation Arrive(Humanoid a, AIManager d)
        {
            return takingDump.Set(a, d);
        }

        private readonly Resumer takingDump = new Resumer("PlanDischarging")
        {
            private readonly AISUB sub = new AISUB.Simple("taking a dump")
            {
                protected override AISTATE Resume(Humanoid a, AIManager d)
                {
                    d.subByte++;

                    if (d.subByte > 4 + RND.rInt(5))
                        return null;

                    if (Blue(d).Service().UsageSound != null && RND.OneIn(2))
                        Blue(d).Service().UsageSound.Rnd(a);

                    return AI.STATES().STAND.Activate(a, d, 5f);
                }
            };

            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                Lavatory l = Get(a, d);
                a.Speed.SetDirCurrent(l.GetDir());
                return sub.Activate(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                Get(a, d).Consume();
                return walk2Water.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                FINDABLE s = Get(a, d);
                return s != null && s.FindableReservedIs();
            }

            public override void Can(Humanoid a, AIManager d)
            {
                FINDABLE s = Get(a, d);
                if (s != null)
                    s.FindableReserveCancel();
            }
        };

        private readonly Resumer walk2Water = new Resumer("Washing up")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                LavatoryInstance b = Blue(d).Get(a.Physics.TileC().X, a.Physics.TileC().Y);
                if (b != null)
                {
                    COORDINATE c = b.GetExtra();

                    if (c != null)
                    {
                        AISubActivation s = AI.SUBS().WalkTo.Coo(a, d, c);
                        if (s == null)
                        {
                            Can(a, d);
                            return null;
                        }
                        return s;
                    }
                }
                return null;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                return washing.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return Blue(d).IsExtra(d.Path.DestX, d.Path.DestY);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                if (Blue(d).IsExtra(d.Path.DestX, d.Path.DestY))
                {
                    LavatoryInstance b = Blue(d).Get(d.Path.DestX, d.Path.DestY);
                    b.ReturnExtra(d.Path.DestX, d.Path.DestY);
                }
            }
        };

        private readonly Resumer washing = new Resumer("Washing up")
        {
            private readonly AISUB sub = new AISUB.Simple("washing")
            {
                protected override AISTATE Resume(Humanoid a, AIManager d)
                {
                    d.subByte++;

                    if (d.subByte > 1)
                        return null;

                    return AI.STATES().Anima.Box.Activate(a, d, 15f);
                }
            };

            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return sub.Activate(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                Can(a, d);
                return null;
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return Blue(d).IsExtra(d.Path.DestX, d.Path.DestY);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                if (Blue(d).IsExtra(d.Path.DestX, d.Path.DestY))
                {
                    LavatoryInstance b = Blue(d).Get(d.Path.DestX, d.Path.DestY);
                    b.ReturnExtra(d.Path.DestX, d.Path.DestY);
                }
            }
        };

        protected override Lavatory Get(Humanoid a, AIManager d)
        {
            return Blue(d).GetService(d.PlanTile.X, d.PlanTile.Y);
        }
    }
}