using System;
using System.Collections.Generic;
using System.Linq;
using game.GAME;
using game.battle.Armies;
using game.battle.div.Div;
using game.boosting.BOOSTING;
using game.boosting.BValue;
using game.boosting.BoostSpec;
using game.boosting.Booster;
using game.boosting.BoosterValue;
using game.faction.FACTIONS;
using game.faction.diplomacy.DIP;
using game.time.TIME;
using init.constant.Config;
using init.race.Race;
using init.sprite.UI.Icon;
using init.sprite.UI.UI;
using init.type.HCLASS;
using init.type.HCLASS_RACE;
using settlement.entity.humanoid.Humanoid;
using settlement.main.SETT;
using settlement.room.military.training.ROOM_M_TRAINER;
using settlement.stats.Induvidual;
using settlement.stats.STATS;
using settlement.stats.StatsInit;
using settlement.stats.StatsInit.StatDisposable;
using settlement.stats.standing.StatStanding;
using settlement.stats.stat.STAT;
using settlement.stats.stat.STATData;
using settlement.stats.stat.STATFake;
using settlement.stats.stat.STATFakeRace;
using settlement.stats.stat.StatCollection;
using settlement.stats.stat.StatInfo;
using settlement.stats.stat.StatObject;
using settlement.stats.util.StatBooster;
using snake2d.util.misc.ACTION;
using snake2d.util.misc.CLAMP;
using snake2d.util.rnd.RND;
using snake2d.util.sets.ArrayList;
using snake2d.util.sets.KeyMap;
using snake2d.util.sets.LIST;
using snake2d.util.sets.SET;
using util.ui.UIBox;
using util.ui.UIBoxContainer;
using util.ui.UIBoxText;
using util.ui.UIBoxButton;
using util.ui.UIBoxList;
using util.ui.UIBoxSlider;
using util.ui.UIBoxProgressBar;
using util.ui.UIBoxCheckbox;
using util.ui.UIBoxDropdown;
using util.ui.UIBoxTextInput;
using util.ui.UIBoxTooltip;
using util.ui.UIBoxScrollbar;
using util.ui.UIBoxWindow;
using util.ui.UIBoxModal;
using util.ui.UIBoxConfirm;
using util.ui.UIBoxPrompt;
using util.ui.UIBoxAlert;
using util.ui.UIBoxError;
using util.ui.UIBoxWarning;
using util.ui.UIBoxSuccess;
using util.ui.UIBoxInfo;
using util.ui.UIBoxDebug;
using util.ui.UIBoxLog;
using util.ui.UIBoxConsole;
using util.ui.UIBoxCommand;
using util.ui.UIBoxCommandHistory;
using util.ui.UIBoxCommandAutocomplete;
using util.ui.UIBoxCommandHelp;
using util.ui.UIBoxCommandManual;
using util.ui.UIBoxCommandGuide;
using util.ui.UIBoxCommandTutorial;
using util.ui.UIBoxCommandExample;
using util.ui.UIBoxCommandReference;
using util.ui.UIBoxCommandSyntax;
using util.ui.UIBoxCommandParameters;
using util.ui.UIBoxCommandOptions;
using util.ui.UIBoxCommandFlags;
using util.ui.UIBoxCommandAliases;
using util.ui.UIBoxCommandDescription;
using util.ui.UIBoxCommandUsage;
using util.ui.UIBoxCommandExamples;
using util.ui.UIBoxCommandNotes;
using util.ui.UIBoxCommandSeeAlso;
using util.ui.UIBoxCommandSince;
using util.ui.UIBoxCommandDeprecated;
using util.ui.UIBoxCommandRemoved;
using util.ui.UIBoxCommandTodo;
using util.ui.UIBoxCommandBugs;
using util.ui.UIBoxCommandFixes;
using util.ui.UIBoxCommandImprovements;
using util.ui.UIBoxCommandChanges;
using util.ui.UIBoxCommandNew;
using util.ui.UIBoxCommandRemoved;
using util.ui.UIBoxCommandRenamed;
using util.ui.UIBoxCommandMoved;
using util.ui.UIBoxCommandSplit;
using util.ui.UIBoxCommandMerged;
using util.ui.UIBoxCommandReorganized;
using util.ui.UIBoxCommandRefactored;
using util.ui.UIBoxCommandOptimized;
using util.ui.UIBoxCommandCleaned;
using util.ui.UIBoxCommandFixed;
using util.ui.UIBoxCommandImproved;
using util.ui.UIBoxCommandUpdated;
using util.ui.UIBoxCommandEnhanced;
using util.ui.UIBoxCommandPolished;
using util.ui.UIBoxCommandRefined;
using util.ui.UIBoxCommandModernized;
using util.ui.UIBoxCommandSimplified;
using util.ui.UIBoxCommandStreamlined;
using util.ui.UIBoxCommandEfficientized;
using util.ui.UIBoxCommandAccelerated;
using util.ui.UIBoxCommandBoosted;
using util.ui.UIBoxCommandEnhanced;
using util.ui.UIBoxCommandOptimized;
using util.ui.UIBoxCommandImproved;
using util.ui.UIBoxCommandUpdated;
using util.ui.UIBoxCommandEnhanced;
using util.ui.UIBoxCommandRefined;
using util.ui.UIBoxCommandModernized;
using util.ui.UIBoxCommandSimplified;
using util.ui.UIBoxCommandStreamlined;
using util.ui.UIBoxCommandEfficientized;
using util.ui.UIBoxCommandAccelerated;
using util.ui.UIBoxCommandBoosted;

