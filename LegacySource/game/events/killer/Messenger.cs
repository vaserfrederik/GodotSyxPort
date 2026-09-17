using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.events.killer
{
    class Messenger
    {
        private readonly Insert[] ins;

        private readonly M mFirst;
        private readonly M mSecond;
        private readonly M mAgain;
        private readonly M mSuspect;
        private readonly M mSuspectFail;
        private readonly M mSuspectSuccess;
        private readonly M mColdCase;

        public Messenger()
        {
            Json js = new Json(PATHS.TEXT_MISC().getFolder("serialKiller").gets("_INFO"));
            mFirst = new M(js.json("FIRST"));
            mSecond = new M(js.json("SECOND"));
            mAgain = new M(js.json("AGAIN"));
            mSuspect = new M(js.json("SUSPECT"));
            mSuspectFail = new M(js.json("SUSPECT_FAIL"));
            mSuspectSuccess = new M(js.json("CAUGHT"));
            mColdCase = new M(js.json("COLD"));


            ins = new Insert[] {
                new Insert("VICTIM_NAME") {
                    public override void set(Data t, Str str) {
                        str.add(STATS.APPEARANCE().name(t.victim.indu()));
                    }
                },
                new Insert("VICTIM_AGE") {
                    public override void set(Data t, Str str) {
                        str.add((int)Math.Ceiling(STATS.POP().age.years.getD(t.victim.indu())));
                    }
                },
                new Insert("VICTIM_RACE") {
                    public override void set(Data t, Str str) {
                        str.add(t.race.info.name);
                    }
                },
                new Insert("KILLER_SIGNATURE") {
                    public override void set(Data t, Str str) {
                        str.add(t.type.method);
                    }
                },
                new Insert("KILLER_ALIAS") {
                    public override void set(Data t, Str str) {
                        str.add(t.type.name);
                    }
                },
                new Insert("KILLER_NAME") {
                    public override void set(Data t, Str str) {
                        str.add(STATS.APPEARANCE().name(t.killer.indu()));
                    }
                },
                new Insert("SUSPECT_NAME") {
                    public override void set(Data t, Str str) {
                        CharSequence s = t.suspect != null ? STATS.APPEARANCE().name(t.suspect.indu()) : Dic.empty;
                        str.add(s);
                    }
                },
            };
        }

        public void murder(Data data)
        {
            M m = mFirst;
            if (data.murders == 2)
                m = mSecond;
            else if (data.murders > 2)
                m = mAgain;

            Str s = Str.TMP;
            s.clear().add(m.body);

            foreach (Insert i in ins)
            {
                while (i.insert(data, s))
                    ;
            }

            MessageText mm = new MessageText(m.title).paragraph(s);
            s.clear();
            s.add('"').add(data.type.messages[data.murders - 1]).add('"').NL().s().s().s().s().add('-').add(data.type.name);
            mm.paragraph(s);
            mm.send();
        }

        public void murderSuspect(Data data)
        {
            M m = mFirst;
            if (data.murders == 2)
                m = mSecond;
            else if (data.murders > 2)
                m = mAgain;

            Str s = Str.TMP;
            s.clear().add(m.body);

            foreach (Insert i in ins)
            {
                while (i.insert(data, s))
                    ;
            }

            string mess1 = "" + s;
            s.clear();
            s.add('"').add(data.type.messages[data.murders]).add('"').NL().s().s().s().s().add('-').add(data.type.name);
            string mess2 = "" + s;

            s.clear();
            s.add(mSuspect.body);
            foreach (Insert i in ins)
            {
                while (i.insert(data, s));
            }

            new mSuspect(m.title, mess1, mess2, "" + s, data.suspect.id(), data.murders, data.killer.id()).send();
        }

        public static class mSuspect : MessageSection
        {
            private readonly string mess1;
            private readonly string mess2;
            private readonly string quest;
            private readonly int suspect;
            private readonly int murders;
            private readonly int killer;

            mSuspect(CharSequence title, CharSequence mess1, CharSequence mess2, CharSequence quest, int suspect, int murders, int killer) : base(title)
            {
                this.mess1 = "" + mess1;
                this.mess2 = "" + mess2;
                this.quest = "" + quest;
                this.suspect = suspect;
                this.murders = murders;
                this.killer = killer;
            }

            protected override void make(GuiSection section)
            {
                paragraph(mess1);
                paragraph(mess2);
                paragraph(quest);

                section.addRelBody(16, DIR.S, new GButt.ButtPanel(Dic.¤¤Yes)
                {
                    protected override void clickA()
                    {
                        if (murders == GAME.events().killer.murders() && GAME.events().killer.theKiller() != null && GAME.events().killer.theKiller().id() == killer)
                            GAME.events().killer.setSuspect(suspect);
                        close();
                    }

                    protected override void renAction()
                    {
                        activeSet(GAME.events().killer.suspect() == -1 && murders == GAME.events().killer.murders() && GAME.events().killer.theKiller() != null && GAME.events().killer.theKiller().id() == killer);
                    }
                });

                section.addRelBody(16, DIR.S, new GButt.ButtPanel(Dic.¤¤No)
                {
                    protected override void clickA()
                    {
                        close();
                    }

                    protected override void renAction()
                    {
                        activeSet(GAME.events().killer.suspect() == -1 && murders == GAME.events().killer.murders() && GAME.events().killer.theKiller() != null && GAME.events().killer.theKiller().id() == killer);
                    }
                });
            }
        }

        public void over(Data data)
        {
            M m = mColdCase;
            Str s = Str.TMP;
            s.clear().add(m.body);

            foreach (Insert i in ins)
            {
                i.insert(data, s);
            }

            new MessageText(m.title).paragraph(s).send();
        }

        public void caught(Data data)
        {
            M m = mSuspectSuccess;
            Str s = Str.TMP;
            s.clear().add(m.body);

            foreach (Insert i in ins)
            {
                i.insert(data, s);
            }

            new MessageText(m.title).paragraph(s).send();
        }

        public void fail(Data data)
        {
            M m = mSuspectFail;
            Str s = Str.TMP;
            s.clear().add(m.body);

            foreach (Insert i in ins)
            {
                i.insert(data, s);
            }

            new MessageText(m.title).paragraph(s).send();
        }

        public static class Data
        {
            public int murders;
            public Humanoid suspect;
            public Humanoid killer;
            public Corpse victim;
            public Race race;
            public KillerType type;
        }

        private static abstract class Insert : StrInserter<Data>
        {
            public Insert(string key) : base(key) { }
        }

        private static class M
        {
            public readonly CharSequence title;
            public readonly CharSequence body;

            public M(Json json)
            {
                title = json.text("TITLE");
                body = json.text("BODY");
            }
        }
    }
}