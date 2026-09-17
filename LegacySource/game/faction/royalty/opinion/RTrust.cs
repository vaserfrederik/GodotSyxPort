using System;
using System.Text;
using game;
using game.boosting;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.royalty;
using init.sprite.UI;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.data;
using util.gui.misc;
using util.info;
using util.text;

public static class RTrust
{
    private static readonly string ¤¤rName = "Rivalry";
    private static readonly string ¤¤vassal = "Vassal";
    private static readonly string ¤¤hName = "Honor";
    private static readonly string ¤¤rFactors = "Trust is gained by maintaining high opinion and treaties, and eroded by rivalry, which is your wealth compared to theirs. Trust below 100% might result in a spontaneous attack, or joining your enemies, if the faction feels like they're on the winning side.";

    static RTrust()
    {
        D.ts(typeof(RTrust));
    }

    public readonly Boostable bo = BOOSTABLES.CIVICS().TRUST;

    public RTrust(FACTIONS factions)
    {
        new BB(BOOSTABLES.CIVICS().bOpinion.name, BOOSTABLES.CIVICS().bOpinion.icon, -100, 100, false)
        {
            public override double vGet(FactionNPC f)
            {
                return 0.5 + ROPINION.get(f.king()) / 200.0;
            }
        };

        new BB(¤¤rName, UI.icons().s.money, 1, 0, true)
        {
            public override double vGet(FactionNPC f)
            {
                if (f == null || DIP.OVERLORD().is(f))
                {
                    return 0;
                }

                double ff = FACTIONS.WORTH().faction(f);
                double pp = FACTIONS.WORTH().faction() * 1.5;
                if (ff < 0)
                    return 1;
                double d = pp / (ff + pp);
                return d;
            }
        };

        //new BB(¤¤rName, UI.icons().s.money, 0, -100, false)
        //{
        //    public override double vGet(FactionNPC f)
        //    {
        //        if (f == null || DIP.OVERLORD().is(f))
        //        {
        //            return 0;
        //        }

        //        double ff = FACTIONS.WORTH().faction(f);
        //        if (ff < 0)
        //            return 0;
        //        double d = 5.0 * FACTIONS.WORTH().faction() / ff;

        //        return CLAMP.d(d, 0, 1000) / 100.0;
        //    }
        //};

        new BB(Dic.¤¤DiplomyStance, UI.icons().s.flag, 1, 2, true)
        {
            public override double vGet(FactionNPC f)
            {
                if (f == null)
                    return 0;

                DipStance stance = DIP.get(f);
                return stance == DIP.VASSAL() ? 0 : stance.loyalty;
            }
        };

        new BB(¤¤vassal, UI.icons().s.flag, 1, 0.5, true)
        {
            public override double vGet(FactionNPC f)
            {
                if (f == null)
                    return 0;

                DipStance stance = DIP.get(f);
                return stance == DIP.VASSAL() ? 1 : 0;
            }
        };

        new BB(¤¤hName, UI.icons().s.fist, 0.5, 2.0, true)
        {
            public override double vGet(FactionNPC f)
            {
                return BOOSTABLES.NOBLE().HONOUR.get(f) / 2.0;
            }
        };
    }

    public static SuperBoostable<Royalty> BOOST()
    {
        return GAME.BOOSTS().TRUST;
    }

    private abstract class BB : SuperSpec<Royalty>
    {
        public BB(string name, SPRITE icon, double from, double to, bool isMul) : base(BOOST(), new BSourceInfo(name, icon), "", from, to, isMul)
        {
        }

        public override double secondsRemaining(Royalty bo)
        {
            return 0;
        }

        public override double increase(Royalty bo)
        {
            return 0;
        }

        protected override double pget(Royalty o)
        {
            if (o == null)
                return 0;

            return vGet(o.court.faction);
        }

        protected abstract double vGet(FactionNPC f);
    }

    public double get(FactionNPC f)
    {
        return BOOST().get(f.king());
    }

    public double get(GETTER<FactionNPC> f)
    {
        return BOOST().get(f.get().king());
    }

    public void hover(GUI_BOX box, FactionNPC f)
    {
        GBox b = (GBox)box;
        b.title(bo.name);
        b.text(bo.desc);
        b.NL(4);
        b.text(¤¤rFactors);
        b.NL(4);
        b.textLL(ROPINION.¤¤wEmmi);
        b.tab(6);
        b.add(GFORMAT.perc(b.text(), ROPINION.EMMI().trustTarget(f.king(), 1.0)));
        b.sep();
        BOOST().hoverDetailed(b, f.court().king().roy());
    }
}