using game.battle.div;
using game.boosting;
using init.settings;
using init.sprite.UI;
using settlement.stats;
using snake2d.util.misc;
using util.gui.misc;
using util.info;
using util.text;

namespace game.battle.factors
{
    class Init
    {
        private static CharSequence ¤¤exhaustion = "Exhaustion";
        private static CharSequence ¤¤exhaustionDesc = "Running and fighting will exhaust a division, making them less effective in combat.";
        private static CharSequence ¤¤exhMess = "Exhaustion";

        private static CharSequence ¤¤formName = "Formation";
        private static CharSequence ¤¤formDesc = "A coherent formation of enough depth makes soldiers feel safe and helps them defend each other against attacks.";
        private static CharSequence ¤¤formMess = "Formation is Intact";
        private static CharSequence ¤¤inPos = "In Position";
        private static CharSequence ¤¤depth = "Formation depth";

        private static CharSequence ¤¤armyName = "Numbers";
        private static CharSequence ¤¤armyDesc = "The size of our army against the size of the enemy's army.";
        private static CharSequence ¤¤armyMess = "Numbers";
        private static CharSequence ¤¤armyPlayer = "Army Size";
        private static CharSequence ¤¤armyEnemy = "Enemy Army Size";

        private static CharSequence ¤¤suppliesName = "Supplies";
        private static CharSequence ¤¤suppliesDesc = "The army's supplies prior to engagement.";
        private static CharSequence ¤¤suppliesMess = "Supplies";

        private static CharSequence ¤¤casultiesName = "Casualties";
        private static CharSequence ¤¤casultiesDesc = "The number of casualties in the division.";
        private static CharSequence ¤¤casultiesMess = "Casualties";

        private static CharSequence ¤¤routName = "Rout";
        private static CharSequence ¤¤routDesc = "The likelihood of the division routing.";
        private static CharSequence ¤¤routMess = "Rout";

        private static CharSequence ¤¤projectilesName = "Projectiles";
        private static CharSequence ¤¤projectilesDesc = "The impact of projectiles on the division.";
        private static CharSequence ¤¤projectilesMess = "Projectiles";

        private static CharSequence ¤¤situationName = "Situation";
        private static CharSequence ¤¤situationDesc = "The overall situation of the division in battle.";
        private static CharSequence ¤¤situationMess = "Situation";

        private static CharSequence ¤¤wearinessName = "Weariness";
        private static CharSequence ¤¤wearinessDesc = "The weariness of the division due to prolonged battles.";
        private static CharSequence ¤¤wearinessMess = "Weariness";

        private static CharSequence ¤¤flanksName = "Flanks";
        private static CharSequence ¤¤flanksDesc = "The status of the division's flanks in battle.";
        private static CharSequence ¤¤flanksMess = "Flanks";

        private static CharSequence ¤¤surroundedName = "Surrounded";
        private static CharSequence ¤¤surroundedDesc = "The likelihood of the division being surrounded.";
        private static CharSequence ¤¤surroundedMess = "Surrounded";

