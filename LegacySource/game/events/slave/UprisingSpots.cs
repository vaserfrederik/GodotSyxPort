using System;
using System.Collections.Generic;
using System.Linq;
using game;
using game.battle.div;
using init.constant;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.room.main.throne;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;

namespace game.events.slave
{
    public sealed class UprisingSpots
    {
        private ArrayList<UprisingSpot> spots = new ArrayList<UprisingSpot>(Config.battle().DIVISIONS_PER_ARMY);
        private int divI;
        private int inposition;
        private readonly Coo position = new Coo();

        private int[] amountsTotal = Alloc.Ii(RACES.all().size());
        private int[] signedUps = Alloc.Ii(RACES.all().size());

        public UprisingSpots()
        {
        }

        public int Riot(double amountD)
        {
            clear();

            if (!UprisingSpot.SetStart(position, THRONE.Coo().X, THRONE.Coo().Y, 128))
                return 0;

            int amountTotal = 0;
            GAME.BATTLE_THREADS().Pause();
            for (int ri = 0; ri < RACES.all().size(); ri++)
            {
                Race r = RACES.all().Get(ri);
                int am = (int)(STATS.POP().Pop(r, HTYPES.SLAVE()) * amountD);

                if (am > 0)
                {
                    int sp = (int)Math.Ceiling((double)am / Config.battle().MEN_PER_DIVISION);
                    sp = CLAMP.i(sp, 0, spots.max());

                    int menPerSpot = (int)Math.Ceiling((double)am / sp);
                    for (int i = 0; i < sp && spots.size() < Config.battle().DIVISIONS_PER_ARMY; i++)
                    {
                        int a = CLAMP.i(menPerSpot, 0, am);
                        am -= a;

                        UprisingSpot s = UprisingSpot.Make(position.x, position.y, a, r);
                        amountsTotal[s.race] += s.amountTotal;
                        if (s != null)
                        {
                            spots.add(s);
                            amountTotal += s.amountTotal;
                        }
                    }
                }
            }

            GAME.BATTLE_THREADS().Unpause(true);

            return amountTotal;
        }

        public bool HasMore()
        {
            return spots.size() > 0;
        }

        public void Save(FilePutter file)
        {
            file.i(divI);
            file.i(0);
            file.i(0);
            file.i(inposition);
            position.Save(file);
            file.i(spots.size());
            foreach (UprisingSpot s in spots)
                s.Save(file);
        }

        protected void Load(FileGetter file)
        {
            divI = file.i();
            file.i();
            file.i();
            inposition = file.i();
            position.Load(file);
            int am = file.i();
            spots.clear();
            for (int i = 0; i < am; i++)
            {
                spots.add(UprisingSpot.Make(file));
            }
            amountsTotal.Fill(0);
            signedUps.Fill(0);
            foreach (UprisingSpot s in spots)
            {
                amountsTotal[s.race] += s.amountTotal;
                signedUps[s.race] += s.signedUp;
            }
        }

        protected void Clear()
        {
            spots.clear();
            divI = 0;
            amountsTotal.Fill(0);
            signedUps.Fill(0);
            inposition = 0;
        }

        public bool ShouldSignUpUpriser(Humanoid h)
        {
            return signedUps[h.race().index()] < amountsTotal[h.race().index()];
        }

        public int SignUpUpriserPositionByte(Humanoid h)
        {
            for (int i = 0; i < spots.size(); i++)
            {
                if (spots.get(i).race == h.race().index() && spots.get(i).signedUp < spots.get(i).amountTotal)
                {
                    spots.get(i).signedUp++;
                    signedUps[spots.get(i).race]++;
                    return i;
                }
            }
            return -1;
        }

        public bool ConfirmUpriser(int positionByte)
        {
            if (positionByte < 0 || positionByte >= spots.size())
                return false;
            return spots.get(positionByte).signedUp <= spots.get(positionByte).amountTotal;
        }

        public void ReportUpriserInPosition(int positionByte)
        {
            if (positionByte < 0 || positionByte >= spots.size())
                return;
            inposition++;
        }

        public void CancelUpriser(Humanoid h, int positionByte, bool inPosition)
        {
            if (positionByte < 0 || positionByte >= spots.size())
                return;
            spots.get(positionByte).signedUp--;
            signedUps[h.race().index]--;
            if (inPosition)
            {
                inposition--;
            }
        }

        public COORDINATE GetUpriserTile(int positionByte)
        {
            return spots.get(positionByte);
        }

        private double dri = 0;

        public bool Update(double ds)
        {
            dri += ds;

            if (dri < 1)
            {
                return false;
            }

            for (int i = 0; i < spots.size(); i++)
            {
                spots.get(i).Validate();
            }

            dri -= 1;

            foreach (Race r in RACES.all())
            {
                if (amountsTotal[r.index] > STATS.POP().Pop(r, HTYPES.SLAVE()))
                {
                    for (int i = 0; i < spots.size(); i++)
                    {
                        if (spots.get(i).race == r.index() && spots.get(i).amountTotal > 0)
                        {
                            spots.get(i).amountTotal--;
                            amountsTotal[r.index]--;
                        }
                    }
                }
            }

            int amTot = 0;
            for (int i = 0; i < spots.size(); i++)
            {
                amTot += spots.get(i).amountTotal;
            }
            if (inposition >= amTot)
            {
                GAME.ARMIES().factors.Init(GAME.ARMIES().enemy(), 1.0);
                while (NextDivision() && spots.size() > 0)
                {
                    Div d = GAME.ARMIES().enemy().divisions().Get(divI);
                    spots.RemoveLast().MakeDiv(d, spots.size());
                }
                clear();
                GAME.ARMIES().factors.Init(GAME.ARMIES().enemy(), 1.0);
                return true;
            }

            return false;
        }

        private bool NextDivision()
        {
            if (divI >= Config.battle().DIVISIONS_PER_ARMY)
            {
                return false;
            }
            while (GAME.ARMIES().enemy().divisions().Get(divI).menNrOf() > 0)
            {
                divI++;

                if (divI >= Config.battle().DIVISIONS_PER_ARMY)
                {
                    return false;
                }
            }
            return true;
        }
    }
}