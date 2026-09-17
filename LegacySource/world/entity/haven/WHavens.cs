using System;
using System.Collections.Generic;
using System.IO;
using game.faction;
using init.race;
using snake2d.util.file;
using snake2d.util.sets;
using util.info;
using util.text;
using world.entity;
using world.map.regions;
using world.region;

namespace world.entity.haven
{
    public class WHavens : WEntityConstructor<WHaven>
    {
        public readonly LIST<WHavenType> types = WHavenType.types();
        private readonly Stack<WHaven> free = new Stack<WHaven>(64);
        private readonly WHavenFactionData[] factions;
        private readonly WHavenType[][] racemap;
        private readonly Player player;

        static WHavens()
        {
            D.t(typeof(WHavens));
        }

        public readonly INFO info = new INFO(Dic.¤¤havens, D.g("HavenD", "Havens are smaller settlements that contain individuals of a specific race. They are independent, but can join a faction if their criteria are met and the faction controls the region in which they are located."));

        public WHavens(LISTE<WEntityConstructor<?>> tot) : base(tot, false)
        {
            factions = new WHavenFactionData[FACTIONS.MAX()];
            for (int i = 0; i < factions.Length; i++)
            {
                factions[i] = new WHavenFactionData(this, i);
            }

            racemap = new WHavenType[RACES.all().Count][];
            for (int i = 0; i < RACES.all().Count; i++)
            {
                int am = 0;
                foreach (WHavenType t in types)
                {
                    if (t.race == RACES.all()[i])
                    {
                        am++;
                    }
                }
                racemap[i] = new WHavenType[am];
                am = 0;
                foreach (WHavenType t in types)
                {
                    if (t.race == RACES.all()[i])
                    {
                        racemap[i][am++] = t;
                    }
                }
            }

            new RD.RDOwnerChanger
            {
                change = (Region reg, Faction oldOwner, Faction newOwner) =>
                {
                    setDirty(oldOwner);
                    setDirty(newOwner);
                }
            };

            player = new Player(this);
        }

        protected override WHaven create()
        {
            if (free.Count > 0)
            {
                return free.Pop();
            }
            return new WHaven();
        }

        protected override void update(double ds)
        {
            player.update(ds);
        }

        public WHaven create(int tx, int ty, WHavenType type, double size, string name)
        {
            WHaven c = create();
            c.add(tx, ty, type, size, name);

            if (c.added())
                return c;

            free.Push(c);
            return null;
        }

        protected override void clear()
        {
            foreach (WHavenFactionData d in factions)
                d.dirty = true;
            player.clear();
        }

        protected override void save(FilePutter file)
        {
            player.save(file);
        }

        protected override void load(FileGetter file)
        {
            player.load(file);
            foreach (WHavenFactionData d in factions)
                d.dirty = true;
        }

        public void setDirty(Faction f)
        {
            if (f == null)
                return;
            factions[f.index()].dirty = true;
        }

        public bool available(Race race)
        {
            return racemap[race.index()].Length > 0;
        }

        public int current(Faction f, Race type)
        {
            int am = 0;
            foreach (WHavenType t in racemap[type.index()])
                am += max(f, t) * player.get(f, t);

            return am;
        }

        public int max(Faction f, Race type)
        {
            int am = 0;
            foreach (WHavenType t in racemap[type.index()])
                am += max(f, t);

            return am;
        }

        public double replenishPerDay(Faction f, Race type)
        {
            double am = 0;
            foreach (WHavenType t in racemap[type.index()])
                am += replenishPerDay(f, t) * player.get(f, t);

            return am;
        }

        public int current(Faction f, WHavenType type)
        {
            factions[f.index()].init();
            return (int)(factions[f.index()].all[type.index()].pop * player.get(f, type));
        }

        public int max(Faction f, WHavenType type)
        {
            factions[f.index()].init();
            return factions[f.index()].all[type.index()].pop;
        }

        public double replenishPerDay(Faction f, WHavenType type)
        {
            factions[f.index()].init();
            return factions[f.index()].all[type.index()].replenish * player.get(f, type);
        }

        public int camps(Faction f, WHavenType type)
        {
            factions[f.index()].init();
            return factions[f.index()].all[type.index()].camps;
        }
    }
}