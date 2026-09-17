using System.Collections.Generic;
using util.keymap;
using util.text;

namespace init.type
{
    public sealed class CAUSE_ARRIVES
    {
        private readonly List<CAUSE_ARRIVE> all = new List<CAUSE_ARRIVE>();

        static CAUSE_ARRIVES()
        {
            D.gInit(typeof(CAUSE_ARRIVES));
        }

        private readonly CAUSE_ARRIVE BORN = new CAUSE_ARRIVE(all,
            "BORN",
            D.g("Born"),
            D.g("BornD", "Subjects that have been born in your city."), false);
        private readonly CAUSE_ARRIVE IMMIGRATED = new CAUSE_ARRIVE(all,
            "IMMIGRATED",
            D.g("Immigrated"),
            D.g("ImmigratedD", "Subjects that have immigrated to your city."), true);
        private readonly CAUSE_ARRIVE EMANCIPATED = new CAUSE_ARRIVE(all,
            "EMANCIPATED",
            D.g("Emancipated"),
            D.g("EmancipatedD", "Subjects that are freed slaves."), false);
        private readonly CAUSE_ARRIVE PAROLE = new CAUSE_ARRIVE(all,
            "PAROLE",
            D.g("Parole"),
            D.g("ParoleD", "Subjects that have been prisoners and are now pardoned and free citizens."), false);
        private readonly CAUSE_ARRIVE SOLDIER_RETURN = new CAUSE_ARRIVE(all,
            "SOLDIER",
            D.g("Soldiers"),
            D.g("SoldiersD", "Soldiers that have returned from campaigning."), true);
        private readonly CAUSE_ARRIVE CURED = new CAUSE_ARRIVE(all,
            "CURED",
            D.g("Readjusted"),
            D.g("ReadjustedD", "Subjects that have been readjusted in the asylum and cured of insanity."), false);

        private readonly RMAPS<CAUSE_ARRIVE> map = new RMAPS<CAUSE_ARRIVE>("CAUSE_ARRIVE", all);

        public static List<CAUSE_ARRIVE> ALL()
        {
            return self.all;
        }
        public static CAUSE_ARRIVE BORN()
        {
            return self.BORN;
        }
        public static CAUSE_ARRIVE IMMIGRATED()
        {
            return self.IMMIGRATED;
        }
        public static CAUSE_ARRIVE EMANCIPATED()
        {
            return self.EMANCIPATED;
        }
        public static CAUSE_ARRIVE PAROLE()
        {
            return self.PAROLE;
        }
        public static CAUSE_ARRIVE SOLDIER_RETURN()
        {
            return self.SOLDIER_RETURN;
        }
        public static CAUSE_ARRIVE CURED()
        {
            return self.CURED;
        }
        public static RMAPS<CAUSE_ARRIVE> MAP()
        {
            return self.map;
        }

        private static readonly CAUSE_ARRIVES self;

        private CAUSE_ARRIVES()
        {
            self = this;
        }
    }
}