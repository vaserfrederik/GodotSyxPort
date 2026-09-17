using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using game;
using game.boosting;
using game.faction;
using game.time;
using init.race;
using init.sprite.UI;
using init.type;
using init.value;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data;
using util.text;
using world;
using world.army;
using world.entity.army;
using world.map.regions;
using world.region;

namespace world.region.pop
{
    public class RDRace : INDEXED
    {
        private static readonly string ¤¤PopulationTarget = "Pop. Target";
        private static readonly string ¤¤RulingSpecies = "¤Ruling Species";
        private static readonly string ¤¤Biome = "¤Species Biome";
        private static readonly string ¤¤Armies = "¤Army presence";
        private static readonly string ¤¤Representation = "¤Representation";

        static RDRace()
        {
            util.text.Dic.¤¤Add(RDRace.¤¤PopulationTarget);
            util.text.Dic.¤¤Add(RDRace.¤¤RulingSpecies);
            util.text.Dic.¤¤Add(RDRace.¤¤Biome);
            util.text.Dic.¤¤Add(RDRace.¤¤Armies);
            util.text.Dic.¤¤Add(RDRace.¤¤Representation);
        }

        public readonly RDNameList intros;
        public readonly RDNameList fNames;
        public readonly RDNameList rIntro;
        public readonly RDNameList rNames;

        public RDRace(Race r, RDInit init)
        {
            intros = new RDNameList(r.info.winfo.intros);
            fNames = new RDNameList(r.info.winfo.fNames);
            rIntro = new RDNameList(r.info.winfo.rIntro);
            rNames = new RDNameList(r.info.winfo.rNames);

            init.savable.add(new SAVABLE()
            {
                save = (FilePutter file) =>
                {
                    file.i(intros.i);
                    file.i(fNames.i);
                    file.i(rIntro.i);
                    file.i(rNames.i);
                },

                load = (FileGetter file) =>
                {
                    intros.i = file.i();
                    fNames.i = file.i();
                    rIntro.i = file.i();
                    rNames.i = file.i();
                },

                clear = () =>
                {
                    // TODO Auto-generated method stub
                }
            });
        }

        public static class RDNameList
        {
            private int i = 0;
            private readonly List<string> all;

            private RDNameList(string[] nn)
            {
                all = new List<string>(nn);
                i = RND.rInt(all.Count);
            }

            public string next()
            {
                i %= all.Count;
                string s = all[i];
                if (FACTIONS.player() != null && Str.isSame(s, FACTIONS.player().name))
                {
                    i++;
                    i %= all.Count;
                    s = all[i];
                }
                i++;
                return s;
            }

            public string get(int index)
            {
                return all[index];
            }
        }
    }
}