using System;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d.util.sprite.text;
using util.text;

namespace view.sett.ui.subject
{
    final class SProblem
    {
        private static CharSequence ¤¤ProblemSick = "¤Currently ill with {0}. Needs to recuperate.";
        private static CharSequence ¤¤ProblemSick2 = "¤Currently ill with {0}. Needs a hospital.";
        private static CharSequence ¤¤ProblemInjured = "¤Bleeding badly";
        private static CharSequence ¤¤Starving = "¤Starving to death.";
        private static CharSequence ¤¤CutOff = "¤Cut off from the throne.";
        private static CharSequence ¤¤Exposed = "¤Exposed to the temperature!";
        private static CharSequence ¤¤OnStrike = "¤On Strike, refuses to work";

        private static CharSequence ¤¤ProbLeisure = "¤Leisure Time.";

        static SProblem()
        {
            D.ts(typeof(SProblem));
        }

        public static CharSequence problem(Humanoid a)
        {
            if (STATS.DISEASE().status(a.indu()).active && STATS.DISEASE().shouldHospital(a))
            {
                if (STATS.DISEASE().shouldHospital(a))
                    return Str.TMP.clear().add(¤¤ProblemSick).insert(0, STATS.DISEASE().get(a.indu()).info.name);
                else
                    return Str.TMP.clear().add(¤¤ProblemSick2).insert(0, STATS.DISEASE().get(a.indu()).info.name);
            }
            if (STATS.NEEDS().INJURIES.inDanger(a.indu()))
            {
                return ¤¤ProblemInjured;
            }
            if (STATS.NEEDS().EXPOSURE.COUNT.indu().get(a.indu()) > 0)
                return ¤¤Exposed;
            if (STATS.FOOD().STARVATION.indu().get(a.indu()) > 0)
                return ¤¤Starving;
            if (STATS.POP().TRAPPED.indu().get(a.indu()) > 0)
                return ¤¤CutOff;
            if (GAME.EVENT().message(a.indu()) != null)
                return GAME.EVENT().message(a.indu());

            return null;
        }

        public static CharSequence warning(Humanoid a)
        {
            if (GAME.events().riot.onStrike(a))
                return ¤¤OnStrike;

            if (STATS.WORK().WORK_TIME.indu().isMax(a.indu()))
            {
                return ¤¤ProbLeisure;
            }
            return null;
        }
    }
}