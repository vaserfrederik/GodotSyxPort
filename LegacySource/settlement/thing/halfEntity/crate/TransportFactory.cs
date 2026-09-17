using System;
using System.IO;
using init.resources;
using settlement.entity.humanoid;
using settlement.thing.halfEntity;
using snake2d.util.file;
using snake2d.util.sets;

namespace settlement.thing.halfEntity.crate
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

        public void make(Humanoid h, int tx, int ty, RESOURCE res, byte ran, bool mil)
        {
            TransportEntity e = create();
            e.init(h, tx, ty, res, ran, mil);
        }
    }
}