        static Init()
        {
            Dic.Add(¤¤exhaustion, "exhaustion");
            Dic.Add(¤¤exhaustionDesc, "exhaustion_desc");
            Dic.Add(¤¤exhMess, "exh_mess");

            Dic.Add(¤¤formName, "form");
            Dic.Add(¤¤formDesc, "form_desc");
            Dic.Add(¤¤formMess, "form_mess");
            Dic.Add(¤¤inPos, "in_pos");
            Dic.Add(¤¤depth, "depth");

            Dic.Add(¤¤armyName, "army");
            Dic.Add(¤¤armyDesc, "army_desc");
            Dic.Add(¤¤armyMess, "army_mess");
            Dic.Add(¤¤armyPlayer, "army_player");
            Dic.Add(¤¤armyEnemy, "army_enemy");

            Dic.Add(¤¤suppliesName, "supplies");
            Dic.Add(¤¤suppliesDesc, "supplies_desc");
            Dic.Add(¤¤suppliesMess, "supplies_mess");

            Dic.Add(¤¤casultiesName, "casulties");
            Dic.Add(¤¤casultiesDesc, "casulties_desc");
            Dic.Add(¤¤casultiesMess, "casulties_mess");

            Dic.Add(¤¤routName, "rout");
            Dic.Add(¤¤routDesc, "rout_desc");
            Dic.Add(¤¤routMess, "rout_mess");

            Dic.Add(¤¤projectilesName, "projectiles");
            Dic.Add(¤¤projectilesDesc, "projectiles_desc");
            Dic.Add(¤¤projectilesMess, "projectiles_mess");

            Dic.Add(¤¤situationName, "situation");
            Dic.Add(¤¤situationDesc, "situation_desc");
            Dic.Add(¤¤situationMess, "situation_mess");

            Dic.Add(¤¤wearinessName, "weariness");
            Dic.Add(¤¤wearinessDesc, "weariness_desc");
            Dic.Add(¤¤wearinessMess, "weariness_mess");

            Dic.Add(¤¤flanksName, "flanks");
            Dic.Add(¤¤flanksDesc, "flanks_desc");
            Dic.Add(¤¤flanksMess, "flanks_mess");

            Dic.Add(¤¤surroundedName, "surrounded");
            Dic.Add(¤¤surroundedDesc, "surrounded_desc");
            Dic.Add(¤¤surroundedMess, "surrounded_mess");
        }

        public Init(DivFactorFactors ff)
        {
            new DivFactor(¤¤exhaustion, ¤¤exhaustionDesc, UI.icons.h, ¤¤exhMess, 1)
            {
                GetD = (Div div) =>
                {
                    if (div.army.men == 0)
                        return 1;
                    return 1.0 - ff.weariness.Get(div) / 10000;
                },
                Phover = (Div div, GBox b) =>
                {
                    b.AddLabel(¤¤weariness);
                    b.AddValue(ff.weariness.Get(div).ToString());
                    b.AddLabel(¤¤wearinessDelta);
                    b.AddValue(-(double)div.status.Engagements() / (1 + div.men)).ToString());
                }
            }.Boost(BOOST.BATTLE.MORALE, -10000, 0, false);

            new DivFactor(¤¤formName, ¤¤formDesc, UI.icons.h, ¤¤formMess, 1)
            {
                GetD = (Div div) =>
                {
                    if (!div.status.Coherent)
                        return 0;
                    double d = div.status.Depth;
                    d /= div.men;
                    d -= 0.3;
                    d *= 4;
                    d = Math.Clamp(d, 0, 1);
                    return d;
                },
                Phover = (Div div, GBox b) =>
                {
                    b.AddLabel(¤¤flanks);
                    b.AddValue(div.status.Flanks.ToString());
                    b.AddLabel(¤¤flanksValue);
                    b.AddValue(Math.Clamp(v(div), 0, 1).ToString());
                }
            }.Boost(BOOST.BATTLE.DEFENCE, 0.2, 1, true);

            new DivFactor(¤¤armyName, ¤¤armyDesc, UI.icons.h, ¤¤armyMess, 1)
            {
                GetD = (Div div) =>
                {
                    if (div.army.men == 0)
                        return 1;
                    return 1.0 - ff.casulties.army.Get(div.army) / (ff.casulties.army.Get(div.army) + div.army.men);
                },
                Phover = (Div div, GBox b) =>
                {
                    b.AddLabel(¤¤inUnit);
                    b.AddValue(ff.casulties.Get(div).ToString());
                    b.AddLabel(¤¤inArmy);
                    b.AddValue(ff.casulties.army.Get(div.army).ToString());
                }
            }.Boost(BOOST.BATTLE.MORALE, 0, 1, true);

