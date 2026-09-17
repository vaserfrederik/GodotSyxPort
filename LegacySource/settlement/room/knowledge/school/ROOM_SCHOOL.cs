using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Knowledge.School
{
    public class RoomSchool : RoomBlueprintIns<SchoolInstance>, INDUSTRY_HASER, ROOM_SERVICE_HASER
    {
        private readonly Industry _industry;
        private readonly SchoolConstructor _constructor;
        private readonly RoomService _service;
        private readonly SchoolStation _station = new SchoolStation(this);
        private readonly List<Industry> _indus;

        private readonly RoomEducationHelper _helper;

        public RoomSchool(string key, int index, RoomInitData init, RoomCategorySub block) : base(index, init, key, block)
        {
            _service = new RoomService(this, init, null)
            {
                Service = (tx, ty) => _station.Service(tx, ty)
            };
            PushBo(init.data(), Type, true);
            _constructor = new SchoolConstructor(this, init);

            _helper = new RoomEducationHelper(this, _constructor.Quality)
            {
                Type = () => STATS.EDUCATION().Child
            };

            _industry = new Industry(this, init.data(), null)
            {
                ConsumptionRate = (ins, h, oo) =>
                {
                    if (ins.Employees().Employed() == 0)
                        return 0;
                    double d = oo.Rate * _service.Load() * _service.Total() / ins.Employees().Employed();
                    return d;
                }
            };
            Employment().CountInputSet();

            _indus = new List<Industry> { _industry };
        }

        protected override void Update(double ds)
        {
        }

        public SFinderRoomService Service(int tx, int ty)
        {
            return _service.Finder;
        }

        protected override void SaveP(FilePutter saveFile)
        {
            _service.Saver.Save(saveFile);
            _industry.Save(saveFile);
        }

        protected override void LoadP(FileGetter saveFile)
        {
            _service.Saver.Load(saveFile);
            _industry.Load(saveFile);
        }

        protected override void ClearP()
        {
            _service.Saver.Clear();
            _industry.Clear();
        }

        public Furnisher Constructor()
        {
            return _constructor;
        }

        public void AppendView(List<UIRoomModule> mm)
        {
            _helper.AppendView(mm);
        }

        public List<Industry> Industries()
        {
            return _indus;
        }

        public RoomService Service()
        {
            return _service;
        }

        public DIR ChildDir(int sx, int sy)
        {
            return _station.ServiceDir(sx, sy);
        }

        public double LearningSpeed(Humanoid student, int tx, int ty)
        {
            return _helper.LearningSpeed(student, tx, ty);
        }

        public double IndustryFormatConsumptionRate(GText text, IndustryResource i, RoomInstance ins)
        {
            SchoolInstance sc = (SchoolInstance)ins;

            double d = i.Rate * sc.Service().Load() * sc.Service().Total();
            GFORMAT.f0(text, -d);
            return d;
        }
    }
}