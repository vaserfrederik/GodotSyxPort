using game.faction;
using game.faction.royalty;
using settlement.stats;
using snake2d.util.sprite.text;
using util.data;

namespace util.text
{
    final class InsertRoyalty : Inserter<Royalty>
    {
        public InsertRoyalty()
        {
            new II("NAME")
            {
                public override void set(Royalty t, Str str)
                {
                    str.add(t.name());
                }
            };

            new II("NAME_FULL")
            {
                public override void set(Royalty t, Str str)
                {
                    t.nameFull(str);
                }
            };

            new II("RANK")
            {
                public override void set(Royalty t, Str str)
                {
                    t.nameSucc(str);
                }
            };

            join(new Inserter<Induvidual>(new InsertIndu(), "INDUVIDUAL_"), new GETTER_TRANS<Royalty, Induvidual>()
            {
                public override Induvidual get(Royalty f)
                {
                    if (f == null)
                        return null;
                    return f.induvidual;
                }
            });

            join(new Inserter<Faction>(new InsertFaction(), "FACTION_"), new GETTER_TRANS<Royalty, Faction>()
            {
                public override Faction get(Royalty f)
                {
                    if (f == null)
                        return null;
                    return f.court.faction;
                }
            });
        }
    }
}