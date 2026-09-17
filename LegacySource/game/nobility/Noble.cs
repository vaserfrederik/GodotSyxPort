using System;
using System.Collections.Generic;
using System.IO;

namespace Game.Nobility
{
    public class Noble : INDEXED
    {
        public readonly short index;
        private int subjectID = -1;
        private int office = -1;
        private int rank = 0;

        Noble(ArrayList<Noble> ii)
        {
            index = (short)ii.Add(this);
        }

        public Humanoid Subject()
        {
            if (subjectID == -1)
                return null;
            ENTITY e = SETT.ENTITIES().GetByID(subjectID);
            if (e != null && e is Humanoid)
            {
                return (Humanoid)e;
            }
            else
            {
                subjectID = -1;
                return null;
            }
        }

        void RankInc()
        {
            if (rank < GAME.NOBLE().MaxRanks() - 1)
            {
                rank++;
            }
        }

        void Assign(Humanoid h)
        {
            Saver.Clear();
            subjectID = h.ID();
            Update(0);
        }

        void SetOffice(NobleOffice office)
        {
            this.office = office == null ? -1 : office.Index;
        }

        void Update(double ds)
        {
            if (Subject() != null)
            {

            }
        }

        readonly SAVABLE Saver = new SAVABLE
        {
            Save = (file) =>
            {
                file.Write(subjectID);
                file.Write(rank);
                file.Write(office);
            },
            Load = (file) =>
            {
                subjectID = file.Read();
                rank = file.Read();
                office = file.Read();
            },
            Clear = () =>
            {
                subjectID = -1;
                rank = 0;
                office = -1;
            }
        };

        public NobleOffice Office()
        {
            if (office < 0 || office >= GAME.NOBLE().OFFICES.Count)
                return null;
            return GAME.NOBLE().OFFICES[office];
        }

        public string Title()
        {
            NobleOffice n = Office();
            if (n == null)
                return RankName();
            return n.Name;
        }

        public int Rank()
        {
            return rank;
        }

        public void HoverOffice(GUI_BOX box)
        {
            NobleOffice o = Office();
            if (o == null)
                return;
            GBox b = (GBox)box;

            b.Title(o.Name);
            b.Text(o.Desc);
            b.NL();

            o.HoverValue(b, 1 + rank * NOBLES.RANK_INCREASE);
        }

        public string RankName()
        {
            return GAME.NOBLE().NameRanks[rank];
        }

        public override int Index()
        {
            return index;
        }
    }
}