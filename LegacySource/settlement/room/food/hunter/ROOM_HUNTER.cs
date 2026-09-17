using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Room.Food.Hunter
{
    public class RoomHunter : RoomBlueprintIns<HunterInstance>, INDUSTRY_HASER
    {
        public static readonly string Type = "HUNTER";

        private readonly Constructor _constructor;
        private readonly List<Industry> _industries;
        private readonly Tile _tile;

        public double Luck { get; private set; } = 1.0;
        private int _year = -1;

        public readonly int MaxEmployed;

        private static readonly string Emp = "The more employees you have, the less efficient this industry will become. The max amount for this room is {0}. Employees after that point will decrease the output gradually.";
        private static readonly string LuckStr = "Luck";
        private static readonly string LuckDesc = "How lucky your hunters are. Changes annually";

        static RoomHunter()
        {
            D.Ts(typeof(RoomHunter));
        }

        public readonly RoomBoost BEmployed;

        public RoomHunter(int index, RoomInitData init, string key, RoomCategorySub cat) : base(index, init, key, cat)
        {
            _constructor = new Constructor(this, init);
            PushBo(init.Data(), Type, true);
            MaxEmployed = init.Data().I("MAX_EMPLOYED", 1, 10000);

            BEmployed = new RoomBoost
            {
                Info = new INFO(Dic.Employees, "" + Str.TMP.Clear().Add(Emp).Insert(0, MaxEmployed))
            };
            BEmployed.Get = (r) => EBonus(0);

            RoomBoost BLuck = new RoomBoost
            {
                Info = new INFO(LuckStr, LuckDesc)
            };
            BLuck.Get = (r) => Luck;
            BLuck.Min = 0.6;
            BLuck.Max = 1.4;

            _industries = Industry.CreateIndustries(this, init, new RoomBoost[] { BEmployed, BLuck, _constructor.Efficiency }, Bonus());
            foreach (Industry i in _industries)
                i.IsOnlyRoomDoNotUse = true;
            _tile = new Tile(this);
        }

        public double EBonus(int delta)
        {
            double emp = Employment().Employed() + delta;
            if (emp < MaxEmployed)
                return 1.0;
            double d = 1 + (emp - MaxEmployed) / (MaxEmployed * 4);
            return 1.0 / d;
        }

        protected override void Update(double ds)
        {
            if (_year != TIME.Years().BitsSinceStart())
            {
                Luck = RND.RFloat1(0.4);
                _year = TIME.Years().BitsSinceStart();
            }
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return null;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            IndustryUtil.Save(saveFile, _industries);
            saveFile.D(Luck);
            saveFile.I(_year);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            IndustryUtil.Load(saveFile, _industries);
            Luck = saveFile.D();
            _year = saveFile.I();
        }

        protected override void ClearP()
        {
            IndustryUtil.Clear(_industries);
            Luck = 1;
            _year = -1;
        }

        public Furnisher Constructor()
        {
            return _constructor;
        }

        public void AppendView(List<UIRoomModule> mm)
        {
        }

        public List<Industry> Industries()
        {
            return _industries;
        }

        public void ResetGore(COORDINATE c)
        {
            _tile.Reset(Getter.Get(c), c);
        }

        public void Gore(COORDINATE c)
        {
            _tile.Gore(Getter.Get(c), c);
        }

        public COORDINATE ReserveWork(RoomInstance inss, Humanoid h)
        {
            COORDINATE start = h.TC();

            HunterInstance ins = (HunterInstance)inss;

            foreach (DIR d in DIR.ORTHO)
            {
                Tile j = _tile.Init(start.X() + d.X(), start.Y() + d.Y(), ins);
                if (j != null && j.Reserved.Get() == 0)
                    return Clean(j);
            }

            ArrayCooShort coos = ins.Coos;
            for (int i = 0; i < coos.Size(); i++)
            {
                coos.Inc();
                Tile j = _tile.Init(coos.Get().X(), coos.Get().Y(), ins);
                if (j.Reserved.Get() == 0)
                    return Clean(j);
            }

            return null;
        }

        private COORDINATE Clean(Tile j)
        {
            // Cadaver ca = SETT.THINGS().Cadavers.TGet.Get(j.Coo);
            // if (ca != null)
            //     ca.Remove();

            return j.Coo;
        }

        public void ReportSkill(RoomInstance inss, Humanoid h)
        {
            HunterInstance ins = (HunterInstance)inss;
            ins.DSkill += IndustryUtil.RoomBonus(ins, _industries[0]) * Bonus.Get(h.Indu());
            ins.ISkill++;
        }

        public bool Work(RoomInstance inss, COORDINATE work, Humanoid h, bool cadaver)
        {
            HunterInstance ins = Getter.Get(work);
            if (ins == null)
                return false;

            if (ins.Produce > 1)
            {
                double mm = 1 + ins.Produce / 10;
                mm = CLAMP.D(mm, 0, ins.Produce);
                ins.Produce -= mm;
                DIR dir = StoreDir(work);
                foreach (IndustryResource o in ins.Industry().Outs())
                {
                    int am = o.Inc(ins, mm * o.Rate);
                    if (am > 0)
                    {
                        SETT.THINGS().Resources.CreatePrecise(work.X() + dir.X(), work.Y() + dir.Y(), ins.Industry().Outs().Get(0).Resource, am);
                    }
                }
            }

            Tile j = _tile.Init(work.X(), work.Y(), ins);
            j.Cadaver.Set(ins, cadaver ? 1 : 0);

            return true;
        }

        public void WorkFinish(COORDINATE work)
        {
            HunterInstance ins = Getter.Get(work);
            if (ins == null)
                return;

            Tile j = _tile.Init(work.X(), work.Y(), ins);
            if (j != null)
            {
                j.Cadaver.Set(ins, 0);
                j.Reserved.Set(ins, 0);
            }
        }

        private DIR StoreDir(COORDINATE c)
        {
            foreach (DIR d in DIR.ORTHO)
            {
                if (SETT.ROOMS().FData.Tile.Get(c, d) == _constructor.RR)
                    return d;
            }
            return DIR.C;
        }
    }
}