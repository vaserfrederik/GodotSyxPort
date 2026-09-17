using System;
using System.Collections.Generic;
using game.faction;
using settlement.stats;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data;
using util.text;

namespace game.raiding
{
    [Serializable]
    public sealed class RaiderText
    {
        private static readonly long serialVersionUID = 1L;
        public LinkedList<string> demandBody = new LinkedList<string>();
        public LinkedList<string> rejected = new LinkedList<string>();
        public LinkedList<string> payed = new LinkedList<string>();
        public LinkedList<string> afterRaid = new LinkedList<string>();

        public static readonly Inserter<Raider> insert = new Inserter<Raider>();
        static RaiderText()
        {
            insert.newII("RAIDER_NAME", (t, str) => str.Add(t.name));

            insert.Join(INSERT.indu, (f) => f.indu);
            insert.Join(INSERT.faction, (f) => FACTIONS.player());
            insert.Join(INSERT.player, (f) => RND.rInt());
        }

        public RaiderText()
        {
        }

        public void Set(Raider raider, bool first)
        {
            RaiderTextsRace tt = raider.indu.Race().Info.raiderMess;
            if (first)
                Insert(demandBody, raider, tt.greetings.Rnd(), tt.mids.Rnd(), tt.bodies.Rnd(), tt.ends.Rnd());
            else
                Insert(demandBody, raider, tt.rgreetings.Rnd(), tt.rmids.Rnd(), tt.rbodies.Rnd(), tt.rends.Rnd());
            Insert(payed, raider, tt.payed.Rnd());
            Insert(rejected, raider, tt.rejected.Rnd());
            Insert(afterRaid, raider, tt.afterRaid.Rnd());
        }

        public void Insert(LinkedList<string> res, Raider raider, params CharSequence[] sources)
        {
            res.Clear();
            foreach (var s in sources)
            {
                Str.TMP.Clear();
                Str.TMP.Add(s);

                insert.Set(Str.TMP, raider);
                res.Add(Str.TMP.ToString());
            }
        }
    }
}