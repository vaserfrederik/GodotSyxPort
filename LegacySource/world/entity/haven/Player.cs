using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace World.Entity.Haven
{
    class Player : ISaveable
    {
        private static string ¤¤titleNew = "Haven controlled";
        private static string ¤¤bodyNew = "We now have under our control a haven. Havens are bastions containing powerful races that you can sway to your cause if you fulfill their requirements.";

        private static string ¤¤titleMore = "More {0} join you";
        private static string ¤¤bodyMore = "Since your worth has increased in the eyes of your {0}. More are willing to join your cause. Make sure you accept them as immigrants.";

        private static string ¤¤titleLess = "{0} are leaving!";
        private static string ¤¤bodyLess = "Since you've failed to uphold the standards of your {0} havens, many have now stopped supporting you, and all its members will start to return home.";

        static Player()
        {
            D.ts(typeof(Player));
        }

        private double[] oldValue;
        private bool hasAny;
        private double upT;

        public Player(WHavens havens)
        {
            oldValue = new double[havens.types.Count];
        }

        public void Save(FilePutter file)
        {
            file.dsE(oldValue);
            file.d(upT);
            file.bool(hasAny);
        }

        public void Load(FileGetter file)
        {
            file.dsE(out oldValue);
            upT = file.d();
            hasAny = file.bool();
        }

        public void Clear()
        {
            oldValue.Fill(0);
            upT = 0;
            hasAny = false;
        }

        public void Update(double ds)
        {
            upT += ds * 0.1;
            if (upT < 10)
                return;
            upT -= 10;

            foreach (WHavenType t in WORLD.ENTITIES().havens.types)
            {
                if (Update(t))
                    return;
            }
        }

        private bool Update(WHavenType t)
        {
            if (WORLD.ENTITIES().havens.camps(FACTIONS.player(), t) == 0)
                return false;

            if (!hasAny && WORLD.ENTITIES().havens.camps(FACTIONS.player(), t) > 0)
            {
                hasAny = true;
                new MessOwn().Send();
                return true;
            }

            double old = oldValue[t.index()];

            double nn = t.amount();

            if (nn <= 0 && old > 0)
            {
                if (t.reqsFrom.progress(FACTIONS.player()) < 0.75)
                {
                    oldValue[t.index()] = 0;
                    new MessageText(new Str(¤¤titleLess).Insert(0, t.race.info.names), new Str(¤¤bodyLess).Insert(0, t.race.info.names)).Send();
                    return true;
                }
                return false;
            }

            int iold = (int)Math.Round(old * 5);
            int inew = (int)Math.Round(nn * 5);

            if (inew > iold)
            {
                SETT.ENTRY().immi().SetHigher(t.race, (int)(WORLD.camps().max(FACTIONS.player(), t) * nn));
                if (iold == 0)
                {
                    new MessChange(t, new Str(¤¤titleMore).Insert(0, t.race.info.names), t.sJoin).Send();
                }
                else
                    new MessChange(t, new Str(¤¤titleMore).Insert(0, t.race.info.names), new Str(¤¤bodyMore).Insert(0, t.race.info.names)).Send();
                oldValue[t.index()] = nn;
                return true;
            }
            else if (inew < iold - 1)
            {
                new MessChange(t, new Str(¤¤titleLess).Insert(0, t.race.info.names), t.sLeave).Send();
                oldValue[t.index()] = nn;
                return true;
            }

            return false;
        }

        public double Get(Faction f, WHavenType t)
        {
            if (f == FACTIONS.player())
                return oldValue[t.index()];
            return 1.0;
        }

        private class MessOwn : MessageSection
        {
            private static readonly long serialVersionUID = 1L;
            private readonly int tx;
            private readonly int ty;

            public MessOwn()
                : base(¤¤titleNew)
            {
                WHaven f = First();
                this.tx = f.ctx();
                this.ty = f.cty();
            }

            private WHaven First()
            {
                foreach (WEntity e in WORLD.ENTITIES().allSlow())
                {
                    if (e.faction() == FACTIONS.player() && e is WHaven)
                    {
                        return (WHaven)e;
                    }
                }
                return null;
            }

            protected override void Make(GuiSection section)
            {
                paragraph(¤¤bodyNew);

                section.addRelBody(16, DIR.S, new GButt.ButtPanel(UI.icons().m.crossair)
                {
                    protected override void ClickA()
                    {
                        VIEW.world().activate();
                        VIEW.world().window.setZoomout(0);
                        VIEW.world().window.centererTile.set(tx, ty);
                    }
                });
            }
        }

        private class MessChange : MessageSection
        {
            private static readonly long serialVersionUID = 1L;
            private readonly int ti;
            private readonly string desc;

            public MessChange(WHavenType t, string title, string desc)
                : base(title)
            {
                ti = t.index();
                this.desc = desc;
            }

            protected override void Make(GuiSection section)
            {
                paragraph(desc);
                section.addRelBody(16, DIR.N, new SPRITE.Imp(Icon.HUGE)
                {
                    public void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        WORLD.camps().types.getC(ti).race.appearance().iconBig.render(r, X1, X2, Y1, Y2);
                    }
                });
            }
        }
    }
}