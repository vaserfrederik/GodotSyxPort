using System;
using System.Collections.Generic;
using game.boosting;
using init;
using init.paths;
using init.race;
using init.type.CRIME_PUNISHMENTS;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.type
{
    public class CRIMES
    {
        private readonly CRIME WAR;
        private readonly CRIME THEFT;
        private readonly CRIME MURDER;
        private readonly CRIME VANDALISM;
        private readonly CRIME FLASHING;
        private readonly CRIME DISRESPECT;
        private readonly CRIME SPEECH;
        private readonly CRIME PLEASURE;

        private readonly CRIME S_THEFT;
        private readonly CRIME S_MURDER;
        private readonly CRIME S_DISRESPECT;
        private readonly CRIME S_PLEASURE;

        private readonly LIST<CRIME> ALL;
        private readonly LIST<CRIME> CITIZENS;
        private readonly LIST<CRIME> SLAVES;

        private static CRIMES self;

        public CRIMES(INIT init)
        {
            self = this;
            LinkedList<CRIME> all = new LinkedList<CRIME>();

            Json json = new Json(PATHS.CONFIG().init.gets("LAW")).json("CRIMES");
            Json desc = new Json(PATHS.CONFIG().text.gets("LAW")).json("CRIMES");

            WAR = new CRIME(json, desc, all, "WAR", false, HCLASSES.OTHER());

            S_THEFT = new CRIME(json, desc, all, "S_THEFT", HCLASSES.SLAVE());
            S_MURDER = new CRIME(json, desc, all, "S_MURDER", HCLASSES.SLAVE());
            S_DISRESPECT = new CRIME(json, desc, all, "S_DISRESPECT", HCLASSES.SLAVE());
            S_PLEASURE = new CRIME(json, desc, all, "S_PLEASURE", false, HCLASSES.SLAVE());

            FLASHING = new CRIME(json, desc, all, "FLASHING", HCLASSES.CITIZEN());
            THEFT = new CRIME(json, desc, all, "THEFT", HCLASSES.CITIZEN());
            VANDALISM = new CRIME(json, desc, all, "VANDALISM", HCLASSES.CITIZEN());
            MURDER = new CRIME(json, desc, all, "MURDER", HCLASSES.CITIZEN());
            DISRESPECT = new CRIME(json, desc, all, "DISRESPECT", HCLASSES.CITIZEN());
            PLEASURE = new CRIME(json, desc, all, "PLEASURE", false, HCLASSES.CITIZEN());
            SPEECH = new CRIME(json, desc, all, "SPEECH", HCLASSES.CITIZEN());

            PLEASURE.isCriminal = false;
            S_PLEASURE.isCriminal = false;
            ALL = new ArrayList<CRIME>(all);

            ArrayListGrower<CRIMES.CRIME> c = new ArrayListGrower<CRIMES.CRIME>();
            ArrayListGrower<CRIMES.CRIME> s = new ArrayListGrower<CRIMES.CRIME>();

            foreach (CRIME cr in all)
            {
                if (cr.cl == HCLASSES.CITIZEN())
                    c.add(cr);
                if (cr.cl == HCLASSES.SLAVE())
                    s.add(cr);
            }

            CITIZENS = c;
            SLAVES = s;
        }

        public static LIST<CRIME> all(HCLASS cl)
        {
            if (cl == HCLASSES.CITIZEN())
                return self.CITIZENS;
            return self.SLAVES;
        }

        public static LIST<CRIME> CITIZENS()
        {
            return self.CITIZENS;
        }

        public static LIST<CRIME> SLAVES()
        {
            return self.SLAVES;
        }

        public static CRIME PERSECUTED(HCLASS cl)
        {
            if (cl == HCLASSES.SLAVE())
                return self.S_PLEASURE;
            return self.PLEASURE;
        }

        public static CRIME DISRESPECT()
        {
            return self.DISRESPECT;
        }

        public static CRIME FLASHING()
        {
            return self.FLASHING;
        }

        public static CRIME VANDALISM()
        {
            return self.VANDALISM;
        }

        public static CRIME MURDER()
        {
            return self.MURDER;
        }

        public static CRIME THEFT()
        {
            return self.THEFT;
        }

        public static CRIME SPEECH()
        {
            return self.SPEECH;
        }

        public static CRIME S_MURDER()
        {
            return self.S_MURDER;
        }

        public static CRIME S_DISRESPECT()
        {
            return self.S_DISRESPECT;
        }

        public static CRIME S_PLEASURE()
        {
            return self.S_PLEASURE;
        }

        public static LIST<CRIME> ALL()
        {
            return self.ALL;
        }

        public static Response RESPONSE()
        {
            return self.RESPONSE;
        }

        public static class Response
        {
            public double oldHappiness;
            public double newHap;
            public double oldLaw;
            public double newLaw;
            public double oldloy;
            public double loy;
            public double diff;
        }
    }

    public class CRIME : INDEXED
    {
        public string name;
        public string description;
        public bool isCriminal;
        public Standing standing;
        public Standing loyalty;

        public CRIME(Json json, Json desc, LinkedList<CRIME> all, string key, bool criminal, HCLASS cl)
            : base(all.Count)
        {
            json = json.json(key);
            desc = desc.json(key);
            name = json.s("name");
            description = desc.s("desc");
            isCriminal = criminal;
            standing = new Standing(json.json("standing"));
            loyalty = new Standing(json.json("loyalty"));
        }

        public double tyrrany(HCLASS cl, Race race, PUNISHMENT pun)
        {
            return standing.value * pun.tyrrany + loyalty.value * pun.loyalty;
        }

        public double law(HCLASS cl, Race race, PUNISHMENT pun)
        {
            return standing.value * pun.law + loyalty.value * pun.loyalty;
        }

        public Response loyaltyInc(HCLASS cl, Race race, PUNISHMENT pun)
        {
            return loyaltyIncCurrent(cl, race, tyrrany(cl, race, pun), law(cl, race, pun));
        }

        public Response loyaltyIncCurrent(HCLASS cl, Race race)
        {
            return loyaltyIncCurrent(cl, race, stat().tyrrany(cl, race), stat().law(cl, race));
        }

        private Response loyaltyIncCurrent(HCLASS cl, Race race, double dtyrrany, double dlaw)
        {
            double hapCurrent = STANDINGS.get(cl).bhappiness.get(cl.get(race));
            Response response = new Response();

            double hapWithout = hapCurrent / (1 - STATS.LAW().tyrrany(cl, race));
            double nextTyr = 0;
            for (int ci = 0; ci < CRIMES.all(cl).size(); ci++)
            {
                CRIME c = CRIMES.all(cl).get(ci);
                nextTyr += c.tyrrany(cl, race, c.stat().punishment(cl, race).punish);
            }
            nextTyr -= tyrrany(cl, race, stat().punishment(cl, race).punish);
            response.oldHappiness = hapWithout * (1 - nextTyr);

            nextTyr += dtyrrany;
            response.newHap = hapWithout * (1 - nextTyr);

            double lawMul = 1;
            double lawAdd = BOOSTABLES.CIVICS().LAW.baseValue;
            foreach (Booster bo in BOOSTABLES.CIVICS().LAW.all())
            {
                if (bo.isMul)
                    lawMul *= bo.get(HCLASS_RACE.clP(race, cl));
                else
                    lawAdd += bo.get(HCLASS_RACE.clP(race, cl));
            }

            for (int ci = 0; ci < CRIMES.all(cl).size(); ci++)
            {
                CRIME c = CRIMES.all(cl).get(ci);
                lawAdd -= c.stat().law(cl, race);
                lawAdd += c.law(cl, race, c.stat().punishment(cl, race).punish);
            }
            lawAdd -= law(cl, race, stat().punishment(cl, race).punish);
            response.oldLaw = lawMul * lawAdd;
            lawAdd += dlaw;

            response.newLaw = lawMul * lawAdd;

            double mul = 1;
            double add = STANDINGS.get(cl).bloyalty.baseValue;
            foreach (Booster bo in STANDINGS.get(cl).bloyalty.all())
            {
                if (bo.isMul)
                    mul *= bo.get(HCLASS_RACE.clP(race, cl));
                else
                    add += bo.get(HCLASS_RACE.clP(race, cl));
            }

            add -= hapCurrent;
            mul /= (1 + BOOSTABLES.CIVICS().LAW.get(HCLASS_RACE.clP(race, cl)));
            response.oldloy = (add + response.oldHappiness) * mul * (1 + response.oldLaw);
            response.loy = (add + response.newHap) * mul * (1 + response.newLaw);

            response.diff = response.loy - response.oldloy;
            response.diff = Math.Floor(response.diff * 1000.0) / 1000.0;
            return response;
        }
    }

    public class Standing
    {
        public double baseValue;
        public double multiplier;

        public Standing(Json json)
        {
            baseValue = json.d("base");
            multiplier = json.d("mul");
        }

        public double value
        {
            get { return baseValue * multiplier; }
        }
    }
}