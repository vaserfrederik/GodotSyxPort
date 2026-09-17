using System;
using System.Collections.Generic;
using System.Text;
using Game;
using Game.Faction;
using Game.Faction.NPC;
using Game.Faction.Royalty.Opinion;
using Game.Raiding.MessDemand;
using Snake2D.Util.DataTypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Rnd;
using Snake2D.Util.Sprite.Text;
using Util.Gui.Misc;
using Util.Text;
using View.UI.Diplomacy;
using View.UI.Message;
using World.Region;

namespace Game.Raiding
{
    class MessAlly
    {
        private static readonly string ¤¤title = "Help!";
        private static readonly string ¤¤desc = "If we don't pay this ransom, chances are neighbour allows for free passage through their lands, or gets attacked themselves.";
        private static readonly string ¤¤destroy = "Raider destroys!";

        private static readonly string ¤¤ftitle = "Raider Chased off";
        private static readonly string ¤¤fdesc = "Due to your failure to pay off the raider, this faction is upset with you.";

        private static readonly string ¤¤fwtitle = "Raider On their way";
        private static readonly string ¤¤fwdesc = "Since we didn't pay our neighbour, they have now allowed for free passage for {0} into our lands. Muster the men!";

        private static readonly string ¤¤requestHelp = "Request Assistance";
        private static readonly string ¤¤requestHelpD = "Request that the faction attack this bandit. The consequences of this are hard to foresee.";

        static MessAlly()
        {
            D.ts(typeof(MessAlly));
        }

        private MessAlly()
        {
        }

        static void Help(Raider r, FactionNPC f)
        {
            Str.TMP.Clear();
            Str.TMP.Add(f.Race().Info.RaiderMess.AllyHelp.Rnd());
            RaiderText.Insert.Set(Str.TMP, r);

            new Help(Str.TMP, f, new Demand(r)).Send();
        }

        static void Fight(Raider r, FactionNPC f)
        {
            if (r.Army.Power * (1 + RND.rFloat() * 2) > f.OffensivePower() + RD.MILITARY().Power.GetD(f.CapitolRegion()))
            {
                FACTIONS.Remove((FactionNPC)f, true);
                new MessDestroy(r).Send();
            }
            else
            {
                Str.TMP.Clear();
                Str.TMP.Add(f.Race().Info.RaiderMess.AllyFight.Rnd());
                RaiderText.Insert.Set(Str.TMP, r);
                ROPINION.GIFTS().MakeDeal(f, -1);
                new Fight(Str.TMP, f).Send();
            }
        }

        static void LetThrough(Raider r, FactionNPC f)
        {
            Str.TMP.Clear();
            Str.TMP.Add(¤¤fwdesc);
            Str.TMP.Insert(0, r.Name);

            new MessageText(¤¤fwtitle, Str.TMP).Send();
        }

        private class Help : UIDipMess
        {
            private readonly Demand demand;
            private bool requested = false;

            private Help(CharSequence message, FactionNPC f, Demand demand) : base(¤¤title, message, ¤¤desc, f)
            {
                this.demand = demand;
            }

            protected override void Make(GuiSection section)
            {
                base.Make(section);
                section.AddRelBody(8, DIR.S, demand.Section(true));
                section.AddRelBody(8, DIR.S, new GButt.ButtPanel(¤¤requestHelp)
                {
                    protected override void RenAction()
                    {
                        SelectedSet(requested);
                        ActiveSet(demand.CanRespond());
                    }

                    protected override void ClickA()
                    {
                        if (!requested && demand.CanRespond())
                        {
                            requested = true;
                            GAME.Raiders().Current.SetAllyFight();
                        }
                    }
                }.HoverInfoSet(¤¤requestHelpD));
            }
        }

        private class Fight : UIDipMess
        {
            private Fight(CharSequence message, FactionNPC f) : base(¤¤ftitle, message, ¤¤fdesc, f)
            {
            }
        }

        public class MessDestroy : MessageSection
        {
            private readonly Raider raider;

            public MessDestroy(Raider raider) : base(¤¤destroy)
            {
                this.raider = raider;
            }

            protected override void Make(GuiSection section)
            {
                Str.TMP.Clear();
                Str.TMP.Add(raider.Indu.Race().Info.RaiderMess.AllyDead.Rnd());
                RaiderText.Insert.Set(Str.TMP, raider);

                Paragraph(Str.TMP);

                section.AddRelBody(32, DIR.N, new RaiderPortrait(4).Set(raider));
            }
        }
    }
}