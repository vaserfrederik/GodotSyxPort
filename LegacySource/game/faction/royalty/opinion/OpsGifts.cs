using game.boosting;
using game.faction.npc;
using game.faction.royalty;
using game.faction.royalty.opinion;
using game.time;
using init.sprite.UI;
using util.text;
using System;
using System.Collections.Generic;

public static class OpsGifts
{
    private static string ¤¤name = "Generosity";
    private static string ¤¤nameE = "Extortion";
    private static string ¤¤nameD = "Based on your previous dealings and gifts.";

    static OpsGifts()
    {
        D.ts(typeof(OpsGifts));
    }

    private readonly ROpperDown op;
    private readonly ROpperDown ex;

    public OpsGifts()
    {
        double year = TIME.secondsPerDay() * 16;
        op = new ROpperDown("DEALINGS", ¤¤name, ¤¤nameD, UI.icons().s.happy, 100, false, year * 10 * 100)
        {
            public override double getModifier(Royalty roy)
            {
                return 0.25 + 0.75 * BOOSTABLES.NOBLE().PRIDE.get(roy.induvidual);
            }

            public override double increase(Royalty roy)
            {
                return (1 + 99 * value.getD(roy)) * base.increase(roy);
            }
        };
        ex = new ROpperDown("DEALINGSE", ¤¤nameE, ¤¤nameD, UI.icons().s.happy, -100, false, year * 10 * 100)
        {
            public override double getModifier(Royalty roy)
            {
                return 0.25 + 0.75 * BOOSTABLES.NOBLE().PRIDE.get(roy.induvidual);
            }

            public override double increase(Royalty roy)
            {
                return (1 + 99 * value.getD(roy)) * base.increase(roy);
            }
        };
    }

    public double getGenerosityNeededForPeace(FactionNPC f)
    {
        return (ROPINION.getPeaceValue(f, op, 1) - op.value.getD(f.king())) * op.to();
    }

    public double getGenerosityNeededForOpinion(FactionNPC f, double target)
    {
        return (ROPINION.getOpinionValue(f, op, target) - op.value.getD(f.king())) * op.to();
    }

    public void makeDeal(FactionNPC f, double generosity)
    {
        foreach (Royalty r in f.court().all())
        {
            makeDeal(r, r.isKing() ? generosity : generosity * 0.25);
        }
    }

    private void makeDeal(Royalty roy, double generosity)
    {
        if (generosity < 0)
        {
            ex.value.incD(roy, generosity / (ex.to() * ex.getModifier(roy)));
        }
        else
        {
            op.value.incD(roy, generosity / (op.to() * op.getModifier(roy)));
        }
    }
}