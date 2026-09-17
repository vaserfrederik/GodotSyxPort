using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace settlement.room.main.employment
{
    public class RoomEquips
    {
        public readonly List<RoomEquip> ALL;
        private readonly List<List<RoomEquip>> perRoom;

        public RoomEquips(ROOMS rooms, RoomEmployments emps)
        {
            BoostableCat cat = new BoostableCat("EQUIP_", Dic.¤¤Equipment, Dic.¤¤Equipment, BoostableCat.TYPE_CRAP, UI.icons().s.house);
            List<RoomEquip> all = new List<RoomEquip>();

            PATH p = PATHS.INIT().getFolder("resource").getFolder("work");

            foreach (string k in p.getFiles())
            {
                new RoomEquip(k, all, emps, new Json(p.gets(k)), rooms, cat);
            }

            this.ALL = all;

            perRoom = new List<List<RoomEquip>>(emps.ALLS().Count);
            for (int i = 0; i < emps.ALLS().Count; i++)
                perRoom.Add(new List<RoomEquip>());

            foreach (RoomEquip t in all)
            {
                foreach (RoomEmploymentSimple e in emps.ALLS())
                {
                    if (t.target(e).max > 0)
                        perRoom[e.eindex()].Add(t);
                }
            }
        }

        public Target boostToTarget(Boostable bo)
        {
            if (bo.index() >= Target.boos[0].boost().index() && bo.index() < Target.boos[Target.boos.Count - 1].boost().index())
            {
                return Target.boos[bo.index() - Target.boos[0].boost().index()];
            }
            return null;
        }

        public List<RoomEquip> get(RoomEmploymentSimple e)
        {
            return perRoom[e.eindex()];
        }

        public readonly SAVABLE saver = new SAVABLE()
        {
            public void save(FilePutter file)
            {
                file.i(ALL.Count);
                foreach (RoomEquip t in ALL)
                    t.saver.save(file);
            }

            public void load(FileGetter file) throws IOException
            {
                int am = file.i();

                if (am != ALL.Count)
                {
                    for (int i = 0; i < am; i++)
                    {
                        ALL[0].saver.load(file);
                    }
                    clear();
                }
                else
                {
                    foreach (RoomEquip t in ALL)
                        t.saver.load(file);
                }
            }

            public void clear()
            {
                foreach (RoomEquip t in ALL)
                    t.saver.clear();
            }
        };
    }
}