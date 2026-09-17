using System;
using System.IO;
using game.debug;
using init.type;
using settlement.main;
using snake2d.util.file;

namespace settlement.environment
{
    public sealed class ENVIRONMENT : SettResource
    {
        private CLIMATE climate = CLIMATES.COLD();
        public readonly SettEnvMap map = new SettEnvMap();
        public readonly Foundation foundation;

        public ENVIRONMENT() : base("ENVIRONMENT", true)
        {
            foundation = new Foundation();
        }

        protected override void generate(CapitolArea area)
        {
            this.climate = area.climate();
            foundation.generate();
        }

        protected override void save(FilePutter file)
        {
            file.i(climate.index());
            foundation.saver.save(file);
        }

        protected override void update(double ds, Profiler profiler)
        {
            map.update(ds);
        }

        protected override void init(bool loaded)
        {
            map.init();
        }

        protected override void load(FileGetter file)
        {
            climate = CLIMATES.ALL()[file.i()];
            foundation.saver.load(file);
        }

        public CLIMATE climate()
        {
            return climate;
        }
    }
}