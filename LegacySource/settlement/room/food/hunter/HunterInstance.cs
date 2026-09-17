using System;
using System.Collections.Generic;

namespace Settlement.Room.Food.Hunter
{
    using static Settlement.Main.Sett.PATH;

    using Game.Time;
    using Settlement.Misc.Job;
    using Settlement.Misc.Util;
    using Settlement.Room.Industry.Module;
    using Settlement.Room.Main;
    using Snake2D.Renderer;
    using Snake2D.Util.Datatypes;
    using Snake2D.Util.Sets;
    using Util.Rendering;

    sealed class HunterInstance : RoomInstance, IRoomProducerInstance
    {
        private static readonly long SerialVersionUID = 1L;
        private readonly ArrayCooShort coos;
        private long[] pData;
        private short industry = 0;

        private double dSkill;
        private int iSkill;
        private float skill = 1;
        private float produce;

        public HunterInstance(ROOM_HUNTER blue, TmpArea area, RoomInit init)
            : base(blue, area, init)
        {
            pData = Industry().MakeData();

            int am = 0;

            foreach (COORDINATE c in Body())
            {
                if (Is(c) && blue.tile.Init(c.X, c.Y, this) != null)
                {
                    am++;
                }
            }

            coos = new ArrayCooShort(am);
            am = 0;
            foreach (COORDINATE c in Body())
            {
                if (Is(c) && blue.tile.Init(c.X, c.Y, this) != null)
                {
                    coos.Set(am++).Set(c);
                }
            }
            Employees().MaxSet(am);
            Employees().NeededSet(am);
            Activate();
        }

        protected override void LoadFix()
        {
            base.LoadFix();
            industry %= BlueprintI().Indus.Size;
            pData = Industry().MakeDataFix(pData);
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it)
        {
            it.Lit();
            return base.Render(r, shadowBatch, it);
        }

        protected override void ActivateAction()
        {
        }

        protected override void DeactivateAction()
        {
        }

        protected override void UpdateAction(double updateInterval, bool day)
        {
            Industry().UpdateRoom(this);

            if (!PATH().Finders.EntryPoints.AnyHas(Mx(), My()))
                return;

            if (iSkill > 0)
            {
                skill = (float)(dSkill / iSkill);
                dSkill = 0;
                iSkill = 0;
            }

            produce += TIME.SecondsPerDayI() * updateInterval * Employees().Employed() * skill;
        }

        protected override void Dispose()
        {
        }

        public ROOM_HUNTER BlueprintI()
        {
            return (ROOM_HUNTER)Blueprint();
        }

        public RESOURCE_TILE ResourceTile(int tx, int ty)
        {
            return null;
        }

        public long[] ProductionData()
        {
            return pData;
        }

        public int IndustryI()
        {
            return industry;
        }

        public Industry Industry()
        {
            return BlueprintI().Indus[industry];
        }

        public void SetIndustry(int i)
        {
            if (i == industry)
                return;

            Industry inIndustry = BlueprintI().Industries[i];
            if (inIndustry == null)
                return;
            pData = inIndustry.MakeData();
            industry = (byte)i;
            iSkill = 0;
            dSkill = 0;
            skill = 0;
            produce = 0;
        }

        public JOB_MANAGER GetWork()
        {
            return null;
        }
    }
}