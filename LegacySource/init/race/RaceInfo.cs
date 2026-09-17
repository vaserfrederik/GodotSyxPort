using System;
using System.Collections.Generic;
using System.Linq;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.info;

namespace init.race
{
    public sealed class RaceInfo : INFO
    {
        public readonly string namePosessive;
        public readonly string namePosessives;
        public readonly string desc_long;
        public readonly string initialChallenge;
        public readonly string[] pros;
        public readonly string[] cons;
        public readonly string[] raiderNames;
        public readonly RaiderTextsRace raiderMess;
        public readonly RaceWorldInfo winfo;
        private static KeyMap<RaceWorldInfo> wi = new KeyMap<RaceWorldInfo>();
        private static KeyMap<string[]> ra = new KeyMap<string[]>();
        private static KeyMap<RaiderTextsRace> ram = new KeyMap<RaiderTextsRace>();
        public readonly LIST<string> armyNames;
        public readonly RPronoun pHE;
        public readonly RPronoun pHIS;
        public readonly RPronoun pHIMSELF;
        public readonly RPronoun pHIM;
        public readonly RPronoun pCHILD;

        public readonly CharSequence[] sHello;
        public readonly CharSequence[] sGoodbye;
        public readonly CharSequence[] sCurse;
        public readonly CharSequence[] sInsult;
        public readonly CharSequence[] sInsulting;
        public readonly CharSequence[] sLord;
        public readonly CharSequence[] sCity;
        public readonly CharSequence[] sOthers;
        public readonly CharSequence[] sSelves;
        public readonly CharSequence[] sSelf;
        public readonly CharSequence[] sChildren;

        public RaceInfo(Json json, Json text) : base(text)
        {
            namePosessive = text.text("POSSESSIVE");
            namePosessives = text.text("POSSESSIVES");
            desc_long = text.text("DESC_LONG");

            initialChallenge = text.text("CHALLENGE", "");

            pros = text.textsTry("PROS");
            cons = text.textsTry("CONS");

            pHE = new RPronoun("PRONOUN_HE", text);
            pHIS = new RPronoun("PRONOUN_HIS", text);
            pHIMSELF = new RPronoun("PRONOUN_HIMSELF", text);
            pHIM = new RPronoun("PRONOUN_HIM", text);
            pCHILD = new RPronoun("PRONOUN_CHILD", text);

            armyNames = new ArrayList<string>(text.texts("ARMY_NAMES", 1, 255));

            string f = json.value("WORLD_NAME_FILE");
            if (!wi.containsKey(f))
            {
                wi.put(f, new RaceWorldInfo(f));
            }
            winfo = wi.get(f);

            f = json.value("RAID_TEXT_FILE");
            if (!ram.containsKey(f))
                ram.put(f, new RaiderTextsRace(new Json(PATHS.RACE().text.getFolder("raider").getFolder("message").gets(f))));
            raiderMess = ram.get(f);

            f = json.value("RAIDER_NAME_FILE");
            if (!ra.containsKey(f))
            {
                ra.put(f, new Json(PATHS.RACE().text.getFolder("raider").getFolder("name").gets(f)).texts("NAMES"));
            }

            raiderNames = ra.get(f);
            //sInsult = text.text("INSULT");

            sHello = text.texts("HELLO");
            sGoodbye = text.texts("GOODBYE");
            sCurse = text.texts("CURSE");
            sInsult = text.texts("INSULT");
            sInsulting = text.texts("INSULTING");
            sLord = text.texts("LORD");
            sCity = text.texts("CITY");
            sOthers = text.texts("OTHERS");
            sSelves = text.texts("SELVES");
            sSelf = text.texts("SELF");
            sChildren = text.texts("CHILDREN");
        }

        public sealed class RaceWorldInfo
        {
            public readonly string[] intros;
            public readonly string[] fNames;
            public readonly string[] rIntro;
            public readonly string[] rNames;

            public RaceWorldInfo(string key)
            {
                Json json = new Json(PATHS.NAMES().getFolder("world").gets(key));
                intros = json.texts("INTRO", 1, 128);
                fNames = json.texts("NAMES", 1, 512);
                rIntro = json.texts("RULER_INTRO", 1, 128);
                rNames = json.texts("RULER", 1, 512);
            }
        }

        public sealed class RPronoun
        {
            public readonly CharSequence[] pronouns;
            public readonly CharSequence[] pronounsC;

            public RPronoun(string key, Json text)
            {
                pronouns = text.texts(key);
                pronounsC = text.texts(key + "C");
            }

            public CharSequence get(Induvidual i, bool cap)
            {
                int k = STATS.APPEARANCE().gender.get(i);
                return get(k, cap);
            }

            public CharSequence get(int gender, bool cap)
            {
                int k = gender;
                CharSequence[] ll = pronouns;
                if (cap)
                    ll = pronounsC;
                k = CLAMP.i(k, 0, ll.Length - 1);
                return ll[k];
            }
        }
    }
}