using System;
using System.Collections.Generic;
using System.Text;

namespace Init.Race.Bio
{
    using Game.Faction;
    using Settlement.Entity;
    using Settlement.Entity.Humanoid;
    using Settlement.Main;
    using Settlement.Stats;
    using Snake2D.Util;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;
    using Snake2D.Util.Sprite.Text;
    using Util.Data;
    using Util.Text;

    public class BioLine
    {
        private CharSequence[] strings;
        protected bool nl = false;
        protected readonly int index;
        private static Str str = new Str(256);
        public readonly string key;
        public static readonly Inserter<Humanoid> insert = new Inserter<Humanoid>(INSERT.Human, "");

        static BioLine()
        {
            insert.Join(new Inserter<Humanoid>(INSERT.Human, "FRIEND_"), new GetterTrans<Humanoid, Humanoid>()
            {
                public Humanoid Get(Humanoid a)
                {
                    Entity b = Stats.POP().FRIEND.Get(a.Indu());
                    if (b is Humanoid)
                        return (Humanoid)b;

                    int ri = Stats.RAN().Get(a.Indu(), 1);
                    Entity[] es = SETT.ENTITIES().GetAllEnts();

                    if (es.Length == 0)
                        return a;

                    for (int i = 0; i < es.Length; i++)
                    {
                        Entity e = es[Math.Mod(i + ri, es.Length)];
                        if (e != a && e is Humanoid)
                        {
                            return (Humanoid)e;
                        }
                    }

                    return a;
                }
            });

            insert.Join(INSERT.Faction, new GetterTrans<Humanoid, Faction>()
            {
                public Faction Get(Humanoid f)
                {
                    return FACTIONS.Player();
                }
            });

            insert.Join(INSERT.Player, new GetterTrans<Humanoid, int>()
            {
                public int Get(Humanoid f)
                {
                    return Stats.RAN().Get(f.Indu(), 21, 10);
                }
            });
        }

        public BioLine(Liste<BioLine> all, Json json, string key)
        {
            if (json.Has(key))
                strings = Strings(json, key);
            else
                strings = new CharSequence[0];
            index = all.Add(this);
            this.key = key;
        }

        public BioLine(Liste<BioLine> all, Json json, string key, CharSequence[] backup)
        {
            if (json.Has(key))
                strings = Strings(json, key);
            else
                strings = backup;
            index = all.Add(this);
            this.key = key;
        }

        protected CharSequence[] Strings(Json json, string key)
        {
            CharSequence[] ll = json.Texts(key);
            insert.Check(ll);

            return ll;
        }

        protected bool Use(Humanoid a)
        {
            if (!a.Indu().HType().CLASS.Player)
                return false;
            return true;
        }

        protected BioLine NlSet()
        {
            nl = true;
            return this;
        }

        public CharSequence Get(Humanoid a)
        {
            if (strings.Length == 0)
                return null;
            if (!Use(a))
                return null;
            int ran = Stats.RAN().Get(a.Indu(), 9 + index * 5, 5);
            CharSequence s = strings[Math.Mod((int)ran, strings.Length)];
            str.Clear().Add(s);

            Inserter.SetRandom(Stats.RAN().GetL(a.Indu(), 0));
            insert.Set(str, a);

            return str;
        }

        public bool Nl()
        {
            return nl;
        }
    }
}