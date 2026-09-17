using System;
using System.Collections.Generic;
using game;
using game.boosting;
using game.event.engine;
using game.faction;
using game.faction.npc;
using game.faction.royalty;
using init.sprite.UI;
using init.type;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using world;
using world.map.regions;
using world.region;

namespace game.event.actions
{
    internal class _BOOST_PERM : EventActionConstructor
    {
        private static readonly CharSequence ¤¤sTitle = "Boosted Subjects";
        private static readonly CharSequence ¤¤sTitleR = "Boosted Regions";
        private static readonly CharSequence ¤¤sTitleF = "Boosted Factions";

        static _BOOST_PERM()
        {
            D.ts(typeof(_BOOST_PERM));
        }

        public _BOOST_PERM() : base("BOOST_PERM")
        {
        }

        public override EventAction Action(Data data)
        {
            return new Imp(Key, data.Parent, data.Choice, data.Json, data.All);
        }

        public sealed class Imp : EventAction
        {
            private readonly TmpBoostSpec spec;
            private LIST<HCLASS_RACE> pops;
            private readonly bool player;
            private readonly bool regions;
            private readonly bool factions;
            private readonly bool royalties;

            public Imp(string key, Event parent, EChoice choice, Json data, LISTE<EventAction> all) : base(key, all)
            {
                string k = "EVENT_" + parent.Key;
                if (choice != null)
                    k += choice.Index;
                spec = new TmpBoostSpec(k, parent.Info.Name, parent.Info.Desc, parent.Info.Icon);
                spec.Spec.Read(data, BValue.VALUE1);
                pops = HCLASS_RACE.MAP().ReadManyWarn(data);
                player = data.Bool("BOOST_ONLY_PLAYER", false);
                regions = data.Bool("USE_SELECTION_REGIONS", true);
                factions = data.Bool("USE_SELECTION_FACTIONS", true);
                royalties = data.Bool("USE_SELECTION_ROYALTIES", true);
                data.CheckUnused();
            }

            public override void Exe(Event e, EContext data)
            {
                if (player)
                {
                    GAME.BOOST().Factions.Set(FACTIONS.player(), spec, true);
                    return;
                }

                if (regions && data.Regs.Am > 0)
                {
                    foreach (Region reg in WORLD.REGIONS().Active())
                    {
                        if (RD.event().ii.Get(reg) == 1)
                        {
                            GAME.BOOST().Regions.Set(reg, spec, true);
                        }
                    }
                }

                if (factions && data.Faction.Am > 0)
                {
                    foreach (Faction f in FACTIONS.Active())
                    {
                        if (f.Event())
                        {
                            GAME.BOOST().Factions.Set(f, spec, true);
                        }
                    }
                }

                if (royalties && data.Royalty.Am > 0)
                {
                    foreach (FactionNPC f in FACTIONS.NPCs())
                    {
                        foreach (Royalty roy in f.Court().All())
                        {
                            if (roy.Event())
                            {
                                GAME.BOOST().Factions.Set(f, spec, true);
                                break;
                            }
                        }
                    }
                }

                foreach (HCLASS_RACE p in pops)
                {
                    GAME.BOOST().Popcl.Set(p, spec, true);
                }

                base.Exe(e, data);
            }

            public override void Hover(GBox b, Event eventObj, EContext context)
            {
            }

            public override void AddToMessageBody(LISTE<RENDEROBJ> rows, Event eventObj, EContext context, RECTANGLE messBody)
            {
                if (!player && pops.Size > 0)
                {
                    rows.Add(new GHeader(¤¤sTitle, UI.FONT().S));
                    foreach (HCLASS_RACE p in pops)
                    {
                        rows.Add(new GStat
                        {
                            Update = text =>
                            {
                                text.Add(p.race.info.names);
                                text.S();
                                text.Add('(');
                                text.Add(p.cl.names);
                                text.Add(')');
                            }
                        }.Hh(p.race.appearance().icon));
                    }
                }

                if (!player && context.Regs.Am > 0)
                {
                    rows.Add(new GStat
                    {
                        Update = text => GFORMAT.i(text, context.Regs.Am)
                    }.Hh(¤¤sTitleR));
                }

                if (!player && context.Faction.Am > 0)
                {
                    rows.Add(new GStat
                    {
                        Update = text => GFORMAT.i(text, context.Faction.Am)
                    }.Hh(¤¤sTitleF));
                }

                GRows rr = new GRows(6).SetMin(100);

                foreach (BoostSpec s in spec.Spec.All())
                {
                    rr.Add(new GStat
                    {
                        Update = text =>
                        {
                            if (s.booster.isMul)
                            {
                                text.Add('*').S();
                                GFORMAT.f1(text, s.booster.to());
                            }
                            else
                            {
                                GFORMAT.f0(text, s.booster.to());
                            }
                        }
                    }.Hh(s.boostable.icon));
                }

                rows.Add(rr.Rows());
            }
        }
    }
}