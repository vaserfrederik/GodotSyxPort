using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.Faction.Diplomacy
{
    public class UpVassal : ISavable
    {
        private static readonly string ¤¤tribute = "¤Tribute";
        private static readonly string ¤¤vassal = "¤It is time for us to pay our annual tribute to our overlord and protector. We must pay 10% of our net worth.";
        private static readonly string ¤¤overlord = "¤Tribute from our loyal subjects has arrived. As always, they are expressing eternal gratitude for your protection. Should we want to show generosity, we can always decline this offer.";

        static UpVassal()
        {
            D.ts(typeof(UpVassal));
        }

        private int year = -1;

        public void Save(FilePutter file)
        {
            file.WriteInt(year);
        }

        public void Load(FileGetter file)
        {
            year = file.ReadInt();
        }

        public void Clear()
        {
            year = -1;
        }

        public void Update()
        {
            if (year == TIME.Years.BitsSinceStart)
                return;
            year = TIME.Years.BitsSinceStart;

            if (DIP.Overlord(FACTIONS.Player) != null)
            {
                new MVassal(DIP.Overlord(FACTIONS.Player)).Send();
            }

            if (DIP.VASSAL.All(FACTIONS.Player).Count > 0)
            {
                new MOverlord(DIP.VASSAL.All(FACTIONS.Player)).Send();
            }
        }

        private class MVassal : MessageSection
        {
            private readonly int credits;
            private readonly int fi;

            public MVassal(Faction ff) : base(¤¤tribute)
            {
                fi = ff.Index;

                int credits = (int)FACTIONS.WORTH.Faction;
                credits *= 0.1;
                credits = Math.Max(credits, FACTIONS.Player.Credits.Worth[0] - FACTIONS.Player.Credits.Worth[1]);
                if (credits < 0)
                    credits = 100;

                int am = (int)FACTIONS.Player.Credits.GetD;
                am = CLAMP.I(am, 0, credits);
                if (am < credits)
                {
                    am += (credits - am) / 4;
                }

                this.credits = am;

                FACTIONS.Player.Credits.Inc(-this.credits, CTYPE.TRIBUTE);
            }

            protected override void Make(GuiSection section)
            {
                Paragraph(¤¤vassal);

                section.AddRelBody(16, DIR.S, new GStat()
                {
                    Update = text => GFORMAT.I(text, -credits)
                }.Hh(UI.Icons.M.Coins));

                Faction f = FACTIONS.GetByIndex(fi);
                if (f != null)
                {
                    section.AddRelBody(16, DIR.N, f.Banner.HUGE);
                }
            }
        }

        private class MOverlord : MessageSection
        {
            private readonly int[] creds;
            private readonly int[] fi;
            private readonly int[] fii;
            private readonly bool[] aa;
            private bool accepted = false;
            private readonly int day = TIME.Days.BitsSinceStart;

            public MOverlord(IList<Faction> all) : base(¤¤tribute)
            {
                fi = new int[all.Count];
                fii = new int[all.Count];
                creds = new int[all.Count];
                aa = new bool[all.Count];
                aa.Fill(true);

                for (int i = 0; i < all.Count; i++)
                {
                    FactionNPC f = (FactionNPC)all[i];
                    fi[i] = f.Index;
                    fii[i] = f.Iteration;
                    DIP.TMP.SetFactionAndClear(f);
                    creds[i] = (int)FWorth.Vassal(f);
                }
            }

            protected override void Make(GuiSection section)
            {
                Paragraph(¤¤overlord);

                LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

                for (int k = 0; k < creds.Length; k++)
                {
                    int i = k;
                    GuiSection row = new GuiSection()
                    {
                        HoverInfoGet = text =>
                        {
                            base.HoverInfoGet(text);
                            if (!text.EmptyIs())
                                return;

                            FactionNPC f = F(i);
                            if (f != null)
                            {
                                VIEW.World.UI.Factions.Hover(text, f);
                            }
                        }
                    };

                    SPRITE s = new SPRITE.Imp(Icon.L)
                    {
                        Render = (r, X1, X2, Y1, Y2) =>
                        {
                            FactionNPC f = F(i);
                            if (f != null)
                            {
                                f.Banner.BIG.Render(r, X1, Y1);
                            }
                        }
                    };

                    row.Add(s, 0, 0);

                    row.AddRightC(8, new GStat()
                    {
                        Update = text => GFORMAT.IIncr(text, creds[i])
                    });
                    row.AddRightC(160, new GButt.Checkbox()
                    {
                        ClickA = () => aa[i] = !aa[i],
                        RenAction = () =>
                        {
                            SelectedSet(aa[i]);
                            ActiveSet(!accepted);
                        }
                    }.HoverInfoSet(Dic.¤¤Accept));

                    row.Pad(8, 4);

                    rows.Add(row);
                }

                section.AddRelBody(8, DIR.S, new GScrollRows(rows, rows.First().Body.Height * 8).View());

                section.AddRelBody(16, DIR.S, new GButt.ButtPanel(Dic.¤¤Accept())
                {
                    ClickA = () =>
                    {
                        if (!accepted && TIME.Days.BitsSinceStart - day < 4)
                        {
                            accepted = true;
                            for (int i = 0; i < creds.Length; i++)
                            {
                                if (F(i) != null)
                                {
                                    if (aa[i])
                                    {
                                        FACTIONS.Player.Credits.Inc(creds[i], CTYPE.TRIBUTE);
                                        ROPINION.OTHER.AcceptTribute(F(i), true);
                                    }
                                    else
                                    {
                                        ROPINION.OTHER.AcceptTribute(F(i), false);
                                    }
                                }
                            }
                            VIEW.Messages.Hide();
                        }

                        base.ClickA();
                    },
                    RenAction = () =>
                    {
                        SelectedSet(accepted && Math.Abs(day - TIME.Days.BitsSinceStart) < 4);
                    }
                });
            }

            private FactionNPC F(int i)
            {
                FactionNPC f = (FactionNPC)FACTIONS.GetByIndex(fi[i]);
                if (f != null && f.IsActive() && f.Iteration == fii[i] && DIP.Overlord(f) == FACTIONS.Player)
                {
                    return f;
                }
                return null;
            }
        }
    }
}