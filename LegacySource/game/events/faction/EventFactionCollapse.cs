using System;
using System.Collections.Generic;
using System.IO;
using game.events.EVENTS;
using game.faction.FACTIONS;
using game.faction.Faction;
using game.faction.diplomacy.DIP;
using game.faction.npc.FactionNPC;
using game.time.TIME;
using init.sprite.UI;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.text;
using view.ui.diplomacy;
using world;
using world.entity.army;
using world.map.pathing;
using world.map.regions;
using world.region;

namespace game.events.faction
{
    public class EventFactionCollapse : EventResource
    {
        private static readonly double dTime = TIME.secondsPerDay() * 32;
        private double timer = dTime;
        private readonly ArrayList<Region> tmp = new ArrayList<Region>(32);

        private static readonly CharSequence ¤¤title = "Realm Collapses";
        private static readonly CharSequence ¤¤desc = "Due to internal strife, the realm of {0} has collapsed and much of its lands have been lost.";
        private static readonly CharSequence ¤¤mess = "This might be a good time to expand our kingdom into these lawless lands without diplomatic penalty.";
        private int nextFaction;
        private double nextAm;

        static EventFactionCollapse()
        {
            D.ts(typeof(EventFactionCollapse));
        }

        public EventFactionCollapse() : base("FACTION_COLLAPSE")
        {
            Clear();
        }

        protected override void Update(double ds)
        {
            timer -= ds;
            if (timer < 0)
            {
                Faction f = FACTIONS.GetByIndex(nextFaction);

                if (f.IsActive() && f is FactionNPC && DIP.WAR().All(f).Count == 0)
                {
                    FactionNPC ff = (FactionNPC)f;
                    if (!ff.Sanctified)
                        Shatter(ff);
                }
                Clear();
            }
        }

        private void Shatter(FactionNPC f)
        {
            if (f.Realm().Regions() <= 1)
                return;
            int si = 1;
            if (f.Realm().Regions() > 2)
                si = 1 + RND.rInt(f.Realm().Regions() - 1);
            Region start = f.Realm().Region(si);

            int am = (int)(f.Realm().Regions() * nextAm);
            am = CLAMP.I(am, 1, 32);

            tmp.Clear();
            foreach (RegDist d in WORLD.PATH().RegFinder.All(start, Treaty.FACTION, WRegSel.DUMMY()))
            {
                if (d.Reg.Faction() == f && d.Reg != f.CapitolRegion())
                {
                    tmp.Add(d.Reg);
                    am--;
                    if (am <= 0)
                        break;
                }
            }

            if (tmp.Count > 0)
            {
                Str.TMP.Clear().Add(¤¤desc).Insert(0, f.Name);
                WORLD.LOG().Log(f, null, UI.Icons().S.Degrade, Str.TMP, f.Cx(), f.Cy());
                if (RD.DIST().FactionHasRegionBorderingPlayer(f))
                {
                    new UIDipMess(¤¤title, Str.TMP.Clear().Add(¤¤desc).Insert(0, f.Name), ¤¤mess, f).Send();
                }
                foreach (Region reg in tmp)
                {
                    if (reg.Faction() != f)
                        continue;
                    RD.SetFaction(reg, null, true);
                    foreach (WArmy a in WORLD.ENTITIES().Armies.Fill(reg))
                    {
                        if (a.Faction() == f)
                            a.Disband();
                    }
                }
            }
        }

        protected override void Save(FilePutter file)
        {
            file.D(timer);
            file.I(nextFaction);
            file.D(nextAm);
        }

        protected override void Load(FileGetter file)
        {
            timer = file.D();
            nextFaction = file.I();
            nextAm = file.D();
        }

        protected override void Clear()
        {
            timer = RND.rFloat() * dTime;
            nextFaction = RND.rInt(FACTIONS.MAX());
            nextAm = RND.rFloat();
        }
    }
}