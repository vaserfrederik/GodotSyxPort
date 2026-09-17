using System;
using System.Collections.Generic;
using game.event.engine;
using game.faction;
using game.faction.FCredits;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;

namespace game.event.actions
{
    internal sealed class _CREDITS : EventActionConstructor
    {
        private static readonly string ¤¤tooPoor = "You are too poor to pay this sum.";

        static _CREDITS()
        {
            D.ts(typeof(_CREDITS));
        }

        public _CREDITS() : base("CREDITS")
        {
        }

        public override EventAction action(Data data)
        {
            return new Imp(Key, data.Json, data.All);
        }

        public sealed class Imp : EventAction
        {
            private readonly Amount amount;
            private readonly bool negativeAllowed;

            public Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                amount = new Amount(new CInt("AMOUNT"));
                amount.Read(data, -int.MinValue);
                negativeAllowed = data.Bool("NEGATIVE_ALLOWED", true);
                data.CheckUnused();
            }

            public override void SetContext(Event event, EContext data)
            {
                amount.Set(event, data, (int)FACTIONS.player().credits().Credits());
            }

            public override void Exe(Event event, EContext data)
            {
                FACTIONS.player().credits().Inc(amount.Amount.Get(event, data), CTYPE.MISC);
            }

            public override void AddToMessageBody(LISTE<RENDEROBJ> rows, Event e, EContext data, RECTANGLE messBody)
            {
                rows.Add(new GStat
                {
                    Update = text =>
                    {
                        GFORMAT.i(text, amount.Amount.Get(e, data));
                    }
                }.Hh(UI.Icons().M.Coins));
            }

            public override void Hover(GBox b, Event event, EContext context)
            {
                b.Add(UI.Icons().S.Money);
                b.TextLL(Dic.¤¤Currs);
                b.Tab(6);
                int ava = (int)FACTIONS.player().credits().Credits();
                {
                    GText te = b.Text();
                    GFORMAT.i(te, amount.Amount.Get(event, context));
                    b.Add(te);

                    te = b.Text();
                    te.Add('(');
                    GFORMAT.i(te, ava);
                    te.Add(')');
                    te.Normalify();
                    b.Add(te);
                }
            }

            public override string Problem(Event event, EContext context)
            {
                if (!negativeAllowed && -amount.Amount.Get(event, context) > FACTIONS.player().credits().GetD())
                    return ¤¤tooPoor;
                return null;
            }
        }
    }
}