namespace settlement.stats
{
    public class StatsBattle : StatCollection
    {
        public static StatsBattle instance;

        public StatBasicTraining basicTraining;
        public STAT enemyKills;
        public STAT combatExperience;
        public List<StatTraining> trainingStats;

        public StatsBattle(StatsInit init)
        {
            instance = this;

            basicTraining = new StatBasicTraining(init, "basicTraining", "Basic Training", "The basic training level of a unit.");

            enemyKills = new STATData(init, "enemyKills", "Enemy Kills", "Number of enemies killed by a unit.", init.count.newDataByte("enemyKills"));

            combatExperience = new STATData(init, "combatExperience", "Combat Experience", "Combat experience gained by a unit.", init.count.newDataByte("combatExperience"));

            trainingStats = new List<StatTraining>();
            foreach (var trainer in ROOM_M_TRAINER.all)
            {
                var statTraining = new StatTraining(init, trainer);
                trainingStats.Add(statTraining);
            }
        }

        public void MakeAKill(Humanoid a)
        {
            enemyKills.indu().inc(a.indu(), 1);
            combatExperience.indu().inc(a.indu(), 1 + RND.rInt(4));
            GAME.ARMIES().factors.reportKill(a);
        }

        public class StatBasicTraining : Stat
        {
            public StatBasicTraining(StatsInit init, string key, string name, string description) : base(init, key, name, description)
            {
            }

            public bool IsMax(Induvidual a)
            {
                return indu().getD(a) >= 15;
            }
        }

        public class StatTraining : Stat, MAPPED
        {
            private readonly INT_OE<Induvidual> count;
            public readonly ROOM_M_TRAINER room;
            public readonly int tIndex;
            public static readonly int MAX = 15;
            public static readonly double MAXI = 1.0 / MAX;

            public readonly STATData stat;

            public StatTraining(StatsInit init, ROOM_M_TRAINER room) : base(init, room.key, room.tInfo.name, room.tInfo.desc)
            {
                this.stat = new STATData(room.key, init, init.count.newDataNibble("BATTLE_TRAINING_" + room.key), new StatInfo(room.tInfo.name, room.tInfo.desc));

                count = init.count.newDataNibble("BATTLE_TCOUNT_" + room.key);
                this.room = room;
                tIndex = room.INDEX_TRAINING;
                BOOSTING.connecter(new ACTION
                {
                    exe = () =>
                    {
                        foreach (var b in room.boosters.all())
                        {
                            var bo = new BoosterValue(bvalue, b.booster.info, b.booster.to(), b.booster.isMul);
                            stat.boosters.push(bo, b.boostable);
                        }
                    }
                });

                stat.info().icon = room.icon.resized(Icon.S);
            }

            public bool ShouldTrain(Induvidual a, double target, bool training)
            {
                if (!basicTraining.IsMax(a))
                    return true;
                double t = target;
                double i = stat.indu().getD(a);
                if (t > i)
                    return true;
                else if (t < i)
                    return false;
                if (t > 0 && training && !count.isMax(a))
                    return true;
                return false;
            }

            public void Inc(Induvidual a, double am)
            {
                int sign = am < 0 ? -1 : 1;
                am = Math.Abs(am);
                am *= 0x0F * stat.indu().max(a);
                int iam = (int)am;
                if (RND.rFloat() < am - iam)
                    iam++;
                iam *= sign;

                int c = count.get(a) + iam;

                while (c >= count.max(a))
                {
                    if (stat.indu().isMax(a))
                    {
                        c = count.max(a);
                        break;
                    }
                    stat.indu().inc(a, 1);
                    c -= count.max(a);
                }
                while (c <= 0)
                {
                    stat.indu().inc(a, -1);
                    c += count.max(a);
                }
                count.set(a, c);
            }

            public int index()
            {
                return tIndex;
            }

            public string key()
            {
                return room.key;
            }

            public double bValue(double d)
            {
                d = CLAMP.d(d, 0, 1);
                return d;
            }

            public readonly BValue bvalue = new StatBooster
            {
                vGet = (HCLASS_RACE t) => bValue(stat.data(t.cl).getD(t.race)),
                vGet = (Div div) => bValue(stat.div().getD(div)),
                vGet = (Induvidual indu) => bValue(stat.indu().getD(indu))
            };
        }
    }
}