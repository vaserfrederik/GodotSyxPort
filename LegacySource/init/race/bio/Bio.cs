using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using game;
using init.paths;
using init.race;
using settlement.entity.humanoid;
using settlement.stats.standing;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite.text;

namespace init.race.bio
{
    public sealed class Bio
    {
        private static readonly KeyMap<BioLines> cachebio = new KeyMap<BioLines>();

        private readonly BioLines data;
        private readonly BioOpinion improve;
        private readonly LIST<Str> tmp = new ArrayList<Str>(new Str(128));

        static Bio()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    cachebio.Clear();
                }
            };
        }

        public Bio(Json json, Race race) throws IOException
        {
            string f = json.value("BIO_FILE");
            Json org = new Json(PATHS.TEXT().getFolder("race").getFolder("bio").get(f));
            if (!cachebio.ContainsKey(f))
            {
                BioLines d = new BioLines(org);
                cachebio.Put(f, d);
            }

            BioLines data = cachebio.Get(f);
            Json spe = null;
            if (json.has("BIO_FILE_SPECIFIC"))
            {
                spe = new Json(PATHS.TEXT().getFolder("race").getFolder("bio").getFolder("specific").get(json.value("BIO_FILE_SPECIFIC")));
                data = new BioLines(data, spe);
            }
            this.data = data;

            improve = new BioOpinion(
                new BioOpinionData(org, spe),
                race);
        }

        public LIST<BioLine> lines()
        {
            return data.descs;
        }

        public CharSequence opinionTitle(Humanoid indu)
        {
            return improve.title(indu, STANDINGS.get(indu.indu().clas()).current(indu.indu()));
        }

        public void opinions(LIST<Str> res, Humanoid indu)
        {
            improve.get(res, indu);
        }

        public CharSequence opinion(Humanoid indu)
        {
            improve.get(tmp, indu);
            return tmp.get(0);
        }

        public CharSequence houseProblem(Humanoid a)
        {
            foreach (BioLine d in data.houseP)
            {
                CharSequence s = d.get(a);
                if (s != null)
                {
                    return s;
                }
            }
            return null;
        }
    }
}