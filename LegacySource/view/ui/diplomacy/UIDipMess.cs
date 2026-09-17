using System;
using System.IO;
using game.faction;
using game.faction.npc;
using init.race.appearence;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.gui.misc;

namespace view.ui.diplomacy
{
    [Serializable]
    public class UIDipMess : MessageSection
    {
        private static readonly long serialVersionUID = 1L;
        private readonly string mess;
        private readonly string desc;
        private readonly MessIntro intro;

        public UIDipMess(CharSequence title, CharSequence message, CharSequence desc, FactionNPC f) : base(title)
        {
            this.desc = desc.ToString();
            this.mess = message.ToString();
            intro = new MessIntro(f);
        }

        protected override void Make(GuiSection section)
        {
            Paragraph(mess);
            section.AddRelBody(8, DIR.N, intro.Make());

            section.AddRelBody(8, DIR.S, new GText(UI.FONT().M, desc).LabilifySub().SetMaxWidth(WIDTH));
        }

        [Serializable]
        private class MessFaction
        {
            private static readonly long serialVersionUID = 1L;
            private readonly int fi;
            private readonly int fii;

            public MessFaction(FactionNPC f)
            {
                this.fi = f.Index();
                this.fii = f.Iteration();
            }

            public FactionNPC Faction()
            {
                Faction f = FACTIONS.GetByIndex(fi);
                if (f == null || !f.IsActive() || !(f is FactionNPC))
                    return null;
                FactionNPC npc = (FactionNPC)f;
                if (npc.Iteration() != fii)
                    return null;
                return npc;
            }
        }

        [Serializable]
        private class MessIntro : MessFaction
        {
            private static readonly long serialVersionUID = 1L;
            private readonly Induvidual iking;
            private readonly string sKingName;
            private readonly string sRealmIntro;

            public MessIntro(FactionNPC f) : base(f)
            {
                Induvidual k = f.Court().King().Roy().Induvidual;
                iking = new Induvidual(k.HType(), k.Race());
                iking.CopyFrom(k);
                STATS.NEEDS().Clear(iking);

                sKingName = f.Court().King().Name.ToString();
                sRealmIntro = f.NameIntro.ToString() + " " + f.Name.ToString();
            }

            public GuiSection Make()
            {
                GuiSection r = new GuiSection();
                if (Faction() != null)
                    r.Add(Faction().Banner().HUGE, 0, 0);

                SPRITE pp = new SPRITE.Imp(RPortrait.P_WIDTH * 2, RPortrait.P_HEIGHT * 2)
                {
                    public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        STATS.APPEARANCE().PortraitRender(r, iking, X1, Y1, 2);
                        if (iking.Race().Appearance().Crown.Crowns().Size > 0)
                            iking.Race().Appearance().Crown.Crowns().Get(0).RenderScaled(r, X1, Y1 + 16, 2);
                    }
                };

                r.AddRelBody(32, DIR.E, pp);
                if (Faction() != null)
                    r.AddRelBody(32, DIR.E, Faction().Banner().HUGE);

                r.AddRelBody(4, DIR.S, new GText(UI.FONT().H2, sKingName).Labilify());
                r.AddRelBody(4, DIR.S, new GText(UI.FONT().M, sRealmIntro).Normalify());

                return r;
            }
        }
    }
}