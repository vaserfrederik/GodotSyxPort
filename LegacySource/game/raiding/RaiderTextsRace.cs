using System.Collections.Generic;
using snake2d.util.file;
using snake2d.util.sets;

public sealed class RaiderTextsRace
{
    public readonly LIST<CharSequence> greetings;
    public readonly LIST<CharSequence> mids;
    public readonly LIST<CharSequence> bodies;
    public readonly LIST<CharSequence> ends;

    public readonly LIST<CharSequence> rgreetings;
    public readonly LIST<CharSequence> rmids;
    public readonly LIST<CharSequence> rbodies;
    public readonly LIST<CharSequence> rends;

    public readonly LIST<CharSequence> rejected;
    public readonly LIST<CharSequence> payed;
    public readonly LIST<CharSequence> afterRaid;

    public readonly LIST<CharSequence> allyHelp;
    public readonly LIST<CharSequence> allyDead;
    public readonly LIST<CharSequence> allyFight;

    public RaiderTextsRace(Json j)
    {
        {
            Json jj = j.json("FIRST");
            greetings = tt(jj.texts("GREETING"));
            mids = tt(jj.texts("INTROS"));
            bodies = tt(jj.texts("BODIES"));
            ends = tt(jj.texts("ENDS"));
        }
        {
            Json jj = j.json("REPEAT");
            rgreetings = tt(jj.texts("GREETING"));
            rmids = tt(jj.texts("INTROS"));
            rbodies = tt(jj.texts("BODIES"));
            rends = tt(jj.texts("ENDS"));
        }

        rejected = tt(j.texts("REJECTED"));
        payed = tt(j.texts("PAYED"));
        afterRaid = tt(j.texts("AFTER_RAID"));
        allyHelp = tt(j.texts("HELP"));
        allyDead = tt(j.texts("HELP_DESTROYED"));
        allyFight = tt(j.texts("HELP_FIGHT"));
    }

    private ArrayList<CharSequence> tt(CharSequence[] tt)
    {
        RaiderText.insert.check(tt);
        return new ArrayList<CharSequence>(tt);
    }
}