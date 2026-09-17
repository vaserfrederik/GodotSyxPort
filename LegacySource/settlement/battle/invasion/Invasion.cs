using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settlement.Battle.Invasion
{
    public class Invasion
    {
        private static readonly string ¤¤Bombardment = "¤Bombardment";
        private static readonly string ¤¤BombardmentD = "¤The enemy has started bombarding us to clear a path. There is nothing we can do but take cover until they are done.";

        private static readonly string ¤¤Deployment = "¤Deployment";
        private static readonly string ¤¤DeploymentD = "¤The enemy are deploying their troops. May the gods help us! Quickly enable the battle view and counter them. They must not reach the throne!";

        private static readonly string ¤¤Retreat = "¤Retreat!";
        private static readonly string ¤¤RetreatD = "¤Enemy forces are weary of fighting and have retreated!";

        private static readonly string ¤¤LooseD = "¤It's over! The enemy forces have reached the throne, and have taken control of the city. They have sacked your treasury for {0} {1}, and collected {2}% of your warehouse stock.";
        private static readonly string ¤¤LooseDFaction = "¤The faction of {0} sends its regards. Now that you've bent the knee, they hope you've learned your lesson, and you are now at peace.";
        private static readonly string ¤¤Victory = "¤Our men have prevailed and our foe is beaten. Rejoice! The {0} survivors can be turned into prisoners, and need a stockade to stay in. Do you accept them? Declining will have the remaining enemies chased down and killed.";

        static Invasion()
        {
            D.ts(typeof(Invasion));
        }

        public readonly InvasionSpec spec;
        private STATE state = null;
        private int artillery;
        private double timer = 0;
        public readonly InvasionSpot spot;
        private List<short> activeDivs = new List<short>(Config.battle().maxDivisions);
        private bool victory;

        private int eDeaths;
        private int pLosses;

        public Invasion(InvasionSpec spec, InvasionSpot spot)
        {
            this.spec = spec;
            this.spot = spot;
            this.eDeaths = STATS.POP().COUNT.leaves().get(CAUSE_LEAVES.SLAYED().index()).statistics(HCLASSES.OTHER()).get(null);
            this.pLosses = STATS.POP().COUNT.leaves().get(CAUSE_LEAVES.SLAYED().index()).statistics(HCLASSES.CITIZEN()).get(null);
        }

        public void Save(BinaryWriter writer)
        {
            spec.Save(writer);
            spot.Save(writer);
            writer.Write((short)state);
            writer.Write(artillery);
            writer.Write(timer);
            writer.Write(activeDivs.Count);
            foreach (var div in activeDivs)
            {
                writer.Write(div);
            }
            writer.Write(victory);
            writer.Write(eDeaths);
            writer.Write(pLosses);
        }

        public static Invasion Load(BinaryReader reader)
        {
            var spec = InvasionSpec.Load(reader);
            var spot = InvasionSpot.Load(reader);
            var state = (STATE)reader.ReadInt16();
            var artillery = reader.ReadInt32();
            var timer = reader.ReadDouble();
            var activeDivsCount = reader.ReadInt32();
            var activeDivs = new List<short>(activeDivsCount);
            for (int i = 0; i < activeDivsCount; i++)
            {
                activeDivs.Add(reader.ReadInt16());
            }
            var victory = reader.ReadBoolean();
            var eDeaths = reader.ReadInt32();
            var pLosses = reader.ReadInt32();
            var invasion = new Invasion(spec, spot)
            {
                state = state,
                artillery = artillery,
                timer = timer,
                activeDivs = activeDivs,
                victory = victory,
                eDeaths = eDeaths,
                pLosses = pLosses
            };
            return invasion;
        }

        public void Update(double deltaTime)
        {
            switch (state)
            {
                case STATE.BOMBARD:
                    if (timer >= TIME.secondsPerDay / 8)
                    {
                        Launch();
                    }
                    break;
                case STATE.PLACEART:
                    if (timer >= 40)
                    {
                        Deploy();
                    }
                    break;
                case STATE.FIGHTING:
                    if (!Fight())
                    {
                        Resolve(victory);
                    }
                    break;
            }
            timer += deltaTime;
        }

        private void Launch()
        {
            state = STATE.PLACEART;
            timer = 0;
            // Additional logic for launching the bombardment
        }

        private void Deploy()
        {
            state = STATE.FIGHTING;
            timer = 0;
            // Additional logic for deploying troops
        }

        private bool Fight()
        {
            for (int i = 0; i < activeDivs.Count; i++)
            {
                var d = GAME.ARMIES().division(activeDivs[i]);
                if (!d.active() && d.menNrOf() == 0)
                {
                    activeDivs.RemoveAt(i);
                    i--;
                }
            }

            if (activeDivs.Count == 0)
            {
                var am = 0;
                foreach (var e in SETT.ENTITIES().getAllEnts())
                {
                    if (e is Humanoid h && SETT.PATH().reachability.is(h.tc()) && h.indu().hType() == HTYPES.ENEMY() && STATS.BATTLE().ROUTING.indu().get(h.indu()) != 0)
                    {
                        am++;
                        foreach (var eq in STATS.EQUIP().allE())
                        {
                            var eam = eq.get(h.indu());
                            if (eam > 0)
                            {
                                SETT.THINGS().resources.create(h.tc(), eq.resource, eam);
                                eq.set(h.indu(), 0);
                            }
                        }
                    }
                }

                var message = string.Format(¤¤Victory, am);
                var yes = new ACTION(() =>
                {
                    foreach (var e in SETT.ENTITIES().getAllEnts())
                    {
                        if (e is Humanoid h && SETT.PATH().reachability.is(h.tc()) && h.indu().hType() == HTYPES.ENEMY() && STATS.BATTLE().ROUTING.indu().get(h.indu()) != 0)
                        {
                            h.HTypeSet(HTYPES.PRISONER(), null, null);
                            STATS.BATTLE().ROUTING.indu().set(h.indu(), 0);
                        }
                    }
                    state = STATE.DONE;
                });

                var no = new ACTION(() =>
                {
                    state = STATE.DONE;
                });

                VIEW.inters().yesNo.activate(message, yes, no, false);
                Resolve(true);
                return false;
            }
            else if (GAME.ARMIES().enemy().men() > 0)
            {
                var c = THRONE.coo();
                for (int x = c.x - 1; x < c.x + 2; x++)
                {
                    for (int y = c.y - 1; y < c.y + 2; y++)
                    {
                        foreach (var e in SETT.ENTITIES().getAtTile(x, y))
                        {
                            if (e is Humanoid h && h.indu().hType() == HTYPES.ENEMY() && h.division() != null)
                            {
                                victory = false;
                                Loose();
                                Resolve(false);
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void Resolve(bool victory)
        {
            this.victory = victory;
            var eDeaths = STATS.POP().COUNT.leaves().get(CAUSE_LEAVES.SLAYED().index()).statistics(HCLASSES.OTHER()).get(null) - this.eDeaths;
            var pLosses = STATS.POP().COUNT.leaves().get(CAUSE_LEAVES.SLAYED().index()).statistics(HCLASSES.CITIZEN()).get(null) - this.pLosses;

            if (victory)
            {
                foreach (var ll in InvasionListener.all)
                {
                    ll.victory(pLosses, eDeaths, spec.ref);
                }
                GAME.count().INVASIONS_WON.inc(1);
            }
            else
            {
                foreach (var ll in InvasionListener.all)
                {
                    ll.defeat(pLosses, eDeaths, spec.ref);
                }
                GAME.count().INVASIONS_LOST.inc(1);
            }

            AD.stats().report(FACTIONS.player(), victory, pLosses, eDeaths);
        }

        private void Loose()
        {
            var am = 0.25 + 0.05 * RND.rInt(6);
            var creds = FACTIONS.player().credits().credits() > 0 ? (int)(FACTIONS.player().credits().credits() * 0.75) : 0;
            FACTIONS.player().credits().inc(-creds, CTYPE.MISC);

            var t1 = string.Format(¤¤LooseD, creds, Dic.¤¤Currs, (int)Math.Ceiling(100 * am));

            var m = new MessageText(Dic.¤¤Defeat, t1);

            var f = spec.fi < 0 ? null : (FactionNPC)FACTIONS.getByIndex(spec.fi);

            if (f != null && f.isActive())
            {
                m.paragraph(string.Format(¤¤LooseDFaction, f.name));
                ROPINION.STANCE().setNewStance(f, DIP.VASSAL(), false);
            }

            Remove();

            RESOURCE.remove(am, RBIT.ALL, RTYPE.SPOILS);

            m.send();
        }

        private void Remove()
        {
            check.init();
            for (int i = 0; i < activeDivs.Count; i++)
            {
                var di = activeDivs[i];
                check.isSetAndSet(di);
            }

            foreach (var e in SETT.ENTITIES().getAllEnts())
            {
                if (e is Humanoid h && h.division() != null && check.isSet(h.division().index()))
                {
                    h.helloMyNameIsInigoMontoyaYouKilledMyFatherPrepareToDie();
                }
            }
        }

        public Faction invador()
        {
            var f = spec.fi < 0 ? null : (FactionNPC)FACTIONS.getByIndex(spec.fi);

            if (f != null)
            {
                return f;
            }
            return FACTIONS.NPCs().get(1);
        }

        private enum STATE
        {
            BOMBARD,
            PLACEART,
            FIGHTING,
            DONE
        }
    }
}