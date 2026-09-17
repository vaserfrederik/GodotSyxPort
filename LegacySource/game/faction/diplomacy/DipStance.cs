using System;
using System.Collections.Generic;
using game.faction;
using game.faction.npc;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.gui.misc;
using util.info;
using util.keymap;
using util.text;

namespace game.faction.diplomacy
{
    public class DipStance : MAPPED
    {
        private List list = new List();

        public readonly string name;
        public readonly string desc;
        public readonly SPRITE icon;
        public readonly bool trades;
        public readonly bool transit;
        public readonly bool ally;
        public readonly double loyalty;
        public readonly double opinionNeeded;
        public readonly double tarif;
        private readonly int index;
        private readonly string key;

        public DipStance(LISTE<DipStance> all, string key, double loyalty, double minLoyalty, double tarif, bool trades, bool transit, bool ally, string name, string desc, SPRITE icon)
        {
            this.name = name;
            this.desc = desc;
            this.icon = icon;
            this.trades = trades;
            this.transit = transit;
            this.ally = ally;
            this.index = all.add(this);
            this.key = key;
            this.loyalty = loyalty;
            this.opinionNeeded = minLoyalty;
            this.tarif = tarif;
        }

        public bool Is(Faction faction, Faction other)
        {
            return DIP.s.Is(faction, other, this);
        }

        public bool Is(FactionNPC faction)
        {
            return Is(faction, FACTIONS.Player());
        }

        public void Set(Faction instigator, Faction accepter)
        {
            DIP.s.Set(instigator, accepter, this);
        }

        public void Set(FactionNPC a)
        {
            Set(FACTIONS.Player(), a);
        }

        public LIST<Faction> All(Faction f)
        {
            return list.All(f);
        }

        public bool Any(Faction f)
        {
            for (int fi = 0; fi < FACTIONS.Active().Size(); fi++)
            {
                Faction f2 = FACTIONS.Active().Get(0);
                if (f != f2 && Is(f, f2))
                    return true;
            }
            return false;
        }

        public LIST<FactionNPC> Player()
        {
            return list.Player();
        }

        private class List
        {
            private int[] state = Alloc.Ii(FACTIONS.MAX());
            private Faction cf;
            private readonly ArrayList<Faction> tmp = new ArrayList<Faction>(FACTIONS.MAX());
            private readonly ArrayList<FactionNPC> player = new ArrayList<FactionNPC>(FACTIONS.MAX());

            public LIST<Faction> All(Faction f)
            {
                if (f == FACTIONS.Player())
                {
                    if (state[f.Index()] != DIP.s.stateI)
                    {
                        state[f.Index()] = DIP.s.stateI;
                        player.ClearSloppy();
                        foreach (FactionNPC o in FACTIONS.NPCs())
                        {
                            if (f != o && Is(o, f))
                                player.Add(o);
                        }
                    }

                    return player;
                }
                if (cf != f || state[f.Index()] != DIP.s.stateI)
                {
                    cf = f;
                    state[f.Index()] = DIP.s.stateI;
                    tmp.ClearSloppy();
                    foreach (Faction o in FACTIONS.Active())
                    {
                        if (f != o && Is(o, f))
                            tmp.Add(o);
                    }
                }
                return tmp;
            }

            public LIST<FactionNPC> Player()
            {
                All(FACTIONS.Player());
                return player;
            }
        }

        public override int Index()
        {
            return index;
        }

        public override string Key()
        {
            return key;
        }

        private static readonly string ¤¤minOpinion = "Minimum Opinion";
        static
        {
            D.ts(typeof(DipStance));
        }

        public void Hover(GUI_BOX box)
        {
            GBox b = (GBox)box;
            box.Title(name);
            box.Text(desc);
            box.NL();

            if (this != DIP.WAR())
            {
                b.TextLL(¤¤minOpinion);
                b.Tab(6);
                b.Add(GFORMAT.f(b.Text(), opinionNeeded));
                b.NL();

                b.TextLL(Dic.¤¤Tariff);
                b.Tab(6);
                b.Add(GFORMAT.perc(b.Text(), tarif));
                b.NL();
            }
        }
    }
}