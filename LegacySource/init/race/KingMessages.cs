using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Init.Race
{
    using Game.Faction;
    using Game.Faction.Diplomacy;
    using Game.Faction.NPC;
    using Init.Paths;
    using Settlement.Stats;
    using Snake2D.Log;
    using Snake2D.Util.File;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;
    using Snake2D.Util.Sprite.Text;
    using Util.Data;
    using Util.Text;
    using View.Interrupter;

    public class KingMessages
    {
        private readonly LinkedList<Message> all = new LinkedList<Message>();

        public readonly Message GREETING_GOOD;
        public readonly Message GREETING_BAD;
        public readonly Message STANCE_WARNING;
        public readonly Message STANCE_DOWN;
        public readonly Message STANCE_UP;
        public readonly Message PEACE_GOOD;
        public readonly Message PEACE_BAD;
        public readonly Message THREAT_NORMAL;
        public readonly Message THREAT_VASSAL;
        public readonly Message THREAT_ALLY;
        public readonly Message WAR_NORMAL;
        public readonly Message WAR_VASSAL;
        public readonly Message WAR_ALLY;
        public readonly Message WAR_JOIN_NORMAL;
        public readonly Message WAR_JOIN_VASSAL;
        public readonly Message WAR_JOIN_ALLY;

        public readonly LIST<CharSequence> COALITION_NAME;
        public readonly LIST<CharSequence> WAR_CAUSE_AGRESSION;
        public readonly LIST<CharSequence> WAR_CAUSE_DEFEND;
        public readonly LIST<CharSequence> RUMOUR;
        public readonly LIST<CharSequence> NGO;

        public static KingMessages Make(Json data, ExpandInit init)
        {
            string key = data.Value("KING_FILE");
            if (!init.KMessages.ContainsKey(key))
            {
                KingMessages m = new KingMessages(new Json(PATHS.RACE().Text.GetFolder("king").Gets(key)));
                init.KMessages.Add(key, m);
                IDebugPanel.Add("King message test: " + key, new ACTION()
                {
                    public void Exe()
                    {
                        FactionNPC f = FACTIONS.NPCs().Rnd();
                        KingMessages m = f.Court().King().Roy().Induvidual.Race().KingMessage();

                        foreach (Message me in m.all)
                        {
                            LOG.Ln(me.key);
                            for (int i = 0; i < me.all.Length; i++)
                            {
                                LOG.Ln(me.Get(f, i));
                            }
                            LOG.Ln();
                        }
                    }
                });
            }

            return init.KMessages[key];
        }

        public KingMessages(Json j)
        {
            GREETING_GOOD = new Message(j, "GREETING_GOOD");
            GREETING_BAD = new Message(j, "GREETING_BAD");
            STANCE_UP = new Message(j, "STANCE_UP");
            STANCE_DOWN = new Message(j, "STANCE_DOWN");
            STANCE_WARNING = new Message(j, "STANCE_WARNING");
            PEACE_GOOD = new Message(j, "PEACE_GOOD");
            PEACE_BAD = new Message(j, "PEACE_BAD");

            THREAT_NORMAL = new Message(j, "THREAT_NORMAL");
            THREAT_VASSAL = new Message(j, "THREAT_VASSAL");
            THREAT_ALLY = new Message(j, "THREAT_ALLY");
            WAR_NORMAL = new Message(j, "WAR_NORMAL");
            WAR_VASSAL = new Message(j, "WAR_VASSAL");
            WAR_ALLY = new Message(j, "WAR_ALLY");
            WAR_JOIN_NORMAL = new Message(j, "WAR_JOIN_NORMAL");
            WAR_JOIN_VASSAL = new Message(j, "WAR_JOIN_VASSAL");
            WAR_JOIN_ALLY = new Message(j, "WAR_JOIN_ALLY");

            COALITION_NAME = new ArrayList<CharSequence>(j.Texts("COALITION_NAME"));
            WAR_CAUSE_AGRESSION = new ArrayList<CharSequence>(j.Texts("WAR_CAUSE_AGRESSION"));
            WAR_CAUSE_DEFEND = new ArrayList<CharSequence>(j.Texts("WAR_CAUSE_DEFEND"));
            RUMOUR = new ArrayList<CharSequence>(j.Texts("RUMOUR"));
            NGO = new ArrayList<CharSequence>(j.Texts("NGO"));
        }

        private static readonly Inserter<FactionNPC> insert = new Inserter<>();

        static KingMessages()
        {
            insert.Join(INSERT.Faction, new GETTER_TRANS<FactionNPC, Faction>()
            {
                public Faction Get(FactionNPC f)
                {
                    return f;
                }
            });

            insert.Join(new Inserter<Faction>(INSERT.Faction, "PLAYER_"), new GETTER_TRANS<FactionNPC, Faction>()
            {
                public Faction Get(FactionNPC f)
                {
                    return FACTIONS.Player();
                }
            });

            insert.Join(INSERT.Player, new GETTER_TRANS<FactionNPC, int>()
            {
                public int Get(FactionNPC f)
                {
                    return RND.RInt();
                }
            });

            insert.Join(INSERT.Indu, new GETTER_TRANS<FactionNPC, Induvidual>()
            {
                public Induvidual Get(FactionNPC f)
                {
                    return f.Court().King().Roy().Induvidual;
                }
            });

            insert.NewII("WAR_CAUSE", new II()
            {
                public void Set(FactionNPC t, Str str)
                {
                    str.Add(DIP.WAR_PLAYER().WarName);
                }
            });

            insert.NewII("COALITION", new II()
            {
                public void Set(FactionNPC t, Str str)
                {
                    str.Add(DIP.WAR_PLAYER().TeamName);
                }
            });
        }

        private FactionNPC debug;

        private static readonly Str TMP = new Str(250);

        public class Message
        {
            private readonly CharSequence[] all;
            public readonly string key;

            Message(Json j, string key)
            {
                all = insert.Check(j.Texts(key));
                KingMessages.this.all.Add(this);
                this.key = key;
            }

            public CharSequence Get(FactionNPC f)
            {
                return Get(f, RND.RInt(all.Length));
            }

            private CharSequence Get(FactionNPC f, int mi)
            {
                TMP.Clear();
                TMP.Add(all[mi]);

                insert.Set(TMP, f);

                return TMP;
            }

            private ACTION da = new ACTION()
            {
                int ii = 0;

                public void Exe()
                {
                    CharSequence m = Get(debug, ii);
                    ii++;
                    if (ii >= all.Length)
                        ii = 0;
                    LOG.Ln(m);
                }
            };
        }

        public void Debug(Debugger d, FactionNPC f)
        {
            debug = f;

            d.Title(KingMessages.class.Name);
            foreach (Message m in all)
            {
                d.Debug(m.key, m.da);
            }
        }
    }
}