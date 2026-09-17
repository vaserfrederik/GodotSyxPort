using System;
using init.constant;
using init.race;
using settlement.entity.humanoid.spirte;
using settlement.main;
using settlement.misc.util;
using settlement.thing.ThingsCorpses;
using snake2d;
using snake2d.util.bit;
using util.rendering;

namespace settlement.room.spirit.dump
{
    class Dump : FSERVICE
    {
        private static readonly Bit isBit = new Bit(0x00000001);
        private static readonly Bit reservedBit = new Bit(0x00000002);
        private static readonly Bits timeBits = new Bits(0x000000FC);
        private static readonly Bits raceBits = new Bits(0x0000FF00);
        private static readonly Bit activeBit = new Bit(0x00010000);

        private static Dump self = new Dump();

        private int tx, ty, data;
        private DumpInstance ins;

        private Dump()
        {
        }

        static void Init(DumpInstance ins, int tx, int ty)
        {
            SETT.ROOMS().data.Set(ins, tx, ty, isBit.Set(0));
        }

        static void Activate(int tx, int ty)
        {
            if (Get(tx, ty) != null)
            {
                self.data = activeBit.Set(self.data);
                self.data = reservedBit.Clear(self.data);
                self.Save();
            }
        }

        static void Deactivate(int tx, int ty)
        {
            if (Get(tx, ty) != null)
            {
                self.data = activeBit.Clear(self.data);
                self.data = reservedBit.Clear(self.data);
                self.Save();
            }
        }

        static int DaysTillDecompose(int tx, int ty)
        {
            if (Get(tx, ty) != null)
            {
                return timeBits.Get(self.data);
            }
            return 0;
        }

        static Dump Get(int tx, int ty)
        {
            self.ins = SETT.ROOMS().DUMP.getter.Get(tx, ty);
            if (self.ins != null)
            {
                self.data = SETT.ROOMS().data.Get(tx, ty);
                if (isBit.Is(self.data))
                {
                    self.tx = tx;
                    self.ty = ty;
                    return self;
                }
            }
            return null;
        }

        private void Save()
        {
            int now = data;
            data = SETT.ROOMS().data.Get(tx, ty);
            ins.service().Report(this, ins.blueprintI().service(), -1);
            data = now;
            ins.service().Report(this, ins.blueprintI().service(), 1);
            SETT.ROOMS().data.Set(ins, tx, ty, data);
        }

        public override bool FindableReservedCanBe()
        {
            return activeBit.Is(data) && timeBits.Get(data) == 0 && !reservedBit.Is(data);
        }

        public override void FindableReserve()
        {
            if (!FindableReservedCanBe())
                throw new Exception();
            data = reservedBit.Set(data);
            Save();
        }

        public override bool FindableReservedIs()
        {
            return activeBit.Is(data) && reservedBit.Is(data);
        }

        public override void FindableReserveCancel()
        {
            data = reservedBit.Clear(data);
            Save();
        }

        public override int X()
        {
            return tx;
        }

        public override int Y()
        {
            return ty;
        }

        public override void Consume()
        {
            throw new Exception();
        }

        void Bury(Corpse corpse)
        {
            bool a = activeBit.Is(data);
            data = isBit.Set(0);
            data = raceBits.Set(data, corpse.indu().race().index);
            data = timeBits.Set(data, 16);
            data = reservedBit.Clear(data);
            data = activeBit.Set(data, a);
            Save();
        }

        void Update()
        {
            if (timeBits.Get(data) > 0)
            {
                data = timeBits.Inc(data, -1);
                if (timeBits.Get(data) == 0)
                    data = reservedBit.Clear(data);
                Save();
            }
        }

        static void Render(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            int data = SETT.ROOMS().data.Get(i.tile());
            if (isBit.Is(data))
            {
                int t = timeBits.Get(data);
                if (t > 0)
                {
                    double deg = 0.5 + (16 - t) / 8.0;
                    Race rr = RACES.all().Get(raceBits.Get(data));
                    int ran = i.ran();
                    int di = ran & 0x07;
                    ran = ran >> 3;
                    int dx = -4 + (ran & 0x07);
                    ran = ran >> 4;
                    int dy = -4 + (ran & 0x07);
                    ran = ran >> 4;

                    if (deg > 1.0)
                        HCorpseRenderer.renderSkelleton(rr, true, di, false, r, shadowBatch, ran, i.x() + dx * C.SCALE, i.y() + dy * C.SCALE);
                    else
                        HCorpseRenderer.renderDump(rr, deg, di, r, shadowBatch, ran, i.x() + dx * C.SCALE, i.y() + dy * C.SCALE);
                }
            }
        }
    }
}