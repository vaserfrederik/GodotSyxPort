using System;
using System.IO;
using settlement.thing.halfEntity;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;

namespace settlement.thing.halfEntity.transport
{
    public class TransportFactory : Factory<TransportEntity>
    {
        public readonly Sprite sprite;

        public TransportFactory(LISTE<Factory<?>> all) : base(all)
        {
            sprite = new Sprite();
        }

        protected override void save(FilePutter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void load(FileGetter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void clear()
        {
            // TODO Auto-generated method stub
        }

        protected override TransportEntity make()
        {
            return new TransportEntity();
        }

        public bool military(int tx, int ty, byte ran, RESOURCE res, int ramount, DIR d)
        {
            TransportEntity e = create();
            return e.initMilitary(tx, ty, ran, res, ramount, d);
        }

        /**
         * 
         * @param tx
         * @param ty
         * @param ran
         * @param res
         * @param ramount
         * @param d
         * @param station
         * @return -1 if unsuccessful, else distance travelled
         */
        public bool loader(int tx, int ty, byte ran, RESOURCE res, int ramount, DIR d, COORDINATE station)
        {
            TransportEntity e = create();
            return e.initStation(tx, ty, ran, res, ramount, d, station);
        }
    }
}