            new DivFactor(¤¤suppliesName, ¤¤suppliesDesc, UI.icons.h, ¤¤suppliesMess, 1)
            {
                GetD = (Div div) =>
                {
                    if (div.men == 0)
                        return 1;
                    return 1.0 - Math.Clamp(ff.projectiles.Get(div) / (div.men * 4), 0, 1);
                },
                Phover = (Div div, GBox b) =>
                {
                    b.AddLabel(Dic.¤¤Projectiles);
                    b.AddValue(Math.Clamp(1.0 - GetD(div), 0, 1).ToString());
                }
            }.Boost(BOOST.BATTLE.MORALE, 0.5, 1, true);

            new DivFactor(¤¤situationName, ¤¤situationDesc, UI.icons.h, ¤¤situationMess, 1)
            {
                GetD = (Div div) =>
                {
                    double ee = div.status.AjacentEnemiesPower();
                    if (ee == 0)
                        return 1;
                    double f = div.status.AjacentFriendsPower();
                    double d = (ee - f) / ((ee + f) * 2.0);
                    return 1.0 - Math.Clamp(d, 0, 1);
                },
                Phover = (Div div, GBox b) =>
                {
                    b.AddLabel(¤¤enemiesNear);
                    b.AddValue(-div.status.AjacentEnemiesPower().ToString());
                    b.AddLabel(¤¤friendsNear);
                    b.AddValue(div.status.AjacentFriendsPower().ToString());
                }
            }.Boost(BOOST.BATTLE.MORALE, 0, 1, true);

            new DivFactor(¤¤wearinessName, ¤¤wearinessDesc, UI.icons.h, ¤¤wearinessMess, 1)
            {
                GetD = (Div div) =>
                {
                    return 1.0 - ff.weariness.Get(div) / 10000;
                },
                Phover = (Div div, GBox b) =>
                {
                    b.AddLabel(¤¤weariness);
                    b.AddValue(-ff.weariness.Get(div).ToString());
                    b.AddLabel(¤¤wearinessDelta);
                    b.AddValue(-(double)div.status.Engagements() / (1 + div.men).ToString());
                }
            }.Boost(BOOST.BATTLE.MORALE, -10000, 0, false);

            new DivFactor(¤¤flanksName, ¤¤flanksDesc, UI.icons.h, ¤¤flanksMess, 1)
            {
                GetD = (Div div) =>
                {
                    if (!div.status.Coherent)
                        return 0;
                    double d = div.status.Depth;
                    d /= div.men;
                    d -= 0.3;
                    d *= 4;
                    d = Math.Clamp(d, 0, 1);
                    return d;
                },
                Phover = (Div div, GBox b) =>
                {
                    b.AddLabel(¤¤flanks);
                    b.AddValue(div.status.Flanks.ToString());
                    b.AddLabel(¤¤flanksValue);
                    b.AddValue(Math.Clamp(v(div), 0, 1).ToString());
                }
            }.Boost(BOOST.BATTLE.DEFENCE, 0.2, 1, true);

            new DivFactor(¤¤surroundedName, ¤¤surroundedDesc, UI.icons.h, ¤¤surroundedMess, 1)
            {
                GetD = (Div div) =>
                {
                    double ee = div.status.AjacentEnemiesPower();
                    if (ee == 0)
                        return 0;
                    double f = div.status.AjacentFriendsPower();
                    double d = (ee - f) / ((ee + f) * 2.0);
                    return Math.Clamp(d, 0, 1);
                },
                Phover = (Div div, GBox b) =>
                {
                    b.AddLabel(¤¤enemiesNear);
                    b.AddValue(-div.status.AjacentEnemiesPower().ToString());
                    b.AddLabel(¤¤friendsNear);
                    b.AddValue(div.status.AjacentFriendsPower().ToString());
                }
            }.Boost(BOOST.BATTLE.MORALE, 0, 1, true);
        }
    }
}